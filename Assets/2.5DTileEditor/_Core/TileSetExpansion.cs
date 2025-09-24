using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Tile set expansions are a general way of incorporating special use cases, 
/// such as combining 2 tile sets, or having tiles that respond to their environment
/// in a unique way. This feature is currently on hold, since implementing it 
/// properly would require a large scale overhaul of the tile editor.
/// </summary>

[System.Serializable]
public class TileSetExpansion{

    public AdvancedTileSetup setup;

    public GameObject mainTile;
    public bool connectUp, connectLeft, connectRight, connectDown;

    public GameObject[] tiles = new GameObject[16];   
}

public enum AdvancedTileSetup {
    singleTile, alternateSet
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(TileSetExpansion))]
public class TileSetExpansionDrawer : PropertyDrawer {

    float baseWidth, height;
    private float x, y;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        AdvancedTileSetup setup = (AdvancedTileSetup)property.FindPropertyRelative("setup").enumValueIndex;

        int indent = EditorGUI.indentLevel;
        x = position.x + indent * 40f;
        y = position.y - height;
        baseWidth = position.width + 12f;
        height = 18f;

        EditorGUI.PropertyField(GetNextPropertyRect(), property.FindPropertyRelative("setup"), label);

        x += 20f;

        switch(setup) {
        case AdvancedTileSetup.singleTile:
        DrawProperty(property, "mainTile");
        DrawProperty(property, "connectUp");
        DrawProperty(property, "connectLeft");
        DrawProperty(property, "connectRight");
        DrawProperty(property, "connectDown");
        break;

        case AdvancedTileSetup.alternateSet:
        DrawProperty(property, "tiles");
        break;
        }
    }

    private void DrawProperty(SerializedProperty property, string name) {
        SerializedProperty nextProp = property.FindPropertyRelative(name);
        EditorGUI.PropertyField(GetNextPropertyRect(), nextProp, new GUIContent(name));

        if(nextProp.isArray) {
            nextProp.arraySize = 16;
            x += 20f;
            for(int i = 0; i < nextProp.arraySize; i++) {
                SerializedProperty p = nextProp.GetArrayElementAtIndex(i);
                GUIContent label = new GUIContent(((TileSet.TileType)i).ToString());
                EditorGUI.PropertyField(GetNextPropertyRect(), p, label);
            }
            x -= 20f;
        }
    }

    private Rect GetNextPropertyRect() {
        y += height;
        float width = baseWidth - x ;
        return new Rect(x, y, width, height);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        AdvancedTileSetup setup = (AdvancedTileSetup)property.FindPropertyRelative("setup").enumValueIndex;

        int elements = 1;

        switch(setup) {
        case AdvancedTileSetup.alternateSet:
        elements += 17;
        break;

        case AdvancedTileSetup.singleTile:
        elements += 5;
        break;
        }

        return elements * 18f;
    }
}

#endif