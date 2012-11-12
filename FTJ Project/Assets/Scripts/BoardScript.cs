using UnityEngine;
using System.Collections;

public class BoardScript : MonoBehaviour {
	public GameObject[] dice_objects;	
	
	// Use this for initialization
	void Start () {
		if(networkView.isMine){
			int next_id = 0;
			Transform dice_spawns = transform.Find("DiceSpawns");
			foreach(Transform child in dice_spawns.transform){
				GameObject dice_object = (GameObject)Network.Instantiate(dice_objects[Random.Range(0,dice_objects.Length)], child.position, Quaternion.identity, 0);
				dice_object.GetComponent<DiceScript>().id_ = next_id;
				next_id++;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
