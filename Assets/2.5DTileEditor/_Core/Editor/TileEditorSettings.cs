using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileEditorSettings : EditorWindow {

    //general constants
    private const int TOTAL_KEYS = 9;
    private const string BINDING_SAVE = "25DTiEdKyBnd";
    private const string BINDING_DEFAULT = "123456789"; 

    public enum Key {
        toggleNormal, fillRectNormal, clearRectNormal,
        toggleBackground, cycleBackground, clearBackground,
        cycleSpecial, fillRectBackgr, clearRectBackgr
    }

    //setting variables
    private static char[] keyMappings { get {
            if(_keyMappings == null)
                _keyMappings = LoadMappings();
            return _keyMappings;
    } }
    private static char[] _keyMappings;

    private static char[] LoadMappings() {
        return EditorPrefs.GetString(BINDING_SAVE, BINDING_DEFAULT).ToCharArray();
    }

    private static void SaveMappings(char[] map) {
        EditorPrefs.SetString(BINDING_SAVE, new string(map));
    }

    private char nextBinding;

    [MenuItem("Window/2.5D Tile Editor settings")]
    public static void OpenTileEditorSettings() {
        GetWindow<TileEditorSettings>().Show();
    }

    private void OnGUI() {
        if(Event.current.isKey) {
            char c = Event.current.character;
            if(c > 0)
                nextBinding = c;
        }

        Rect labelRect = new Rect(20f, 10f, 350f, 40f);
        GUI.Label(labelRect, "Press a key then click a button to change keybinding.");

        for(int i = 0; i < TOTAL_KEYS; i++) {
            float y = 40f + 22f * i;
            Rect rectButton = new Rect(50f, y, 200f, 20f);
            string line = GetKeyName((Key)i) + ": " + keyMappings[i];
            if(GUI.Button(rectButton, line) && nextBinding > 0) {
                keyMappings[i] = nextBinding;
                SaveMappings(keyMappings);
            }
        }
    }

    private string GetKeyName(Key key) {
        string toReturn = key.ToString();
        for(int i = 0; i < toReturn.Length; i++) {
            char c = toReturn.ToCharArray()[i];
            if(System.Char.IsUpper(c)) {
                toReturn = toReturn.Substring(0, i) + " " + 
                    System.Char.ToLower(c) + toReturn.Substring(i + 1);
            }
        }
        return toReturn;
    }

    public static char GetBinding(Key key) {
        return keyMappings[(int)key];
    }

    public static Key GetKey(char c) {
        char[] map = keyMappings;
        for(int i = 0; i < map.Length; i++) {
            if(map[i] == c)
                return (Key)i;
        }
        return (Key)(-1);
    }
}