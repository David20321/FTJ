using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MiniJSON;

public class CardManagerScript : MonoBehaviour {
	public GameObject card_bake_prefab;
	public TextAsset deck_json;
	public Material cutout;
	public Texture[] back_textures;
	List<CardData> cards_ = new List<CardData>();
	List<Material> back_materials = new List<Material>();
	Dictionary<string, List<int>> decks_ = new Dictionary<string, List<int>>();
		
	// Use this for initialization
	void Start () {
		foreach(Texture texture in back_textures){
			var mat = new Material(cutout);
			mat.mainTexture = texture;
			back_materials.Add(mat);
		}
		var card_bake_object = (GameObject)GameObject.Instantiate(card_bake_prefab);
		int next_id = 0;
		var dict = Json.Deserialize(deck_json.text) as Dictionary<string,object>;
		foreach(var pair in dict){
			var deck_name = (string)pair.Key;
			decks_[deck_name] = new List<int>();
			var deck_list = decks_[deck_name];
			var card_list = (List<object>)pair.Value;
			foreach(var card in card_list){
				var card_dict = (Dictionary<string, object>)card;
				var card_data = new CardData();
				card_data.title = "Title";
				if(card_dict.ContainsKey("Title")){
					card_data.title = (string)card_dict["Title"];
				}
				card_data.type = "Type";
				if(card_dict.ContainsKey("Type")){
					card_data.type = (string)card_dict["Type"];
				}
				card_data.rules = "Rules";
				if(card_dict.ContainsKey("Rules")){
					card_data.rules = (string)card_dict["Rules"];
				}
				card_data.flavour = "Flavour";
				if(card_dict.ContainsKey("Flavour")){
					card_data.flavour = (string)card_dict["Flavour"];
				}
				card_data.target = 0;
				if(card_dict.ContainsKey("Target")){
					card_data.target = (int)(long)card_dict["Target"];
				}
				card_data.gold = 0;
				if(card_dict.ContainsKey("Gold")){
					card_data.gold = (int)(long)card_dict["Gold"];
				}
				card_data.points = 0;
				if(card_dict.ContainsKey("Points")){
					card_data.points = (int)(long)card_dict["Points"];
				}
				card_data.price = 0;
				if(card_dict.ContainsKey("Price")){
					card_data.price = (int)(long)card_dict["Price"];
				}
				card_data.image = 0;
				if(card_dict.ContainsKey("Image")){
					card_data.image = (int)(long)card_dict["Image"];
				}
				card_data.back = 0;
				if(card_dict.ContainsKey("Back")){
					card_data.back = (int)(long)card_dict["Back"];
				}
				int duplicates = 1;
				if(card_dict.ContainsKey("Duplicates")){
					duplicates = (int)(long)card_dict["Duplicates"];
				}
				for(int i=0; i<duplicates; ++i){
					deck_list.Add(cards_.Count);
				}
				card_bake_object.GetComponent<CardBakeScript>().SetCardData(card_data);
				card_data.material = card_bake_object.GetComponent<CardBakeScript>().Bake();
				cards_.Add(card_data);
			}
		}
	}
	
	public List<int> GetDeckCards(string name){
		return decks_[name];
	}
	
	public static CardManagerScript Instance() {
		if(GameObject.Find("GlobalScriptObject")){
			return GameObject.Find("GlobalScriptObject").GetComponent<CardManagerScript>();
		}
		return null;
	}
	
	public Material GetBackMaterial(int id){
		return back_materials[cards_[id].back];
	}
	
	public Material GetFrontMaterial(int id){
		return cards_[id].material;
	}
}
