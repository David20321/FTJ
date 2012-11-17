using UnityEngine;
using System.Collections;

public class BoardCreateScript : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		foreach(Transform tile in transform.FindChild("DisplayTiles")){
			tile.FindChild("default").renderer.material = TileManagerScript.Instance().GetMaterial(int.Parse(tile.gameObject.name)-1);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
