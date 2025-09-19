using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TileEditor))]
[CanEditMultipleObjects]
public class TileEditorExt : Editor {

    //name string constants
    const string basicTileName = "Tile";
    const string bckgrTileName = "Background";
    const string spclTileName = "Special";
    const string basicParentName = "basicTileParent";
    const string bckgrParentName = "backgroundTileParent";

    const float SCENE_VIEW_SIZE_OFFSET = 27.5f;

    //tile hash tables, for increased performance
    Dictionary<TileCoords, Transform> basicTileDictionary, bckgrTileDictionary;

    //editor state variables
    bool building, deletingPrompt, largePlacementPrompt;
    List<List<TileDelta>> undoStack;
    List<TileDelta> nextDelta;
    int undoPosition;

    //problem flags
    bool tilesInParent, unkownObjectInParent, invalidObjectInTiles, tilesetMissing,
        tilesetInvalid, basicTileParentMissing, backgroundTileParentMissing;

    //tile editor children, needed for general functionality
    TileSet tileSet;
    Transform basicTileParent;
    Transform backgroundTileParent;

    //build input variables
    bool mouseInBuildPlane;
    int buildX, buildY;
    TileEditorSettings.Key dragKey; bool freezeDrag;
    int dragX1, dragX2, dragY1, dragY2;

    Transform transform
    {
        get
        {
            return ((TileEditor)this.target).transform;
        }
    }

    private void OnEnable() {
        building = false;
        deletingPrompt = false;
        CheckForProblems();
        if(!anyProblem)
            PrepareForBuilding();
    }

