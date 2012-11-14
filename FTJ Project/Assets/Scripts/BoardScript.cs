using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardScript : MonoBehaviour {
	public GameObject[] dice_prefabs;	
	public GameObject[] token_prefabs;	
	List<GameObject> dice_objects = new List<GameObject>();	
	List<GameObject> cursor_objects = new List<GameObject>();
	const float HOLD_FORCE = 10000.0f;
	const float MAX_DICE_VEL = 15.0f;
	
	public void ClientClickedOnDie(int die_id, int player_id){
		ConsoleScript.Log("Player "+player_id+" clicked on die "+die_id);
		bool holding_dice = false;
		bool holding_token = false;
		foreach(GameObject die in dice_objects){
			DiceScript dice_script = die.GetComponent<DiceScript>();
			if(dice_script.held_by_player_ == player_id){
				switch(dice_script.type_){
					case DiceScript.Type.TOKEN:
						holding_token = true;
						break;
					case DiceScript.Type.DIE:
						holding_dice = true;
						break;
				}
			}
		}
		foreach(GameObject die in dice_objects){
			DiceScript dice_script = die.GetComponent<DiceScript>();
			if(dice_script.id_ == die_id){
				if((dice_script.type_ == DiceScript.Type.DIE && !holding_token) ||
				   (dice_script.type_ == DiceScript.Type.TOKEN && !holding_dice && !holding_token))
			    {
					dice_script.held_by_player_ = player_id;
				}
			}
		}
	}
	
	public void ClientReleasedMouse(int player_id){
		GameObject held = null;
		foreach(GameObject die in dice_objects){
			if(die.GetComponent<DiceScript>().held_by_player_ == player_id){
				if(die.rigidbody.velocity.magnitude > MAX_DICE_VEL){
					die.rigidbody.velocity = die.rigidbody.velocity.normalized * MAX_DICE_VEL;
				}
				var dice_script = die.GetComponent<DiceScript>();
				if(dice_script.type_ == DiceScript.Type.DIE){
					die.rigidbody.angularVelocity = new Vector3(Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f)) * 100.0f;			
				}
				dice_script.held_by_player_ = -1;
			}
		}
		if(held){
			
		}
	}
	
	public void RegisterCursorObject(GameObject obj) {
		cursor_objects.Add(obj);
	}
	
	public void UnRegisterCursorObject(GameObject obj) {
		cursor_objects.Remove(obj);
	}
	
	[RPC]
	public void RecoverDice() {
		if(!Network.isServer){
			networkView.RPC("RecoverDice", RPCMode.Server);
			return;
		} else {
			foreach(GameObject die in dice_objects){
				Network.Destroy(die);
			}
			dice_objects.Clear();
			SpawnDice();
		}
	}
	
	void SpawnDice() {
		int next_id = 0;
		Transform dice_spawns = transform.Find("DiceSpawns");
		foreach(Transform child in dice_spawns.transform){
			GameObject dice_object = (GameObject)Network.Instantiate(dice_prefabs[Random.Range(0,dice_prefabs.Length)], child.position, Quaternion.identity, 0);
			dice_object.GetComponent<DiceScript>().id_ = next_id;
			dice_objects.Add(dice_object);
			next_id++;
		}
		Transform token_spawns = transform.Find("TokenSpawns");
		foreach(Transform child in token_spawns.transform){
			GameObject token_object = (GameObject)Network.Instantiate(token_prefabs[Random.Range(0,token_prefabs.Length)], child.position, Quaternion.identity, 0);
			token_object.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
			token_object.GetComponent<DiceScript>().id_ = next_id;
			dice_objects.Add(token_object);
			next_id++;
		}
	}
	
	// Use this for initialization
	void Start () {
		if(networkView.isMine){
			SpawnDice();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void FixedUpdate() {
		if(Network.isServer){
			foreach(GameObject die in dice_objects){
				int held_by_player = die.GetComponent<DiceScript>().held_by_player_;
				if(held_by_player != -1){
					GameObject holder = null;
					foreach(GameObject cursor in cursor_objects){
						if(cursor.GetComponent<CursorScript>().id() == held_by_player){
							holder = cursor;
						}
					}
					if(holder){
						die.rigidbody.AddForce((holder.transform.position - die.rigidbody.position) * Time.deltaTime * HOLD_FORCE);
						die.rigidbody.velocity *= 0.8f;			
						die.rigidbody.angularVelocity *= 0.9f;			
						die.rigidbody.WakeUp();
					} else {
						ConsoleScript.Log("Could not find cursor for player: "+held_by_player);
					}
				}
			}
		}
	}
	
	public static BoardScript Instance() {
		return GameObject.Find("Board(Clone)").GetComponent<BoardScript>();
	}
}
