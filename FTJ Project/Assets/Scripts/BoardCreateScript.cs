using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class BoardCreateScript : MonoBehaviour {
	public TextAsset board_json;
	public GameObject tile_prefab;
	public GUISkin gui_skin;
	const float TILE_SIZE = 0.7f;
	const int MAX_TITLE_WIDTH = 60;
	const int MAX_RULES_WIDTH = 115;
	
	// Use this for initialization
	void Start () {
		var dict = Json.Deserialize(board_json.text) as Dictionary<string,object>;
		var grid_dim = new int[2];
		object retrieve_obj = null;
		if(!dict.TryGetValue("Grid", out retrieve_obj)){
			Debug.Log ("Could not find 'Grid' in board JSON");
		}
		var obj_list = (List<object>)retrieve_obj;
		grid_dim[0] = (int)(long)(obj_list[0]);
		grid_dim[1] = (int)(long)(obj_list[1]);
		
		Vector3 offset = new Vector3(grid_dim[1] * -0.5f + 0.5f, 0, grid_dim[0] * -0.5f + 0.5f) * TILE_SIZE; 
		
		if(!dict.TryGetValue("Tiles", out retrieve_obj)){
			Debug.Log ("Could not find 'Tiles' in board JSON");
		}
		var tile_list = (List<object>)retrieve_obj;
		
		foreach(var obj in tile_list){
			var tile = (Dictionary<string, object>)obj;
			var coord_list = (List<object>)tile["Pos"];
			var coord = new long[2];
			coord[0] = (long)coord_list[0];
			coord[1] = (long)coord_list[1];
			var tile_obj = (GameObject)Instantiate(tile_prefab, transform.position + offset + new Vector3((grid_dim[1]-coord[1])*TILE_SIZE,0,(grid_dim[0]-coord[0])*TILE_SIZE), Quaternion.identity);
			var color_list = (List<object>)tile["Color"];
			var color = new Color(((float)(long)color_list[0])/255.0f,
								  ((float)(long)color_list[1])/255.0f,
								  ((float)(long)color_list[2])/255.0f);
			tile_obj.transform.FindChild("Tile_base").FindChild("default").renderer.material.color = color;
			if(tile.TryGetValue("Title", out retrieve_obj)){
				var title_string = (string)retrieve_obj;
				var dimensions = gui_skin.GetStyle("label").CalcSize(new GUIContent(title_string));
				var title_obj = tile_obj.transform.FindChild("Title");
				title_obj.GetComponent<TextMesh>().text = title_string;
				if(dimensions.x > MAX_TITLE_WIDTH){
					title_obj.transform.localScale *= MAX_TITLE_WIDTH/dimensions.x;
				}
			}
			if(tile.TryGetValue("Rules", out retrieve_obj)){
				var rules_string = (string)retrieve_obj;
				var dimensions = gui_skin.GetStyle("label").CalcSize(new GUIContent(rules_string));
				var rules_obj = tile_obj.transform.FindChild("Rules");
				rules_obj.GetComponent<TextMesh>().text = rules_string;
				if(dimensions.x > MAX_RULES_WIDTH){
					rules_obj.transform.localScale *= MAX_RULES_WIDTH/dimensions.x;
				}
			}
			tile_obj.GetComponent<TileScript>().Bake();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
