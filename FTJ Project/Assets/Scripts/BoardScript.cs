using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardScript : MonoBehaviour {
	public GameObject[] dice_prefabs;
	public GameObject[] token_prefabs;
	public GameObject deck_prefab;		
	List<GameObject> grabbable_objects = new List<GameObject>();	
	List<GameObject> cursor_objects = new List<GameObject>();
	const float HOLD_FORCE = 10000.0f;
	const float HOLD_LINEAR_DAMPENING = 0.8f;
	const float HOLD_ANGULAR_DAMPENING = 0.9f;
	const float MAX_DICE_VEL = 15.0f;
	const float DICE_ANG_SPEED = 300.0f;
	
	public void ClientGrab(int grabbed_id, int player_id){
		ConsoleScript.Log("Player "+player_id+" clicked on grabbable "+grabbed_id);
		// Check if client is already holding dice or tokens
		bool holding_dice = false;
		bool holding_token = false;
		foreach(GameObject grabbable in grabbable_objects){
			GrabbableScript grabbable_script = grabbable.GetComponent<GrabbableScript>();
			if(grabbable_script.held_by_player_ == player_id){
				if(grabbable.GetComponent<TokenScript>()){
					holding_token = true;
				}
				if(grabbable.GetComponent<DiceScript>()){
					holding_dice = true;
				}
			}
		}
		// See if client can grab object given already-grabbed objects
		foreach(GameObject grabbable in grabbable_objects){
			GrabbableScript grabbable_script = grabbable.GetComponent<GrabbableScript>();
			if(grabbable_script.id_ == grabbed_id){
				if((grabbable.GetComponent<DiceScript>() && !holding_token) ||
				   (grabbable.GetComponent<TokenScript>() && !holding_dice && !holding_token))
			    {
					grabbable_script.held_by_player_ = player_id;
				}
			}
		}
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
				if(grabbable.rigidbody.velocity.magnitude > MAX_DICE_VEL){
					grabbable.rigidbody.velocity = grabbable.rigidbody.velocity.normalized * MAX_DICE_VEL;
				}
				if(grabbable.GetComponent<DiceScript>()){
					grabbable.rigidbody.angularVelocity = new Vector3(Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f)) * DICE_ANG_SPEED;			
				}
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
			SpawnDice();
		}
	}
	
	void SpawnDice() {
		int next_id = 0;
		Transform dice_spawns = transform.Find("DiceSpawns");
		foreach(Transform child in dice_spawns.transform){
			GameObject dice_object = (GameObject)Network.Instantiate(dice_prefabs[Random.Range(0,dice_prefabs.Length)], child.position, Quaternion.identity, 0);
			dice_object.GetComponent<GrabbableScript>().id_ = next_id;
			next_id++;
		}
		Transform token_spawns = transform.Find("TokenSpawns");
		foreach(Transform child in token_spawns.transform){
			GameObject token_object = (GameObject)Network.Instantiate(token_prefabs[Random.Range(0,token_prefabs.Length)], child.position, Quaternion.identity, 0);
			token_object.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
			token_object.GetComponent<GrabbableScript>().id_ = next_id;
			next_id++;
		}
		//Transform deck_spawn = transform.Find("deck_spawn");
		//GameObject deck_object = (GameObject)Network.Instantiate(deck_prefab, deck_spawn.transform.position, deck_spawn.transform.rotation, 0);
	}
	
	void Start () {
		if(networkView.isMine){
			SpawnDice();
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
		AssignTokenColors();
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
						var held_rigidbody = grabbable.rigidbody;
						held_rigidbody.AddForce((holder.transform.position - held_rigidbody.position) * Time.deltaTime * HOLD_FORCE);
						held_rigidbody.velocity *= HOLD_LINEAR_DAMPENING;			
						held_rigidbody.angularVelocity *= HOLD_ANGULAR_DAMPENING;			
						held_rigidbody.WakeUp();
					} else {
						ConsoleScript.Log("Could not find cursor for player: "+held_by_player);
					}
				}
			}
		}
	}
	
	public static BoardScript Instance() {
		if(!GameObject.Find("Board(Clone)")){
			return null;
		}
		return GameObject.Find("Board(Clone)").GetComponent<BoardScript>();
	}
}
