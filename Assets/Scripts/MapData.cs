using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapData {
	[System.Serializable]
	class MapTileSerial {
		public string sprname;
		public string guiname;
		public string[] valskeys;
		public string[] valsvals;

		public MapTile ConvertToMapTile() {
			Dictionary<string,string> newvals = new Dictionary<string,string>();
			for (int i = 0; i < valskeys.Length; i++) {
				newvals.Add(valskeys[i],valsvals[i]);
			}
			return new MapTile(sprname, guiname, newvals);
		}
	}

	class MapTile {
		public string sprname;
		public string guiname;
		public int xpos;
		public int ypos;
		public Dictionary<string,string> vals;

		public MapTile(string sprnamei, string guinamei, Dictionary<string,string> valsi) {
			sprname = sprnamei;
			guiname = guinamei;
			vals = valsi;
		}
	}

	class MapAggregate {
		public MapTile[] tiles;
		public MapAggregate(string filePath) {

		}

		public int GetNumTiles() {
			return tiles.Length;
		}

		public MapTile GetTileByNumber(int num) {
			return tiles[num];
		}
	}
}