    /// <summary>
    /// Check if some problem is occuring, and switch problem flags accordingly.
    /// </summary>
    private void CheckForProblems() {
        //iterate trough child objects of tileEditor
        unkownObjectInParent = false;
        tilesInParent = false;
        tilesetMissing = true;
        basicTileParentMissing = true;
        backgroundTileParentMissing = true;
        for(int i = 0; i < transform.childCount; i++) {
            Transform t = transform.GetChild(i);

            //look for tile set
            if(t.GetComponent<TileSet>() != null && tilesetMissing) {
                tilesetMissing = false;
                tileSet = t.GetComponent<TileSet>();
            }

            //look for basic tile parent
            else if(t.name == basicParentName && basicTileParentMissing) {
                basicTileParentMissing = false;
                basicTileParent = t;
            }

            //look for background tile parent
            else if(t.name == bckgrParentName && backgroundTileParentMissing) {
                backgroundTileParentMissing = false;
                backgroundTileParent = t;
            }

            else {
                //found foreign object, check if it is a stray tile or something else
                if(IsLegalTileName(t.name))
                    tilesInParent = true;
                else
                    unkownObjectInParent = true;
            }
        }

        //check if tileset is valid, if we found one
        if(!tilesetMissing)
            tilesetInvalid = !tileSet.Valid();

        //check for invalid object in tile parents, if they're available
        invalidObjectInTiles = false;
        if(basicTileParentMissing || backgroundTileParentMissing)
            return;

        //check for invalid object in basic tiles.
        for(int i = 0; i < basicTileParent.childCount; i++) {
            string name = basicTileParent.GetChild(i).name;
            if(!IsLegalTileName(name))
                invalidObjectInTiles = true;
            else if(!IsBasic(name))
                invalidObjectInTiles = true;
        }

        //check for invalid object in background tiles.
        for(int i = 0; i < backgroundTileParent.childCount; i++) {
            string name = backgroundTileParent.GetChild(i).name;
            if(!IsLegalTileName(name))
                invalidObjectInTiles = true;
            else if(!IsBackgr(name))
                invalidObjectInTiles = true;
        }
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        //tileSet = (TileSet)EditorGUILayout.ObjectField(tileSet, typeof(TileSet), true);

        if(building) {
            if(deletingPrompt) {
                GUILayout.Label("Are you sure you want to delete all tiles?");
                if(GUILayout.Button("Delete all")) {
                    DeleteAllTiles();
                    deletingPrompt = false;
                }
                if(GUILayout.Button("Cancel"))
                    deletingPrompt = false;
            }
            else if(largePlacementPrompt) {
                int nTiles = (Mathf.Abs(dragX2 - dragX1) + 1) * (Mathf.Abs(dragY2 - dragY1) + 1);
                GUILayout.Label("You are about to place down ");
                GUILayout.Label(nTiles + " tiles");
                GUILayout.Label("Are you sure you want to do this?");
                if(GUILayout.Button("Cancel")) {
                    largePlacementPrompt = false;
                    dragKey = 0;
                    freezeDrag = false;
                }
                if(GUILayout.Button("Confirm")) {
                    largePlacementPrompt = false;
                    ApplyDragRectKey();
                    freezeDrag = false;
                }
            }
            else {
                //building normally
                GUILayout.Label(GetHotKey(1) + " toggle current tile");
                GUILayout.Label(GetHotKey(2) + " press twice to fill rectangle of tiles");
                GUILayout.Label(GetHotKey(3) + " press twice to clear rectangle of tiles");
                GUILayout.Label(GetHotKey(4) + " toggle (standard) background tile");
                GUILayout.Label(GetHotKey(5) + " cycle background tile");
                GUILayout.Label(GetHotKey(6) + " clear background tile");
                GUILayout.Label(GetHotKey(7) + " cycle trough special tiles");
                GUILayout.Label(GetHotKey(8) + " press twice to fill rectangle with standard background tiles");
                GUILayout.Label(GetHotKey(9) + " press twice to clear rectangle off background tiles");

                if(GUILayout.Button("Delete all"))
                    deletingPrompt = true;
                if(GUILayout.Button("Update all"))
                    UpdateAllTiles();

                if(undoPosition < undoStack.Count) {
                    if(GUILayout.Button("Undo")) {
                        List<TileDelta> undoDeltas = undoStack[undoPosition++];
                        foreach(TileDelta delta in undoDeltas)
                            delta.BuildOld(this);
                    }
                }
                if(undoPosition > 0) {
                    if(GUILayout.Button("Redo")) {
                        List<TileDelta> redoDeltas = undoStack[--undoPosition];
                        foreach(TileDelta delta in redoDeltas)
                            delta.BuildNew(this);
                    }
                }
            }
        }
        else {
            if(tilesInParent) {
                GUILayout.Label("Tiles found parented to editor!");
                if(GUILayout.Button("Move tile objects")) {
                    MoveTilesToAppropriateParents();
                    CheckForProblems();
                }
                GUILayout.Label("");
            }

            if(unkownObjectInParent) {
                GUILayout.Label("Foreign objects found parented to editor!");
                if(GUILayout.Button("Delete foreign objects")) {
                    DeleteForeignObjects();
                    CheckForProblems();
                }
                GUILayout.Label("");
            }

            if(invalidObjectInTiles) {
                GUILayout.Label("Found invalid objects in tile hierarchy");
                if(GUILayout.Button("Delete invalid objects")) {
                    DeleteInvalidTiles();
                    CheckForProblems();
                }
                GUILayout.Label("");
            }

            if(tilesetMissing) {
                GUILayout.Label("Could not find tileSet object!");
                GUILayout.Label("Please parent a valid tileSet to the tileEditor.");
                GUILayout.Label("");
            }

            if(tilesetInvalid) {
                GUILayout.Label("Tileset object is invalid!");
                GUILayout.Label("Please solve the problems shown in the tileSet.");
                GUILayout.Label("");
            }

            if(basicTileParentMissing) {
                GUILayout.Label("Could not find \"" + basicParentName + "\"!");
                if(GUILayout.Button("Spawn empty \"" + bckgrParentName + "\"")) {
                    Transform newParent = (new GameObject(basicParentName)).transform;
                    newParent.SetParent(transform);
                    newParent.localScale = Vector3.one;
                    CheckForProblems();
                }
                GUILayout.Label("");
            }

            if(backgroundTileParentMissing) {
                GUILayout.Label("Could not find \"" + bckgrParentName + "\"!");
                if(GUILayout.Button("Spawn empty \"" + bckgrParentName + "\"")) {
                    Transform newParent = (new GameObject(bckgrParentName)).transform;
                    newParent.SetParent(transform);
                    newParent.localScale = Vector3.one;
                    CheckForProblems();
                }
                GUILayout.Label("");
            }

            if(anyProblem) {
                if(GUILayout.Button("Refresh")) {
                    CheckForProblems();
                }
            }
            else {
                if(GUILayout.Button("Start building!")) {
                    PrepareForBuilding();
                }
            }
        }
    }

    private string GetHotKey(int idx) {
        return TileEditorSettings.GetBinding((TileEditorSettings.Key)(idx - 1)).ToString();
    }

