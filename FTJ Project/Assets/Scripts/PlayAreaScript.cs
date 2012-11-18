using UnityEngine;
using System.Collections;

public class PlayAreaScript : MonoBehaviour {
	[RPC]
	public void SetColor(int id){
		var color = ColorPalette.GetColor(id);
		float avg_color = (color.r+color.g+color.b)/3.0f;
		float desaturation = 0.7f;
		color = new Color(Mathf.Lerp(color.r, avg_color, desaturation),
						  Mathf.Lerp(color.g, avg_color, desaturation),
						  Mathf.Lerp(color.b, avg_color, desaturation));
		renderer.material.color = color;
		if(Network.isServer){
			networkView.RPC("SetColor",RPCMode.OthersBuffered, id);
		}
	}
}
