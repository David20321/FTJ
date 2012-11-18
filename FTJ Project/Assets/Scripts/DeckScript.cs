using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MiniJSON;

public class DeckScript : MonoBehaviour {
	public GameObject card_prefab;
	public GameObject card_facade_prefab;
	List<int> cards_ = new List<int>();
	int top_card_id_;
	int bottom_card_id_;
	int num_cards_;
	GameObject top_card_ = null;
	GameObject bottom_card_ = null;
	const float CARD_THICKNESS_MULT = 0.04f;
	const float ORIGINAL_SCALE = 1.0f;
	const float DECK_MASS_PER_CARD = 0.3f;
	
	public AudioClip[] shuffle_sound;
	public AudioClip[] impact_sound;
	public AudioClip[] pick_up_sound;
	float last_sound_time = 0.0f;
	float last_shuffle_time = 0.0f;
	const float PHYSICS_SOUND_DELAY = 0.1f;
	const float SHUFFLE_DELAY = 0.6f;
	
	void PlayRandomSound(AudioClip[] clips, float volume){
		audio.PlayOneShot(clips[Random.Range(0,clips.Length)], volume);
	}	
	
	[RPC]
	public void PickUpSound() {
		if(Network.isServer){
			networkView.RPC("PickUpSound",RPCMode.Others);
		}
		PlayRandomSound(pick_up_sound, 0.1f);
	}
	
	public List<int> GetCards(){
		return cards_;
	}
	
	public void RandomizeCards(){
		List<int> new_card_list = new List<int>();
		while(cards_.Count > 0){
			int rand_card = Random.Range(0,cards_.Count);
			new_card_list.Add (cards_[rand_card]);
			cards_.RemoveAt(rand_card);
		}
		cards_ = new_card_list;
		RegenerateEndCardIDs();
		RegenerateEndCards();
	}
	
	[RPC]
	void ShuffleSound(){
		if(Network.isServer){
			networkView.RPC("ShuffleSound",RPCMode.Others);
		}
		PlayRandomSound(shuffle_sound, 0.5f);		
	}
	
	public void Shuffle(){
		if(Time.time < last_shuffle_time + SHUFFLE_DELAY){
			return;
		}
		last_shuffle_time = Time.time;
		ShuffleSound();
		RandomizeCards();
	}
	
	[RPC]
	void ImpactSound(float volume){
		if(Network.isServer){
			networkView.RPC("ImpactSound",RPCMode.Others,volume);
		}
		PlayRandomSound(impact_sound, volume*0.3f);		
	}
	
	void OnCollisionEnter(Collision info) {
		if(info.relativeVelocity.magnitude > 1.0f && Time.time > last_sound_time + PHYSICS_SOUND_DELAY) { 
			float volume = info.relativeVelocity.magnitude*0.1f;
			ImpactSound(volume);
			last_sound_time = Time.time;
		}	
		if(Network.isServer){
			if(info.collider.GetComponent<DeckScript>()){
				ObjectManagerScript.Instance().NotifyDeckHitDeck(gameObject, info.collider.gameObject);
			}
		}
	}
	
	// Use this for initialization
	void Start () {
	}
	
	public void Fill(string deck_name){
		cards_ = new List<int>(CardManagerScript.Instance().GetDeckCards(deck_name));
		num_cards_ = cards_.Count;
		RandomizeCards();
		RegenerateEndCardIDs();
		RegenerateEndCards();
	}
	
	void RegenerateEndCardIDs() {
		top_card_id_ = -1;
		bottom_card_id_ = -1;
		if(num_cards_ > 1){
			top_card_id_ = cards_[0];
		}		
		if(num_cards_ > 0){
			bottom_card_id_ = cards_[num_cards_-1];
		}
	}
	
	void RegenerateEndCards() {
		if(top_card_){
			GameObject.Destroy(top_card_);
			top_card_ = null;
		}
		if(bottom_card_){
			GameObject.Destroy(bottom_card_);
			bottom_card_ = null;
		}
		if(top_card_id_ != -1){
			var pos = transform.FindChild("bottom_card").transform.position;
			var rot = transform.FindChild("bottom_card").transform.rotation;
			pos += transform.rotation * new Vector3(0,(num_cards_*0.013f+0.1f)*transform.localScale.y,0);
			top_card_ = CreateCardFacade(top_card_id_,pos,rot);
		}		
		if(bottom_card_id_ != -1){
			var pos = transform.FindChild("bottom_card").transform.position;
			var rot = transform.FindChild("bottom_card").transform.rotation;
			bottom_card_ = CreateCardFacade(bottom_card_id_,pos,rot);
		}
	}
	
	GameObject CreateCardFacade(int card_id, Vector3 pos, Quaternion rot){
		var card = (GameObject)GameObject.Instantiate(card_facade_prefab, pos, rot);
		card.transform.parent = transform;
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
		if(num_cards_ == 0){
			return null;
		}
		GameObject card;
		int card_id = -1;
		if(top && top_card_){
			card = top_card_;
			card_id = cards_[0];
			cards_.RemoveAt(0);
		} else {
			card = bottom_card_;
			card_id = cards_[num_cards_-1];
			cards_.RemoveAt(num_cards_-1);
		}
		--num_cards_;
		RegenerateEndCardIDs();
		RegenerateEndCards();
		
		var new_card = (GameObject)Network.Instantiate(card_prefab, card.transform.position, card.transform.rotation, 0);
		new_card.GetComponent<CardScript>().Prepare(card_id);
		GameObject.Destroy(card);
		if(num_cards_ == 1){
			TakeCard(false);
		}
		return new_card;
	}
	
	public void AddCard(bool top, int id){
		if(top){
			cards_.Insert(0, id);
		} else {
			cards_.Add(id);
		}
		++num_cards_;
		RegenerateEndCardIDs();
		RegenerateEndCards();
	}
	
	// Update is called once per frame
	void Update () {
		transform.FindChild("default").localScale = new Vector3(1,Mathf.Max(2,num_cards_) * CARD_THICKNESS_MULT,1);	
		if(GetComponent<GrabbableScript>().held_by_player_ == -1){
			rigidbody.mass = Mathf.Max(1,num_cards_) * DECK_MASS_PER_CARD;
		}
		if(top_card_ && bottom_card_){
			var the_collider = GetComponent<BoxCollider>();
			the_collider.center = (top_card_.transform.localPosition + bottom_card_.transform.localPosition) * 0.5f;
			the_collider.extents = new Vector3(the_collider.extents.x,Vector3.Distance(top_card_.transform.position, bottom_card_.transform.position)*0.5f+0.1f,the_collider.extents.z);
		}
		if(num_cards_ <= 1){
			transform.FindChild("default").renderer.enabled = false;
			collider.enabled = false;
		} else {
			transform.FindChild("default").renderer.enabled = true;
			collider.enabled = true;
		}
	}
	
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		if(stream.isWriting) {
			int top_card_id = top_card_id_;
			stream.Serialize(ref top_card_id);
			int bottom_card_id = bottom_card_id_;
			stream.Serialize(ref bottom_card_id);
			int num_cards = num_cards_;
			stream.Serialize(ref num_cards);
		} else {
			int top_card_id = -1;
			stream.Serialize(ref top_card_id);
			top_card_id_ = top_card_id;
			int bottom_card_id = -1;
			stream.Serialize(ref bottom_card_id);
			bottom_card_id_ = bottom_card_id;
			int num_cards = -1;
			stream.Serialize(ref num_cards);
			num_cards_ = num_cards;
			RegenerateEndCards();
		}
	}	
}
