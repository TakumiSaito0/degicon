using UnityEngine;

public class TileSet : MonoBehaviour {

	public GameObject[] basicTiles = new GameObject[16];
    public GameObject[] backgroundTiles = new GameObject[0];
	public GameObject[] specialTiles = new GameObject[0];

    public ConnectorType connectorType;
    public GameObject[] connectors = new GameObject[0];
    //public TileSetExpansion[] expansions;

	/// <summary>
	/// Instantiates tile of this tileSet, that fits in the given surroundings 
	/// (true indicates there is another tile in that direction).
	/// The tile has the correct rotation to fit in under a tile parent at default rotation.
	/// </summary>
	public Transform SpawnTile(bool up, bool left, bool right, bool down) {
		TileType type = 0;

		//use boolean magic to find correct tile index in basicTiles array.
		if(up)
			type += 8;
		if(down ^ up)
			type += 4;
		if(left ^ right)
			type += 2;
		if(left)
			type += 1;

		//Copy basic tile and it's rotation
		GameObject toReturn = Instantiate(basicTiles[(int)type]);
		toReturn.transform.rotation = basicTiles[(int)type].transform.rotation;

		//new tile is ready; return it.
		return toReturn.transform;

	}

    /// <summary>
    /// Instantiate connector object and parent it to the given tile
    /// </summary>
    public Transform SpawnConnector(Transform tile, int index) {
        Transform toReturn = (Instantiate(connectors[index])).transform;
        toReturn.rotation = connectors[index].transform.rotation;
        toReturn.SetParent(tile);
        toReturn.localPosition = Vector3.zero;
        toReturn.localScale = Vector3.one;
        return toReturn;
    }

	/// <summary>
	/// Spawn background tile object in default rotation and return it.
	/// Returns null if given tile does not exist.
	/// </summary>
	public Transform SpawnBackground(int index) {
        if(index < 0 || index >= backgroundTiles.Length)
            return null;
        GameObject toReturn = Instantiate(backgroundTiles[index]);
        toReturn.transform.rotation = backgroundTiles[index].transform.rotation;
        return toReturn.transform;
	}

	/// <summary>
	/// Spawn special tile object in default rotation and return it.
	/// next should be ModSpecialIndex(index of the old tile + 1).
	/// </summary>
	public Transform SpawnNextSpecial(int next) {
		if(specialTiles.Length == 0)
			return null;

		GameObject toReturn = Instantiate(specialTiles[next]);
		toReturn.transform.rotation = specialTiles[next].transform.rotation;
		return toReturn.transform;
	}

    public Transform SpawnNextBackground(int index) {
        if(index >= backgroundTiles.Length)
            return null;

        GameObject toReturn = Instantiate(backgroundTiles[index]);
        toReturn.transform.rotation = backgroundTiles[index].transform.rotation;
        return toReturn.transform;
    }

    public int GetNbSpecialTiles() {
		return specialTiles.Length;
	}

    public int GetNbBackgroundTiles() {
        return backgroundTiles.Length;
    }

    /// <summary>
    /// Enum shows indices that basic tiles should follow in the basicTiles array.
    /// E.g. basicTiles[9] should be a wall piece tile, 
    /// to be surrounded by other tiles on all sides.
    /// </summary>
    public enum TileType {
		HoverBlock = 0,
		HoverPiece = 1,
		HoverCorner_L = 2,
		HoverCorner_R = 3,

		FloorBlock = 4,
		FloorPiece = 5,
		FloorCorner_L = 6,
		FloorCorner_R = 7,

		WallBlock = 8,
		WallPiece = 9,
		WallCorner_L = 10,
		WallCorner_R = 11,

		CeilingBlock = 12,
		CeilingPiece = 13,
		CeilingCorner_L = 14,
		CeilingCorner_R = 15
	}

	public bool BasicTilesValid() {
		if(basicTiles == null)
			return false;
		if(basicTiles.Length != 16)
			return false;
		for(int i = 0; i < 16; i++) {
			if(basicTiles[i] == null)
				return false;
		}
		return true;
	}

	public bool BackgroundTilesValid() {
        for(int i = 0; i < backgroundTiles.Length; i++) {
            if(backgroundTiles[i] == null)
                return false;
        }
        return true;
    }

	public bool SpecialTilesValid() {
		if(specialTiles == null) {
			specialTiles = new GameObject[0];
			return true;
		}
		for(int i = 0; i < specialTiles.Length; i++) {
			if(specialTiles[i] == null)
				return false;
		}
		return true;
	}

	public bool Valid() {
		return BasicTilesValid() && 
			BackgroundTilesValid() && 
			SpecialTilesValid();
	}

    private int GetNboConnectors(ConnectorType type) {
        switch(type) {
        case ConnectorType.none: return 0;
        case ConnectorType.InvertedCorners: return 4;
        default: return 0;
        }
    }

    public enum ConnectorType {
        none, InvertedCorners
    }
}