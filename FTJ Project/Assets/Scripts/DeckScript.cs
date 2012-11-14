using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class CardData {
	public string title;
	public string type;
	public string rules;
	public string flavour;
	public int image;
	public int back;
}

public class DeckScript : MonoBehaviour {
	public GameObject card_prefab;
	public TextAsset deck_json;
	public string deck_name;
	List<CardData> cards_ = new List<CardData>();
	GameObject top_card = null;
	GameObject bottom_card = null;
	
	// Use this for initialization
	void Start () {
		var dict = Json.Deserialize(deck_json.text) as Dictionary<string,object>;
		if(!dict.ContainsKey(deck_name)){
			Debug.Log ("Could not find deck:"+deck_name+" in deck JSON");
		}
		var card_list = (List<object>)dict[deck_name];
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
				cards_.Add(card_data);	
			}
		}
		var top_transform = transform.FindChild("top_card").transform;
		top_card = (GameObject)GameObject.Instantiate(card_prefab, top_transform.position, top_transform.rotation);
		top_card.transform.parent = transform;
		GameObject.Destroy(top_card.rigidbody);
		GameObject.Destroy(top_card.collider);
		{
			var card_script = top_card.GetComponent<CardScript>();
			card_script.SetCardData(cards_[0]);
			card_script.Bake();
		}
		
		var bottom_transform = transform.FindChild("bottom_card").transform;
		bottom_card = (GameObject)GameObject.Instantiate(card_prefab, bottom_transform.position, bottom_transform.rotation);
		bottom_card.transform.parent = transform;
		GameObject.Destroy(bottom_card.rigidbody);
		GameObject.Destroy(bottom_card.collider);
		{
			var card_script = bottom_card.GetComponent<CardScript>();
			card_script.SetCardData(cards_[cards_.Count-1]);
			card_script.Bake();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
