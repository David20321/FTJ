using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardScript : MonoBehaviour {
	public GameObject[] dice_prefabs;
	public GameObject[] token_prefabs;
	public GameObject deck_prefab;		
	
	public void SpawnDice() {
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
		if(ObjectManagerScript.Instance()){
			ObjectManagerScript.Instance().RegisterBoardObject(gameObject);
		}
	}
	
	void OnDestroy() {
		if(ObjectManagerScript.Instance()){
			ObjectManagerScript.Instance().UnRegisterBoardObject();
		}
	}
}