    private bool anyProblem
    {
        get
        {
            return tilesInParent || unkownObjectInParent ||
                invalidObjectInTiles || tilesetMissing || tilesetInvalid ||
                basicTileParentMissing || backgroundTileParentMissing;
        }
    }

    private void PrepareForBuilding() {
        basicTileDictionary = new Dictionary<TileCoords, Transform>();
        bckgrTileDictionary = new Dictionary<TileCoords, Transform>();

        for(int i = 0; i < basicTileParent.childCount; i++) {
            Transform tile = basicTileParent.GetChild(i);
            TileCoords key = new TileCoords(tile);
            if(basicTileDictionary.ContainsKey(key)) {
                TryDestroy(tile.gameObject);
                Debug.LogWarning("Deleted duplicate tile!");
            }
            else
                basicTileDictionary.Add(key, tile);
        }

        for(int i = 0; i < backgroundTileParent.childCount; i++) {
            Transform tile = backgroundTileParent.GetChild(i);
            TileCoords key = new TileCoords(tile);
            if(bckgrTileDictionary.ContainsKey(key)) {
                TryDestroy(tile.gameObject);
                Debug.LogWarning("Deleted duplicate tile!");
            }
            else
                bckgrTileDictionary.Add(key, tile);
        }

        building = true;

        undoStack = new List<List<TileDelta>>();
        nextDelta = new List<TileDelta>();
        undoPosition = 0;
    }

    /// <summary>
    /// Moves all valid tiles parented to editor to their parents.
    /// </summary>
    private void MoveTilesToAppropriateParents() {

        //look for basic and special tiles
        for(int i = 0; i < transform.childCount; i++) {
            string name = transform.GetChild(i).name;
            if(IsLegalTileName(name)) {
                bool empty = GetTile(GetX(name), GetY(name)) == null;
                if(IsBasic(name) && empty) {
                    transform.GetChild(i).SetParent(basicTileParent);
                    transform.localScale = Vector3.one;
                    i--;
                }
            }
        }

        //look for background tiles
        for(int i = 0; i < transform.childCount; i++) {
            string name = transform.GetChild(i).name;
            if(IsLegalTileName(name)) {
                bool empty = GetBackGroundTile(GetX(name), GetY(name)) == null;
                if(IsBackgr(name) && empty) {
                    transform.GetChild(i).SetParent(backgroundTileParent);
                    transform.localScale = Vector3.one;
                    i--;
                }
            }
        }
    }

    private void DeleteForeignObjects() {
        //firstly, move all valid tiles out of the way
        MoveTilesToAppropriateParents();

        //now remove anything that is not one of the necessary children
        for(int i = 0; i < transform.childCount; i++) {
            Transform t = transform.GetChild(i);
            if(t != tileSet.transform && t != basicTileParent && t != backgroundTileParent)
                TryDestroy(t.gameObject);
        }
    }

    private void DeleteInvalidTiles() {

        //check for invalid object in basic tiles.
        for(int i = 0; i < basicTileParent.childCount; i++) {
            string name = basicTileParent.GetChild(i).name;
            if(!IsLegalTileName(name))
                TryDestroy(basicTileParent.GetChild(i).gameObject);
            else if(!IsBasic(name))
                TryDestroy(basicTileParent.GetChild(i).gameObject);
        }

        //check for invalid object in background tiles.
        for(int i = 0; i < backgroundTileParent.childCount; i++) {
            string name = backgroundTileParent.GetChild(i).name;
            if(!IsLegalTileName(name))
                TryDestroy(backgroundTileParent.GetChild(i).gameObject);
            else if(!IsBackgr(name))
                TryDestroy(backgroundTileParent.GetChild(i).gameObject);
        }
    }

    private void OnSceneGUI() {
        if(!building)
            return;

        char c = Event.current.character;
        if (c != '\0') {
            TileEditorSettings.Key key = TileEditorSettings.GetKey(c);
            Build(key);
        }

        if(dragKey != 0) {
            if(!freezeDrag) {
                dragX2 = buildX;
                dragY2 = buildY;
            }
            buildX = dragX1;
            buildY = dragY1;
            MarkBuildPoint(new Color(1f, .2f, .2f));
            buildX = dragX2;
            buildY = dragY2;
        }

        if(Event.current.type == EventType.MouseMove)
            MouseCast(GetSceneViewMouseRay());
        if(mouseInBuildPlane)
            MarkBuildPoint(new Color(1f, 1f, .2f));
    }

