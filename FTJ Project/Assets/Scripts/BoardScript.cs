using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardScript : MonoBehaviour {
	public GameObject dice_prefab;
	public GameObject token_prefab;
	public GameObject deck_prefab;		
	public GameObject silver_coin_prefab;	
	public GameObject gold_coin_prefab;		
	
	public void SpawnDice() {
		Transform dice_spawns = transform.Find("DiceSpawns");
		foreach(Transform child in dice_spawns.transform){
			GameObject dice_object = (GameObject)Network.Instantiate(dice_prefab, child.position, Quaternion.identity, 0);
		}
		Transform token_spawns = transform.Find("TokenSpawns");
		var count = 0;
		foreach(Transform child in token_spawns.transform){
			GameObject token_object = (GameObject)Network.Instantiate(token_prefab, child.position, Quaternion.identity, 0);
			token_object.GetComponent<ParentTokenScript>().AssignMesh(count);
			token_object.GetComponent<ParentTokenScript>().AssignColor(count);
			++count;
			//token_object.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
		}
		Transform silver_coin_spawns = transform.Find("SilverCoinSpawns");
		foreach(Transform child in silver_coin_spawns.transform){
			Network.Instantiate(silver_coin_prefab, child.position, Quaternion.identity, 0);
		}
		Transform gold_coin_spawns = transform.Find("GoldCoinSpawns");
		foreach(Transform child in gold_coin_spawns.transform){
			Network.Instantiate(gold_coin_prefab, child.position, Quaternion.identity, 0);
		}
		Transform deck_spawns = transform.Find("DeckSpawns");
		foreach(Transform child in deck_spawns.transform){
			GameObject deck_object = (GameObject)Network.Instantiate(deck_prefab, child.position, child.rotation, 0);
			deck_object.GetComponent<DeckScript>().Fill(child.name);
		}	
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
