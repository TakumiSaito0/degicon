using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileSet))]
[CanEditMultipleObjects]
public class TileSetExt : Editor {

    public override void OnInspectorGUI() {
        TileSet tileSet = (TileSet)this.target;

        if(!tileSet.BasicTilesValid()) {
            GUILayout.Label("Basic tiles not assigned!");
            GUILayout.Label("Require 16 valid tiles.");
            GUILayout.Label("");
        }
        if(!tileSet.BackgroundTilesValid()) {
            GUILayout.Label("Background tiles not assigned!");
            GUILayout.Label("Require valid tile in each slot.");
            GUILayout.Label("");
        }
        if(!tileSet.BasicTilesValid() || !tileSet.BackgroundTilesValid()) {
            if(GUILayout.Button("Auto fill by name"))
                AutoFillBasicTiles();
            GUILayout.Label("");
        }
        if(!tileSet.SpecialTilesValid()) {
            GUILayout.Label("Special tiles invalid!");
            GUILayout.Label("Require valid special tile set, assign empty objects or reduce array size.");
            GUILayout.Label("");
        }
        if(tileSet.Valid()) {
            GUILayout.Label("Tileset ready!");
        }

        DrawDefaultInspector();
    }

    private void AutoFillBasicTiles() {
        TileSet tileSet = (TileSet)this.target;

        for(int i = 0; i < tileSet.transform.childCount; i++) {
            GameObject tile = tileSet.transform.GetChild(i).gameObject;
            string tileName = tile.name.ToLower();
            tileName = tileName.Replace('.', '_');
            tileName = tileName.Replace(' ', '_');

            //check if name matches basic tile
            for(int j = 0; j < 16; j++) {
                string matchName = ((TileSet.TileType)j).ToString().ToLower();

                if(tileName.Contains(matchName))
                    tileSet.basicTiles[j] = tile;
            }

            //check if name matches background tiles
            if(tileName.Contains("background")) {
                bool light = tileName.Contains("light");
                bool window = tileName.Contains("window");
                GameObject[] bgrt = tileSet.backgroundTiles;
                if(!light && !window)
                    tileSet.backgroundTiles = FillArray(bgrt, 0, tile);
                else if(!window)
                    tileSet.backgroundTiles = FillArray(bgrt, 1, tile);
                else if(!light)
                    tileSet.backgroundTiles = FillArray(bgrt, 2, tile);
            }
        }
    }

    /// <summary>
    /// Return array which has the same elements as the given array
    /// but with the given item inserted at the given spot. The array
    /// will be larger if necssary.
    /// </summary>
    private static T[] FillArray<T>(T[] array, int spot, T item) {
        if(array.Length > spot) {
            array[spot] = item;
            return array;
        }
        else {
            T[] newArray = new T[spot + 1];
            for(int i = 0; i < array.Length; i++)
                newArray[i] = array[i];
            newArray[spot] = item;
            return newArray;
        }
    }
}