    private Ray GetSceneViewMouseRay() {
        Vector3 mousePosition = Event.current.mousePosition;
        Rect sceneViewRect = SceneView.lastActiveSceneView.position;
        mousePosition.x /= sceneViewRect.width;
        mousePosition.y /= sceneViewRect.height - SCENE_VIEW_SIZE_OFFSET;
        mousePosition.z = 1f;
        mousePosition.y = 1f - mousePosition.y;
        return SceneView.lastActiveSceneView.camera.ViewportPointToRay(mousePosition);
    }

    private void Build(TileEditorSettings.Key key) {
        if(key < 0)
            return;

        switch(key) {

        case TileEditorSettings.Key.toggleNormal:
        if(GetTile(buildX, buildY) == null) {
            UndoDeltaOneShot(0, 0);
            SpawnTile();
        }
        else {
            UndoDeltaOneShot(0, -1);
            DeleteTile();
        }
        return;

        case TileEditorSettings.Key.toggleBackground:
        if(GetBackGroundTile(buildX, buildY) == null) {
            UndoDeltaOneShot(2, 0);
            SpawnBackgroundTile(0);
        }
        else {
            UndoDeltaOneShot(2, -1);
            DeleteBackgroundTile();
        }
        return;

        case TileEditorSettings.Key.cycleBackground:
        UndoDeltaCycle(2, 1);
        CycleBackground(1);
        return;

        case TileEditorSettings.Key.cycleSpecial:
        UndoDeltaCycle(1, 1);
        CycleSpecial(1);
        return;

        case TileEditorSettings.Key.clearBackground:
        UndoDeltaOneShot(2, -1);
        DeleteBackgroundTile();
        return;
        }

        //if not returned yet, we are doing a rect (drag) build

        if(dragKey == 0) {
            //start drag selection
            dragKey = key;
            dragX1 = buildX;
            dragY1 = buildY;
        }
        else if(dragKey == key) {
            //confirm drag selection
            int nTiles = (Mathf.Abs(dragX2 - dragX1) + 1) *
                (Mathf.Abs(dragY2 - dragY1) + 1);
            bool placing = dragKey == TileEditorSettings.Key.fillRectNormal ||
                dragKey == TileEditorSettings.Key.fillRectBackgr;

            if(placing && nTiles > 1000) {
                freezeDrag = true;
                largePlacementPrompt = true;
                Repaint();
            }
            else
                ApplyDragRectKey();
        }
        else {
            //cancel drag selection
            dragKey = 0;
        }
    }

    /// <summary>
    /// Places or deletes stretch of tiles defined by drag coordinates and dragKey.
    /// </summary>
    private void ApplyDragRectKey() {
        for(int x = Mathf.Min(dragX1, dragX2); x <= Mathf.Max(dragX1, dragX2); x++) {
            for(int y = Mathf.Min(dragY1, dragY2); y <= Mathf.Max(dragY1, dragY2); y++) {
                buildX = x;
                buildY = y;
                switch(dragKey) {
                case TileEditorSettings.Key.fillRectNormal:
                if(GetTile(buildX, buildY) == null) {
                    UndoDelta(0, 0);
                    SpawnTile();
                }
                break;
                case TileEditorSettings.Key.clearRectNormal:
                if(GetTile(buildX, buildY) != null) {
                    UndoDelta(0, -1);
                    DeleteTile();
                }
                break;
                case TileEditorSettings.Key.fillRectBackgr:
                if(GetBackGroundTile(buildX, buildY) == null) {
                    UndoDelta(2, 0);
                    SpawnBackgroundTile(0);
                }
                break;
                case TileEditorSettings.Key.clearRectBackgr:
                if(GetBackGroundTile(buildX, buildY) != null) {
                    UndoDelta(2, -1);
                    DeleteBackgroundTile();
                }
                break;
                }
            }
        }
        dragKey = 0;
        FinishUndoDelta();
    }

    private void MouseCast(Ray ray) {
        Vector3 normal = transform.forward;
        float offset = Vector3.Dot(transform.position, normal);

        Plane buildPlane = new Plane(normal, -offset);

        float enter;
        mouseInBuildPlane = buildPlane.Raycast(ray, out enter);
        if(mouseInBuildPlane) {
            Vector3 intersect = ray.origin + enter * ray.direction;

            float relX = Vector3.Dot(intersect - transform.position, transform.right) / transform.localScale.x;
            float relY = Vector3.Dot(intersect - transform.position, transform.up) / transform.localScale.y;

            buildX = Mathf.FloorToInt(relX);
            buildY = Mathf.FloorToInt(relY);
        }
    }

