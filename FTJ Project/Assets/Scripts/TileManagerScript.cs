using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MiniJSON;

public class TileManagerScript : MonoBehaviour {
	public GameObject tile_bake_prefab;
	public TextAsset board_json;
	public GUISkin gui_skin;
	List<Material> tile_materials = new List<Material>();
	const int MAX_TITLE_WIDTH = 60;
	const int MAX_RULES_WIDTH = 115;
		
	// Use this for initialization
	void Awake () {
		var tile_bake_object = (GameObject)GameObject.Instantiate(tile_bake_prefab);
		
		var dict = Json.Deserialize(board_json.text) as Dictionary<string,object>;
		object retrieve_obj;
		if(!dict.TryGetValue("Tiles", out retrieve_obj)){
			Debug.Log ("Could not find 'Tiles' in board JSON");
		}
		var tile_list = (List<object>)retrieve_obj;
		foreach(var obj in tile_list){
			var tile = (Dictionary<string, object>)obj;
			var title_obj = tile_bake_object.transform.FindChild("Title");
			var rules_obj = tile_bake_object.transform.FindChild("Rules");
			var old_title_scale = title_obj.transform.localScale;
			var old_rules_scale = rules_obj.transform.localScale;
			if(tile.TryGetValue("Title", out retrieve_obj)){
				var title_string = (string)retrieve_obj;
				var dimensions = gui_skin.GetStyle("label").CalcSize(new GUIContent(title_string));
				title_obj.GetComponent<TextMesh>().text = title_string;
				if(dimensions.x > MAX_TITLE_WIDTH){
					title_obj.transform.localScale *= MAX_TITLE_WIDTH/dimensions.x;
				}
			}
			if(tile.TryGetValue("Rules", out retrieve_obj)){
				var rules_string = (string)retrieve_obj;
				var dimensions = gui_skin.GetStyle("label").CalcSize(new GUIContent(rules_string));
				rules_obj.GetComponent<TextMesh>().text = rules_string;
				if(dimensions.x > MAX_RULES_WIDTH){
					rules_obj.transform.localScale *= MAX_RULES_WIDTH/dimensions.x;
				}
			}
			tile_materials.Add(tile_bake_object.GetComponent<TileScript>().Bake());
			title_obj.transform.localScale = old_title_scale;
			rules_obj.transform.localScale = old_rules_scale;
		}
	}
	
	public static TileManagerScript Instance() {
		if(GameObject.Find("GlobalScriptObject")){
			return GameObject.Find("GlobalScriptObject").GetComponent<TileManagerScript>();
		}
		return null;
	}
	
	public Material GetMaterial(int id){
		if(id >= tile_materials.Count){
			return tile_materials[0];
		}
		
		return tile_materials[id];
	}
}
