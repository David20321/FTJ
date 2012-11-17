using UnityEngine;
using System.Collections;

public class ColorPalette : MonoBehaviour {
	static Color Color255(int r, int g, int b){
		return new Color(r/255.0f, g/255.0f, b/255.0f);
	}
	
	public static Color GetColor(int which) {
		switch(which){
			case 0:
				return Color255(175,0,37); break;
			case 1:
				return Color255(9,144,239); break;
			case 2:
				return Color255(2,255,32); break;
			case 3:
				return Color255(255,223,97); break;
			default:
				return Color255(0,0,0); break;
		}
	}
}
