using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
	GameObject top_card_ = null;
	GameObject bottom_card_ = null;
	const float CARD_THICKNESS_MULT = 0.04f;
	const float ORIGINAL_SCALE = 1.0f;
	
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
		RegenerateEndCards();
	}
	
	void RegenerateEndCards() {
		if(!top_card_ && cards_.Count > 1){
			var pos = transform.FindChild("bottom_card").transform.position;
			var rot = transform.FindChild("bottom_card").transform.rotation;
			pos += transform.rotation * new Vector3(0,cards_.Count*0.012f+0.1f,0);
			top_card_ = CreateCard(cards_[0],pos,rot);
		}		
		if(!bottom_card_ && cards_.Count > 0){
			var pos = transform.FindChild("bottom_card").transform.position;
			var rot = transform.FindChild("bottom_card").transform.rotation;
			bottom_card_ = CreateCard(cards_[cards_.Count-1],pos,rot);
		}
	}
	
	GameObject CreateCard(CardData card_data, Vector3 pos, Quaternion rot){
		var card = (GameObject)GameObject.Instantiate(card_prefab, pos, rot);
		card.transform.parent = transform;
		GameObject.Destroy(card.rigidbody);
		card.collider.enabled = false;
		var card_script = card.GetComponent<CardScript>();
		card_script.SetCardData(card_data);
		card_script.Bake();
		return card;
	}
	
	void CopyComponent(Component old_component, GameObject game_object){
		Component new_component = game_object.AddComponent(old_component.GetType());
		foreach (FieldInfo f in old_component.GetType().GetFields())
		{
		  f.SetValue(new_component, f.GetValue(old_component));
		}
	}
	
	public GameObject TakeTopCard(){
		if(cards_.Count == 0){
			return null;
		}
		GameObject card;
		if(top_card_){
			card = top_card_;
			top_card_ = null;
			cards_.RemoveAt(0);
		} else {
			card = bottom_card_;
			bottom_card_ = null;
			cards_.RemoveAt(cards_.Count-1);
		}
		if(cards_.Count <= 1){
			transform.FindChild("default").renderer.enabled = false;
		}
		if(cards_.Count == 1){
			bottom_card_.transform.position += transform.rotation * new Vector3(0,0.07f,0);
		}
		RegenerateEndCards();
		CopyComponent(card_prefab.rigidbody, card);
		card.collider.enabled = true;
		card.transform.parent = null;
		card.transform.localScale = new Vector3(1,1,1);
		return card;
	}
	
	// Update is called once per frame
	void Update () {
		transform.FindChild("default").localScale = new Vector3(1,Mathf.Max(2,cards_.Count) * CARD_THICKNESS_MULT,1);	
		if(Input.GetMouseButtonDown(0)){
			var card = TakeTopCard();
			if(card){
				card.rigidbody.AddForce(new Vector3(0,1000,0));
				GameObject.Destroy(card);
			}
		}
	}
}