    private void MarkBuildPoint(Color clr) {
        Vector3 center = GetWorldPoint(buildX, buildY);
        Handles.color = clr;

        for(int b = 0; b < 8; b++) {
            Vector3 up = (b % 2 == 0 ? -.5f : .5f) * transform.up;
            up = up * transform.localScale.y;
            Vector3 right = ((b >> 1) % 2 == 0 ? -.5f : .5f) * transform.right;
            right = right * transform.localScale.x;
            Vector3 forward = ((b >> 2) % 2 == 0 ? -1f : 1f) * transform.forward;
            forward = forward * transform.localScale.z;
            Vector3 corner = center + up + right + forward;

            Handles.DrawLine(corner, corner - up);
            Handles.DrawLine(corner, corner - right);
            Handles.DrawLine(corner, corner - forward);
        }

        SceneView.RepaintAll();
    }

    private Vector3 GetWorldPoint(int x, int y) {
        Vector3 coordOffset = ((x + .5f) * transform.right * transform.localScale.x) +
            ((y + .5f) * transform.up * transform.localScale.y);

        return transform.position + coordOffset;
    }

    /// <summary>
    /// Looks for tile at given x and y position and returns it.
    /// Returns null if no tile was found at given position.
    /// </summary>
    private Transform GetTile(int x, int y) {
        if(building) {
            Transform tile;
            basicTileDictionary.TryGetValue(new TileCoords(x, y), out tile);
            return tile;
        }

        //if not building, search for the tile manually
        for(int i = 0; i < basicTileParent.childCount; i++) {
            string name = basicTileParent.GetChild(i).name;
            if(!IsLegalTileName(name))
                continue;
            int refX = GetX(name);
            int refY = GetY(name);
            if(IsBasic(name))
                if(x == refX && y == refY)
                    return basicTileParent.GetChild(i);
        }
        return null;
    }

    /// <summary>
    /// Delete tile object at current build position
    /// </summary>
    private void DeleteTile() {
        Transform t = GetTile(buildX, buildY);
        if(t != null) {
            //if building, maintain tile dictionary
            if(building)
                basicTileDictionary.Remove(new TileCoords(t));

            TryDestroy(t.gameObject);
        }

        UpdateTile(buildX + 1, buildY);
        UpdateTile(buildX - 1, buildY);
        UpdateTile(buildX, buildY + 1);
        UpdateTile(buildX, buildY - 1);

        if(RequireDiagonalUpdate(tileSet.connectorType)) {
            UpdateTile(buildX + 1, buildY + 1);
            UpdateTile(buildX - 1, buildY + 1);
            UpdateTile(buildX + 1, buildY - 1);
            UpdateTile(buildX - 1, buildY - 1);
        }

        Change();
    }

    /// <summary>
    /// Spawn tile object at current build position
    /// </summary>
    private void SpawnTile() {
        Transform newTile = tileSet.SpawnTile(false, false, false, false);
        newTile.position = GetWorldPoint(buildX, buildY);
        newTile.SetParent(basicTileParent);
        newTile.localScale = Vector3.one;
        newTile.name = GetName(basicTileName, buildX, buildY);

        //if building, add tile to the dictionary
        if(building)
            basicTileDictionary[new TileCoords(newTile)] = newTile;

        UpdateTile(buildX, buildY);

        UpdateTile(buildX + 1, buildY);
        UpdateTile(buildX - 1, buildY);
        UpdateTile(buildX, buildY + 1);
        UpdateTile(buildX, buildY - 1);

        if(RequireDiagonalUpdate(tileSet.connectorType)) {
            UpdateTile(buildX + 1, buildY + 1);
            UpdateTile(buildX - 1, buildY + 1);
            UpdateTile(buildX + 1, buildY - 1);
            UpdateTile(buildX - 1, buildY - 1);
        }

        Change();
    }

