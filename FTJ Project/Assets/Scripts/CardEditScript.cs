using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardEditScript : MonoBehaviour {
	CardData card_data = new CardData();
	public GameObject card_prefab;
	List<GameObject> spawned_cards = new List<GameObject>();
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void CreateCard(Transform the_transform){
		var card = (GameObject)GameObject.Instantiate(card_prefab, the_transform.position, the_transform.rotation);
		//card.GetComponent<CardScript>().SetCardData(card_data);
		//card.GetComponent<CardScript>().Bake();
		GameObject.Destroy(card.rigidbody);
		spawned_cards.Add(card);
	}
	
	void Test() {
		foreach(var card in spawned_cards){
			GameObject.Destroy(card);
		}
		spawned_cards.Clear();
		CreateCard (GameObject.Find("card_spawn_front").transform);
		CreateCard (GameObject.Find("card_spawn_back").transform);
	}
	
	void OnGUI () {
		GUILayout.BeginArea(new Rect(30,30,300,300));
		GUILayout.BeginHorizontal();
		GUILayout.Label("Title: ");
		card_data.title = GUILayout.TextField(card_data.title, GUILayout.MinWidth(100));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Type: ");
		card_data.type = GUILayout.TextField(card_data.type, GUILayout.MinWidth(100));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Rules: ");
		card_data.rules = GUILayout.TextArea(card_data.rules, GUILayout.MinWidth(100));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Flavour: ");
		card_data.flavour = GUILayout.TextArea(card_data.flavour, GUILayout.MinWidth(100));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Image: ");
		var image_str = GUILayout.TextField(card_data.image.ToString(), GUILayout.MinWidth(100));
		if(!int.TryParse(image_str, out card_data.image)){
			card_data.image = 0;
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Back: ");
		var back_str = GUILayout.TextField(card_data.back.ToString(), GUILayout.MinWidth(100));
		if(!int.TryParse(back_str, out card_data.back)){
			card_data.back = 0;
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Test")){
			Test();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
