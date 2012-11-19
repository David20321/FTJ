using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectManagerScript : MonoBehaviour {	
	public GameObject deck_prefab;
	List<GameObject> grabbable_objects = new List<GameObject>();	
	List<GameObject> cursor_objects = new List<GameObject>();
	GameObject board_object = null;
	const float HOLD_FORCE = 20000.0f;
	const float ANGULAR_FORCE = 400.0f;
	const float HOLD_LINEAR_DAMPENING = 0.4f;
	const float HOLD_ANGULAR_DAMPENING = 0.4f;
	const float MAX_DICE_VEL = 15.0f;
	const float DICE_ANG_SPEED = 300.0f;
	const float DECK_MERGE_THRESHOLD = 0.4f;
	const float SHAKE_THRESHOLD = 1.0f;
	int free_id = 0;
	
	public void RegisterBoardObject(GameObject obj){
		board_object = obj;
	}
	
	public void UnRegisterBoardObject(){
		board_object = null;
	}
	
	int GetRotateFromGrabbable(GameObject grabbable){
		var forward = grabbable.transform.forward;
		if(!grabbable.GetComponent<DeckScript>()){
			forward *= -1.0f;
		}
		float ang = Mathf.Atan2(forward.z, -forward.x)*180.0f/Mathf.PI;
		int rotate = -1;
		if(ang >=-45.0f && ang < 45.0f){
			rotate = 1;
		} else if(ang >= 45.0f && ang < 135.0f){
			rotate = 2;
		} else if(ang >= -135.0f && ang < -45.0f){
			rotate = 0;
		} else {
			rotate = 3;
		}
		return rotate;
	}
	
	public void ClientGrab(int grabbed_id, int player_id){
		// Check if client is already holding dice or tokens
		bool holding_anything = false;
		bool holding_anything_but_dice = false;
		foreach(GameObject grabbable in grabbable_objects){
			GrabbableScript grabbable_script = grabbable.GetComponent<GrabbableScript>();
			if(grabbable_script.held_by_player_ == player_id){
				holding_anything = true;
				if(!grabbable.GetComponent<DiceScript>()){
					holding_anything_but_dice = true;
				}
			}
		}
		CursorScript cursor_script = null;
		foreach(GameObject cursor in cursor_objects){
			if(cursor.GetComponent<CursorScript>().id() == player_id){
				cursor_script = cursor.GetComponent<CursorScript>();
			}
		}
		// See if client can grab object given already-grabbed objects
		foreach(GameObject grabbable in grabbable_objects){
			GrabbableScript grabbable_script = grabbable.GetComponent<GrabbableScript>();
			if(grabbable_script.id_ == grabbed_id){
				if(grabbable_script.held_by_player_ == player_id){
					return;
				}
				if((grabbable.GetComponent<DiceScript>() && !holding_anything_but_dice) ||
				   (grabbable.GetComponent<TokenScript>() && !holding_anything)||
				   (grabbable.GetComponent<ParentTokenScript>() && !holding_anything)||
				   (grabbable.GetComponent<DeckScript>() && !holding_anything) ||
			       (grabbable.GetComponent<CardScript>() && !holding_anything)||
			       (grabbable.GetComponent<CoinScript>() && !holding_anything))
			    {
					grabbable_script.held_by_player_ = player_id;
					if(grabbable.GetComponent<DiceScript>()){
						grabbable.GetComponent<DiceScript>().PickUpSound();
					}
					if(grabbable.GetComponent<CoinScript>()){
						grabbable.GetComponent<CoinScript>().PickUpSound();
					}
					if(grabbable.GetComponent<DeckScript>()){
						cursor_script.SetCardFaceUp((grabbable.transform.up.y > 0.0f));
						cursor_script.SetCardRotated(GetRotateFromGrabbable(grabbable));
						grabbable.GetComponent<DeckScript>().PickUpSound();
					}
					if(grabbable.GetComponent<CardScript>()){
						cursor_script.SetCardFaceUp((grabbable.transform.up.y < 0.0f));
						cursor_script.SetCardRotated(GetRotateFromGrabbable(grabbable));
						grabbable.GetComponent<CardScript>().PickUpSound();
					}
					if(grabbable.GetComponent<TokenScript>()){
						grabbable.GetComponent<TokenScript>().PickUpSound();
					}
					if(grabbable.GetComponent<ParentTokenScript>()){
						grabbable.GetComponent<ParentTokenScript>().PickUpSound();
						cursor_script.SetCardRotated(GetRotateFromGrabbable(grabbable));
					}
					grabbable.rigidbody.mass = 0.2f;
				}
			}
		}
	}
	
	
	public void ClientCardPeel(int grabbed_id, int player_id){
		// Return if player is already holding something
		foreach(GameObject grabbable in grabbable_objects){
			if(grabbable.GetComponent<GrabbableScript>().held_by_player_ == player_id){
		   		return;
		    }
		}
		// Find the deck, return if can't find it
		GameObject deck = null;
		foreach(GameObject grabbable in grabbable_objects){
			if(grabbable.GetComponent<GrabbableScript>().id_ == grabbed_id &&
			   grabbable.GetComponent<DeckScript>())
		    {
		   		deck = grabbable;
		    }
		}
		if(!deck){
			return;
		}
		// Grab whatever card is on top of the deck, depending on which way
		// the deck is facing
		GameObject card = null;
		if((deck.rigidbody.rotation * new Vector3(0,1,0)).y >= 0.0f){
			card = deck.GetComponent<DeckScript>().TakeCard(true);
		} else {
			card = deck.GetComponent<DeckScript>().TakeCard(false);
		}
		card.GetComponent<GrabbableScript>().held_by_player_ = player_id;
		CursorScript cursor_script = null;
		foreach(GameObject cursor in cursor_objects){
			if(cursor.GetComponent<CursorScript>().id() == player_id){
				cursor_script = cursor.GetComponent<CursorScript>();
			}
		}
		cursor_script.SetCardFaceUp((card.transform.up.y < 0.0f));
		cursor_script.SetCardRotated(GetRotateFromGrabbable(card));
		card.GetComponent<CardScript>().PickUpSound();
	}
	
	public GameObject GetMyCursorObject() {
		foreach(var cursor in cursor_objects){
			if(cursor.GetComponent<CursorScript>().id() == Net.GetMyID()){ 
				return cursor;
			}
		}
		return null;
	}
	
	public void ClientReleasedMouse(int player_id){
		foreach(GameObject grabbable in grabbable_objects){
			var grabbable_script = grabbable.GetComponent<GrabbableScript>();
			if(grabbable_script.held_by_player_ == player_id){
				grabbable.rigidbody.velocity = new Vector3(grabbable.rigidbody.velocity.x, -5.0f, grabbable.rigidbody.velocity.z);
				if(grabbable.rigidbody.velocity.magnitude > MAX_DICE_VEL){
					grabbable.rigidbody.velocity = grabbable.rigidbody.velocity.normalized * MAX_DICE_VEL;
				}
				if(grabbable.GetComponent<DiceScript>()){
					grabbable.rigidbody.angularVelocity = new Vector3(Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f)) * DICE_ANG_SPEED;			
				}
				grabbable.rigidbody.mass = 1.0f;
				grabbable_script.held_by_player_ = -1;
			}
		}
	}
	
	public void RegisterCursorObject(GameObject obj) {
		cursor_objects.Add(obj);
	}
	
	public void UnRegisterCursorObject(GameObject obj) {
		cursor_objects.Remove(obj);
	}
	
	public void RegisterGrabbableObject(GameObject obj) {
		grabbable_objects.Add(obj);
		obj.GetComponent<GrabbableScript>().id_ = free_id;
		++free_id;
	}
	
	public void UnRegisterGrabbableObject(GameObject obj) {
		grabbable_objects.Remove(obj);
	}
	
	[RPC]
	void DestroyObject(NetworkViewID id){
		GameObject.Destroy(NetworkView.Find(id).gameObject);
	}
	
	[RPC]
	public void RecoverDice() {
		if(!Network.isServer){
			networkView.RPC("RecoverDice", RPCMode.Server);
			return;
		} else {
			foreach(GameObject grabbable in grabbable_objects){
				networkView.RPC("DestroyObject",RPCMode.AllBuffered,grabbable.networkView.viewID);
			}
			board_object.GetComponent<BoardScript>().SpawnDice();
			NetUIScript.Instance().SpawnHealthTokens();
		}
	}
	
	void AssignTokenColors() {
		// Create list of tokens
		var token_objects = new List<GameObject>();
		foreach(GameObject grabbable in grabbable_objects){
			if(grabbable.GetComponent<TokenScript>()){
				token_objects.Add(grabbable);
			}
		}
		var players = PlayerListScript.Instance().GetPlayerInfoList();
		// Assign owners to tokens as needed
		if(Network.isServer){
			var used_id = new HashSet<int>();
			foreach(GameObject token in token_objects){
				used_id.Add(token.GetComponent<TokenScript>().owner_id_);
			}
			foreach(GameObject token in token_objects){
				var token_script = token.GetComponent<TokenScript>();
				if(!players.ContainsKey(token_script.owner_id_)){
					foreach(var pair in players){
						if(!used_id.Contains(pair.Key)){
							token_script.owner_id_ = pair.Key;
							used_id.Add(pair.Key);
						}
					}
				}
			}
		}
		// Assign colors to tokens based on owner
		foreach(GameObject token in token_objects){
			var token_script = token.GetComponent<TokenScript>();
			if(players.ContainsKey(token_script.owner_id_)){
				token.renderer.material.color = players[token_script.owner_id_].color_;
			} else {
				token.renderer.material.color = Color.white;
			}
		}
	}
	
	void Update () {
	}
	
	void UpdatePhysicsState(GameObject grabbable, GameObject holder){
		var held_rigidbody = grabbable.rigidbody;
		var target_position = holder.transform.position;
		var cursor_script = holder.GetComponent<CursorScript>();
		if(!cursor_script.tapping()){
			target_position.y += 0.5f;
		} else {
			target_position.y -= 1.3f;
		}
		if(grabbable.GetComponent<DeckScript>() || grabbable.GetComponent<CardScript>() || grabbable.GetComponent<ParentTokenScript>()){
			target_position.y += 0.5f;
			Quaternion target_rotation = Quaternion.identity;
			if(grabbable.GetComponent<DeckScript>() || grabbable.GetComponent<CardScript>()){
				if(grabbable.GetComponent<DeckScript>()){
					target_rotation = Quaternion.AngleAxis(180,new Vector3(0,1,0)) * target_rotation;
					target_rotation = Quaternion.AngleAxis(180,new Vector3(0,0,1)) * target_rotation;
				}
				if(cursor_script.card_face_up()){
					target_rotation = Quaternion.AngleAxis(180,new Vector3(0,0,1))*target_rotation;
				}
				target_rotation = Quaternion.AngleAxis(cursor_script.card_rotated() * 90, new Vector3(0,1,0)) * target_rotation;
			}
			if(grabbable.GetComponent<ParentTokenScript>()){
				target_rotation = Quaternion.AngleAxis(cursor_script.card_rotated() * 90, new Vector3(0,1,0)) * target_rotation;
			}
			Quaternion offset = target_rotation * Quaternion.Inverse(held_rigidbody.rotation);
			float angle;
			Vector3 offset_vec3;
			offset.ToAngleAxis(out angle, out offset_vec3);
			if(angle > 180){
				angle -= 360;
			}
			if(angle < -180){
				angle += 360;
			}
			if(angle != 0.0f){
				offset_vec3 *= angle;
				float mult = 1.0f;
				if(grabbable.GetComponent<ParentTokenScript>()){
					mult = 0.1f;
				}
				held_rigidbody.AddTorque(offset_vec3 * Time.deltaTime * ANGULAR_FORCE * mult * held_rigidbody.mass);
			}
		}
		if(!cursor_script.tapping() && Vector3.Dot(target_position - held_rigidbody.position, held_rigidbody.velocity) < -SHAKE_THRESHOLD){
			//ConsoleScript.Log("Shake");
			if(grabbable.GetComponent<DiceScript>()){
				for(int i=0; i<10; ++i){
					held_rigidbody.rotation = Quaternion.AngleAxis(Random.Range(0.0f,360.0f),new Vector3(Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f)).normalized) * held_rigidbody.rotation;
				}
				grabbable.GetComponent<DiceScript>().ShakeSound();
			}
			if(grabbable.GetComponent<DeckScript>()){
				grabbable.GetComponent<DeckScript>().Shuffle();
			}
		}
		held_rigidbody.AddForce((target_position - held_rigidbody.position) * Time.deltaTime * HOLD_FORCE * held_rigidbody.mass);
		held_rigidbody.velocity *= HOLD_LINEAR_DAMPENING;			
		held_rigidbody.angularVelocity *= HOLD_ANGULAR_DAMPENING;	
		held_rigidbody.WakeUp();
	}
	
	void FixedUpdate() {
		if(Network.isServer){
			// Move grabbed objects to position of cursor
			foreach(GameObject grabbable in grabbable_objects){
				int held_by_player = grabbable.GetComponent<GrabbableScript>().held_by_player_;
				if(held_by_player != -1){
					GameObject holder = null;
					foreach(GameObject cursor in cursor_objects){
						if(cursor.GetComponent<CursorScript>().id() == held_by_player){
							holder = cursor;
						}
					}
					if(holder){
						UpdatePhysicsState(grabbable, holder);
					} else {
						ConsoleScript.Log("Could not find cursor for player: "+held_by_player);
					}
				}
			}
		}
	}
	
	public static ObjectManagerScript Instance() {
		if(!GameObject.Find("GlobalScriptObject")){
			return null;
		}
		return GameObject.Find("GlobalScriptObject").GetComponent<ObjectManagerScript>();
	}
	
	public void NotifyCardHitDeck(GameObject card, GameObject deck){
		if(card.GetComponent<CardScript>().card_id() == -1 ||
		   deck.GetComponent<DeckScript>().GetCards().Count < 1)
	    {
			return;
		}
		bool facing_same_way = Vector3.Dot(card.transform.up, deck.transform.up) <= 0.0;
		var rel_pos = card.transform.position - deck.transform.position;
		bool close_enough = false;
		if(Mathf.Abs(Vector3.Dot(rel_pos, deck.transform.forward)) < DECK_MERGE_THRESHOLD && 
		   Mathf.Abs(Vector3.Dot(rel_pos, deck.transform.right)) < DECK_MERGE_THRESHOLD &&
		   Mathf.Abs(Vector3.Dot(card.transform.forward, deck.transform.forward)) > 0.5f)
	    {
			close_enough = true;
		}
		if(card.GetComponent<GrabbableScript>().held_by_player_ == -1 && facing_same_way && close_enough){
			bool top = Vector3.Dot(card.transform.position - deck.transform.position, deck.transform.up) >= 0.0;
			deck.GetComponent<DeckScript>().AddCard(top, card.GetComponent<CardScript>().card_id());
			card.GetComponent<CardScript>().SetCardID(-1);
			networkView.RPC("DestroyObject",RPCMode.AllBuffered,card.networkView.viewID);
		}
	}
	
	
	public void NotifyDeckHitDeck(GameObject deck_a, GameObject deck_b){
		if(deck_b.GetComponent<DeckScript>().GetCards().Count == 0 ||
		   deck_a.GetComponent<DeckScript>().GetCards().Count == 0)
	    {
		 	return;  
		}
		bool facing_same_way = Vector3.Dot(deck_a.transform.up, deck_b.transform.up) > 0.0;
		var rel_pos = deck_a.transform.position - deck_b.transform.position;
		bool close_enough = false;
		if(Mathf.Abs(Vector3.Dot(rel_pos, deck_a.transform.forward)) < DECK_MERGE_THRESHOLD && 
		   Mathf.Abs(Vector3.Dot(rel_pos, deck_a.transform.right)) < DECK_MERGE_THRESHOLD &&
		   Mathf.Abs(Vector3.Dot(rel_pos, deck_b.transform.forward)) < DECK_MERGE_THRESHOLD && 
		   Mathf.Abs(Vector3.Dot(rel_pos, deck_b.transform.right)) < DECK_MERGE_THRESHOLD &&
		   Mathf.Abs(Vector3.Dot(deck_a.transform.forward, deck_b.transform.forward)) > 0.5f)
	    {
			close_enough = true;
		}
		if(deck_a.GetComponent<GrabbableScript>().held_by_player_ == -1 && deck_b.GetComponent<GrabbableScript>().held_by_player_ == -1 && facing_same_way && close_enough){
			bool top = Vector3.Dot(deck_a.transform.position - deck_b.transform.position, deck_a.transform.up) <= 0.0;
			var cards = deck_b.GetComponent<DeckScript>().GetCards();
			if(!top){
				foreach(var card in cards){
					deck_a.GetComponent<DeckScript>().AddCard(top, card);
				}
			} else {
				for(int i=cards.Count-1; i>=0; --i){
					deck_a.GetComponent<DeckScript>().AddCard(top, cards[i]);
				}
			}
			cards.Clear();
			networkView.RPC("DestroyObject",RPCMode.AllBuffered,deck_b.networkView.viewID);
		}
	}
	
	public void NotifyCardHitCard(GameObject card_a, GameObject card_b){
		if(card_a.GetComponent<CardScript>().card_id() == -1 ||
		   card_b.GetComponent<CardScript>().card_id() == -1){
			return;
		}
		bool facing_same_way = Vector3.Dot(card_a.transform.up, card_b.transform.up) > 0.0;
		var rel_pos = card_a.transform.position - card_b.transform.position;
		bool close_enough = false;
		if(Mathf.Abs(Vector3.Dot(rel_pos, card_b.transform.forward)) < DECK_MERGE_THRESHOLD && 
		   Mathf.Abs(Vector3.Dot(rel_pos, card_b.transform.right)) < DECK_MERGE_THRESHOLD &&
		   Mathf.Abs(Vector3.Dot(rel_pos, card_a.transform.forward)) < DECK_MERGE_THRESHOLD && 
		   Mathf.Abs(Vector3.Dot(rel_pos, card_a.transform.right)) < DECK_MERGE_THRESHOLD &&
		   Mathf.Abs(Vector3.Dot(card_a.transform.forward, card_b.transform.forward)) > 0.5f)
	    {
			close_enough = true;
		}
		if(card_a.GetComponent<GrabbableScript>().held_by_player_ == -1 && card_b.GetComponent<GrabbableScript>().held_by_player_ == -1 && facing_same_way && close_enough){
			bool top = Vector3.Dot(card_a.transform.position - card_b.transform.position, card_a.transform.up) >= 0.0;
			var deck = (GameObject)Network.Instantiate(deck_prefab, (card_a.transform.position + card_b.transform.position)*0.5f, card_a.transform.rotation,0); 
			deck.transform.rotation = Quaternion.AngleAxis(180,deck.transform.right)*deck.transform.rotation;
			deck.GetComponent<DeckScript>().AddCard(top, card_a.GetComponent<CardScript>().card_id());
			deck.GetComponent<DeckScript>().AddCard(top, card_b.GetComponent<CardScript>().card_id());
			card_a.GetComponent<CardScript>().SetCardID(-1);
			networkView.RPC("DestroyObject",RPCMode.AllBuffered,card_a.networkView.viewID);
			card_b.GetComponent<CardScript>().SetCardID(-1);
			networkView.RPC("DestroyObject",RPCMode.AllBuffered,card_b.networkView.viewID);
		}
	}
}