    /// <summary>
    /// Force tile at given position to adapt to its surroundings, if it exists
    /// </summary>
    private void UpdateTile(int x, int y) {
        //check if there is a tile at this position
        Transform oldTile = GetTile(x, y);
        if(oldTile == null)
            return;

        //should not update special tiles
        if(GetTypeName(oldTile.name).StartsWith(spclTileName))
            return;

        //dealing with actual tile, delete old one and spawn a new.
        TryDestroy(oldTile.gameObject);

        bool up = GetTile(x, y + 1) != null;
        bool left = GetTile(x - 1, y) != null;
        bool right = GetTile(x + 1, y) != null;
        bool down = GetTile(x, y - 1) != null;
        Transform newTile = tileSet.SpawnTile(up, left, right, down);

        newTile.SetParent(basicTileParent);
        newTile.localScale = Vector3.one;
        newTile.position = GetWorldPoint(x, y);
        newTile.name = GetName(basicTileName, x, y);
        newTile.gameObject.isStatic = transform.gameObject.isStatic;

        AddConnectors(newTile, x, y);

        //if building, add tile to the dictionary
        if(building)
            basicTileDictionary[new TileCoords(newTile)] = newTile;
    }

    /// <summary>
    /// Add connector objects to the given tile based on its environment
    /// </summary>
    private void AddConnectors(Transform tile, int x, int y) {
        switch(tileSet.connectorType) {
        case TileSet.ConnectorType.none:
        break;

        case TileSet.ConnectorType.InvertedCorners:
        bool up = GetTile(x, y + 1) != null;
        bool left = GetTile(x - 1, y) != null;
        bool right = GetTile(x + 1, y) != null;
        bool down = GetTile(x, y - 1) != null;
        bool upLeft = up && left && GetTile(x - 1, y + 1) == null;
        bool upRight = up && right && GetTile(x + 1, y + 1) == null;
        bool downLeft = down && left && GetTile(x - 1, y - 1) == null;
        bool downRight = down && right && GetTile(x + 1, y - 1) == null;
        if(upLeft)
            tileSet.SpawnConnector(tile, 0);
        if(upRight)
            tileSet.SpawnConnector(tile, 1);
        if(downLeft)
            tileSet.SpawnConnector(tile, 2);
        if(downRight)
            tileSet.SpawnConnector(tile, 3);
        break;

        default:
        Debug.LogError("Encountered unkown tile connector type: " + tileSet.connectorType);
        break;
        }
    }

    private bool RequireDiagonalUpdate(TileSet.ConnectorType type) {
        switch(type) {
        case TileSet.ConnectorType.InvertedCorners:
        return true;
        default:
        return false;
        }
    }

    /// <summary>
    /// Deletes all valid tiles
    /// </summary>
    private void DeleteAllTiles() {
        while(basicTileParent.childCount > 0)
            TryDestroy(basicTileParent.GetChild(0).gameObject);
        while(backgroundTileParent.childCount > 0)
            TryDestroy(backgroundTileParent.GetChild(0).gameObject);

        Change();
    }

    /// <summary>
    /// Forces update on all valid tiles
    /// </summary>
    private void UpdateAllTiles() {

        int repeat = basicTileParent.childCount;
        while(repeat > 0) {
            repeat--;

            string name = basicTileParent.GetChild(0).name;
            int x = GetX(name);
            int y = GetY(name);

            //basic tile
            if(GetTypeName(name).StartsWith(basicTileName))
                UpdateTile(x, y);

            //special tile
            else {
                buildX = x;
                buildY = y;
                bool hasSpecials = tileSet.GetNbSpecialTiles() > 0;
                if(hasSpecials)
                    CycleSpecial(0);
                else
                    DeleteTile();
            }
        }

        repeat = backgroundTileParent.childCount;
        while(repeat > 0) {
            repeat--;

            //background tile
            string name = backgroundTileParent.GetChild(0).name;
            buildX = GetX(name);
            buildY = GetY(name);
            DeleteBackgroundTile();
            SpawnBackgroundTile(GetVariationIndex(name));
        }

        Change();
    }

    /// <summary>
    /// Return background tile at given coordinates, if one exists.
    /// </summary>
    private Transform GetBackGroundTile(int x, int y) {
        if(building) {
            Transform tile;
            bckgrTileDictionary.TryGetValue(new TileCoords(x, y), out tile);
            return tile;
        }

        //if not building, search for the tile manually
        for(int i = 0; i < backgroundTileParent.childCount; i++) {
            string name = backgroundTileParent.GetChild(i).name;
            if(!IsLegalTileName(name))
                continue;
            int refX = GetX(name);
            int refY = GetY(name);
            if(IsBackgr(name) && x == refX && y == refY)
                return backgroundTileParent.GetChild(i);
        }
        return null;
    }

