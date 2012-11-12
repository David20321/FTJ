using UnityEngine;
using System.Collections;

public class BoardScript : MonoBehaviour {
	public GameObject[] dice_objects;	
	
	// Use this for initialization
	void Start () {
		if(networkView.isMine){
			Transform dice_spawns = transform.Find("DiceSpawns");
			foreach(Transform child in dice_spawns.transform){
				Network.Instantiate(dice_objects[Random.Range(0,dice_objects.Length)], child.position, Quaternion.identity, 0);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
