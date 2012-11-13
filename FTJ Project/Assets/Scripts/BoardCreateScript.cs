using UnityEngine;
using System.Collections;

public class BoardCreateScript : MonoBehaviour {
	public TextAsset board_json;
	public GameObject tile_prefab;
	const float TILE_SIZE = 0.7f;

	// Use this for initialization
	void Start () {
		int[] grid_dim = new int[2];
		grid_dim[0] = 9;
		grid_dim[1] = 7;
		Vector3 offset = new Vector3(grid_dim[1] * -0.5f + 0.5f, 0, grid_dim[0] * -0.5f + 0.5f) * TILE_SIZE; 
		for(int i=0; i<grid_dim[0]; ++i){
			for(int j=0; j<grid_dim[1]; ++j){
				Instantiate(tile_prefab, transform.position + offset + new Vector3(j*TILE_SIZE,0,i*TILE_SIZE), Quaternion.identity);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