    /// <summary>
    /// spawns background tile of given type at build coordinates.
    /// </summary>
    private void SpawnBackgroundTile(int index) {
        Transform newTile = tileSet.SpawnBackground(index);

        if(newTile == null) {
            Debug.LogError("Background tile index not found: " + index);
            return;
        }

        newTile.position = GetWorldPoint(buildX, buildY);
        newTile.SetParent(backgroundTileParent);
        newTile.localScale = Vector3.one;
        newTile.name = GetName(bckgrTileName + "_" + index, buildX, buildY);
        newTile.gameObject.isStatic = transform.gameObject.isStatic;

        //if building, add tile to the dictionary
        if(building)
            bckgrTileDictionary[new TileCoords(newTile)] = newTile;

        Change();
    }


    private void CycleBackground(int cycleOffset) {
        Transform oldTile = GetBackGroundTile(buildX, buildY);
        int next = cycleOffset - 1;
        if(oldTile != null) {
            next = GetVariationIndex(oldTile.name) + cycleOffset;
            TryDestroy(oldTile.gameObject);
        }

        int nbBackground = tileSet.GetNbBackgroundTiles();
        next = next % nbBackground;
        Transform newTile = tileSet.SpawnNextBackground(next);

        newTile.position = GetWorldPoint(buildX, buildY);
        newTile.SetParent(backgroundTileParent);
        newTile.localScale = Vector3.one;
        newTile.name = GetName(bckgrTileName + '_' + next, buildX, buildY);
        newTile.gameObject.isStatic = transform.gameObject.isStatic;

        //if building, add tile to the dictionary
        if(building)
            bckgrTileDictionary[new TileCoords(newTile)] = newTile;

        Change();
    }

    /// <summary>
    /// Delete tile at build coordinates, if it exists
    /// </summary>
    private void DeleteBackgroundTile() {
        Transform t = GetBackGroundTile(buildX, buildY);
        if(t != null) {
            //if building, maintain tile dictionary
            if(building)
                bckgrTileDictionary.Remove(new TileCoords(t));

            TryDestroy(t.gameObject);
        }

        Change();
    }


    private void CycleSpecial(int cycleOffset) {
        Transform oldTile = GetTile(buildX, buildY);
        int next = cycleOffset - 1;
        if(oldTile != null) {
            next = GetVariationIndex(oldTile.name) + cycleOffset;
            TryDestroy(oldTile.gameObject);
        }

        int nbSpecial = tileSet.GetNbSpecialTiles();
        next = next % nbSpecial;

        Transform newTile = tileSet.SpawnNextSpecial(next);
        newTile.position = GetWorldPoint(buildX, buildY);
        newTile.SetParent(basicTileParent);
        newTile.localScale = Vector3.one;
        newTile.name = GetName(spclTileName + '_' + next, buildX, buildY);
        newTile.gameObject.isStatic = transform.gameObject.isStatic;

        //if building, add tile to the dictionary
        if(building)
            basicTileDictionary[new TileCoords(newTile)] = newTile;

        UpdateTile(buildX + 1, buildY);
        UpdateTile(buildX - 1, buildY);
        UpdateTile(buildX, buildY + 1);
        UpdateTile(buildX, buildY - 1);

        Change();
    }

    /// <summary>
    /// Returns true if given name is a legal name, 
    /// assigned to a tile generated by the tileEditor.
    /// </summary>
    private static bool IsLegalTileName(string name) {
        if(string.IsNullOrEmpty(name))
            return false;
        string[] split = name.Split('.');
        if(split.Length != 3)
            return false;
        int dummy = 0;
        if(!int.TryParse(split[1], out dummy))
            return false;
        if(!int.TryParse(split[2], out dummy))
            return false;
        return true;
    }


    private static int GetVariationIndex(string name) {
        string sub = name.Split('.')[0];
        string[] split = sub.Split('_');
        if(split.Length < 2)
            return -1;
        return int.Parse(split[1]);
    }

    private static bool IsBasic(string name) {
        return GetTypeName(name).StartsWith(basicTileName) ||
            GetTypeName(name).StartsWith(spclTileName);
    }

    private static bool IsBackgr(string name) {
        return GetTypeName(name).StartsWith(bckgrTileName);
    }

    private static string GetTypeName(string name) {
        return name.Split('.')[0];
    }

    static int GetX(string name) {
        return int.Parse(name.Split('.')[1]);
    }

    static int GetY(string name) {
        return int.Parse(name.Split('.')[2]);
    }

