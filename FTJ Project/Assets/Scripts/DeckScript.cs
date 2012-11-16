using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MiniJSON;

public class DeckScript : MonoBehaviour {
	public GameObject card_prefab;
	public string deck_name;
	List<int> cards_ = new List<int>();
	GameObject top_card_ = null;
	GameObject bottom_card_ = null;
	const float CARD_THICKNESS_MULT = 0.04f;
	const float ORIGINAL_SCALE = 1.0f;
	const float DECK_MASS_PER_CARD = 0.3f;
	
	// Use this for initialization
	void Start () {
		cards_ = CardManagerScript.Instance().GetDeckCards(deck_name);
		RegenerateEndCards();
	}
	
	void RegenerateEndCards() {
		if(!top_card_ && cards_.Count > 1){
			var pos = transform.FindChild("bottom_card").transform.position;
			var rot = transform.FindChild("bottom_card").transform.rotation;
			pos += transform.rotation * new Vector3(0,(cards_.Count*0.013f+0.1f)*transform.localScale.y,0);
			top_card_ = CreateCard(cards_[0],pos,rot);
		}		
		if(!bottom_card_ && cards_.Count > 0){
			var pos = transform.FindChild("bottom_card").transform.position;
			var rot = transform.FindChild("bottom_card").transform.rotation;
			bottom_card_ = CreateCard(cards_[cards_.Count-1],pos,rot);
		}
	}
	
	GameObject CreateCard(int card_id, Vector3 pos, Quaternion rot){
		var card = (GameObject)Network.Instantiate(card_prefab, pos, rot,0);
		card.transform.parent = transform;
		GameObject.Destroy(card.rigidbody);
		card.collider.enabled = false;
		var card_script = card.GetComponent<CardScript>();
		card_script.Prepare(card_id);
		card.transform.localScale = new Vector3(1,1,1);
		return card;
	}
	
	void CopyComponent(Component old_component, GameObject game_object){
		Component new_component = game_object.AddComponent(old_component.GetType());
		foreach (FieldInfo f in old_component.GetType().GetFields())
		{
		  f.SetValue(new_component, f.GetValue(old_component));
		}
	}
	
	public GameObject TakeCard(bool top){
		if(cards_.Count == 0){
			return null;
		}
		GameObject card;
		int card_id = -1;
		if(top && top_card_){
			card = top_card_;
			top_card_ = null;
			card_id = cards_[0];
			cards_.RemoveAt(0);
		} else {
			card = bottom_card_;
			bottom_card_ = null;
			card_id = cards_[cards_.Count-1];
			cards_.RemoveAt(cards_.Count-1);
		}
		if(cards_.Count <= 1){
			transform.FindChild("default").renderer.enabled = false;
		}
		RegenerateEndCards();
		
		var new_card = (GameObject)GameObject.Instantiate(card_prefab, card.transform.position, card.transform.rotation);
		new_card.GetComponent<CardScript>().Prepare(card_id);
		GameObject.Destroy(card);
		if(cards_.Count == 1){
			collider.enabled = false;
			TakeCard(false);
		}
		return new_card;
	}
	
	public GameObject TakeBottomCard(){
		return TakeCard(false);
	}
	
	public GameObject TakeTopCard(){
		return TakeCard(true);
	}
	
	// Update is called once per frame
	void Update () {
		transform.FindChild("default").localScale = new Vector3(1,Mathf.Max(2,cards_.Count) * CARD_THICKNESS_MULT,1);	
		rigidbody.mass = Mathf.Max(1,cards_.Count) * DECK_MASS_PER_CARD;
		if(top_card_ && bottom_card_){
			var the_collider = GetComponent<BoxCollider>();
			the_collider.center = (top_card_.transform.localPosition + bottom_card_.transform.localPosition) * 0.5f;
			the_collider.extents = new Vector3(the_collider.extents.x,Vector3.Distance(top_card_.transform.position, bottom_card_.transform.position)*0.5f+0.1f,the_collider.extents.z);
		}
		/*if(Input.GetMouseButtonDown(0)){
			var card = TakeTopCard();
			if(card){
				card.rigidbody.AddForce(new Vector3(0,1000,0));
				//GameObject.Destroy(card);
			}
		}*/
	}
}
