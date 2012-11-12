using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardScript : MonoBehaviour {
	public GameObject[] dice_prefabs;	
	List<GameObject> dice_objects = new List<GameObject>();	
	List<GameObject> cursor_objects = new List<GameObject>();
	const float HOLD_FORCE = 10000.0f;
	const float MAX_DICE_VEL = 15.0f;
	
	public void ClientClickedOnDie(int die_id, int player_id){
		ConsoleScript.Log("Player "+player_id+" clicked on die "+die_id);
		foreach(GameObject die in dice_objects){
			DiceScript dice_script = die.GetComponent<DiceScript>();
			if(dice_script.id_ == die_id){
				dice_script.held_by_player_ = player_id;
			}
		}
	}
	
	public void ClientReleasedMouse(int player_id){
		GameObject held = null;
		foreach(GameObject die in dice_objects){
			if(die.GetComponent<DiceScript>().held_by_player_ == player_id){
				held = die;
			}
		}
		if(held){
			if(held.rigidbody.velocity.magnitude > MAX_DICE_VEL){
				held.rigidbody.velocity = held.rigidbody.velocity.normalized * MAX_DICE_VEL;
			}
			held.rigidbody.angularVelocity = new Vector3(Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f) * 100.0f);			
			held.GetComponent<DiceScript>().held_by_player_ = -1;
		}
	}
	
	public void RegisterCursorObject(GameObject obj) {
		cursor_objects.Add(obj);
	}
	
	public void UnRegisterCursorObject(GameObject obj) {
		cursor_objects.Remove(obj);
	}
	
	// Use this for initialization
	void Start () {
		if(networkView.isMine){
			int next_id = 0;
			Transform dice_spawns = transform.Find("DiceSpawns");
			foreach(Transform child in dice_spawns.transform){
				GameObject dice_object = (GameObject)Network.Instantiate(dice_prefabs[Random.Range(0,dice_prefabs.Length)], child.position, Quaternion.identity, 0);
				dice_object.GetComponent<DiceScript>().id_ = next_id;
				dice_objects.Add(dice_object);
				next_id++;
			}
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