    private static string GetName(string typeName, int x, int y) {
        return typeName + "." + x + '.' + y;
    }

    private static void TryDestroy(GameObject o) {
        try {
            DestroyImmediate(o);
        }
        catch(System.Exception e) {
            Debug.LogError("The tile editor failed to destroy an object. If the editor is part of a prefab you must unpack it temporarily or open the prefab for seperate editing.\n" + e);
        }
    }

    /// <summary>
    /// x y coordinate struct used in tile dictionaries
    /// </summary>
    private struct TileCoords {
        public int x, y;

        public TileCoords(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public TileCoords(Transform tile) {
            x = GetX(tile.name);
            y = GetY(tile.name);
        }

        public override bool Equals(object obj) {
            TileCoords o = (TileCoords)obj;
            return x == o.x && y == o.y;
        }

        public override int GetHashCode() {
            return x * 0x3FD3 + y;
        }

        public override string ToString() {
            return x.ToString() + '.' + y.ToString();
        }
    }

    private void UndoDeltaCycle(int type, int cycleOffset) {
        bool mainTile = type <= 1;
        int var = -1;
        Transform oldTile = mainTile ? GetTile(buildX, buildY) :
                GetBackGroundTile(buildX, buildY);
        if(oldTile != null)
            var = GetVariationIndex(oldTile.name);
        var += cycleOffset;
        var %= mainTile ? tileSet.GetNbSpecialTiles() : tileSet.GetNbBackgroundTiles();
        UndoDeltaOneShot(type, var + cycleOffset);
    }

    private void UndoDeltaOneShot(int type, int var) {
        UndoDelta(type, var);
        FinishUndoDelta();
    }

    private void UndoDelta(int type, int var) {
        TileDelta newDelta = new TileDelta(buildX, buildY, type, var, this);
        nextDelta.Add(newDelta);
    }

    private void FinishUndoDelta() {
        undoStack.RemoveRange(0, undoPosition);
        undoPosition = 0;
        undoStack.Insert(0, nextDelta);
        nextDelta = new List<TileDelta>();
        Repaint();
        Change();
    }

    private void Change() {
        EditorUtility.SetDirty(transform.gameObject);
    }

    private struct TileDelta {
        private TileCoords coords;
        private TileState oldState, newState;

        public TileDelta(int x, int y, int newMainType, int newSubType, TileEditorExt tileEditor) : 
            this(new TileCoords(x, y), new TileState(newMainType, newSubType), tileEditor) { }

        public TileDelta(TileCoords coords, TileState newState, TileEditorExt tileEditor) {
            this.coords = coords;
            this.oldState = new TileState(newState.mainType, coords, tileEditor);
            this.newState = newState;
        }

        public void BuildOld(TileEditorExt tileEditor) {
            oldState.Build(coords, tileEditor);
        }

        public void BuildNew(TileEditorExt tileEditor) {
            newState.Build(coords, tileEditor);
        }
    }

    private struct TileState {
        public int mainType; //0 - normal, 1 - Special, 2 - Background
        public int subType; //variation index, for background and special tiles (-1 for nothing)

        public TileState(int mainType, int subType) {
            this.mainType = mainType;
            this.subType = subType;
        }

        public TileState(int type, TileCoords coords, TileEditorExt tileEditor) {
            this.mainType = type;
            bool mainTile = type <= 1;
            Transform oldTile = mainTile ? tileEditor.GetTile(coords.x, coords.y) :
                tileEditor.GetBackGroundTile(coords.x, coords.y);

            if(oldTile == null) {
                subType = -1;
                return;
            }

            int var = GetVariationIndex(oldTile.name);

            if(mainTile) { 
                mainType = var < 0 ? 0 : 1;
                subType = var < 0 ? 0 : var;
            }
            else {
                mainType = 2;
                subType = var;
            }
        }

        public void Build(TileCoords coords, TileEditorExt tileEditor) {
            tileEditor.buildX = coords.x;
            tileEditor.buildY = coords.y;

            //delete any old tiles
            if(mainType <= 1)
                tileEditor.DeleteTile();
            else
                tileEditor.DeleteBackgroundTile();

            //if this is an empty state, we're done
            if(subType < 0)
                return;

            //if not build the tile we need
            switch(mainType) {
            case 0: tileEditor.SpawnTile(); break;
            case 1: tileEditor.CycleSpecial(subType + 1); break;
            case 2: tileEditor.SpawnBackgroundTile(subType); break;
            }
        }
    }
}