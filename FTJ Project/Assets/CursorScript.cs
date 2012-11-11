using UnityEngine;
using System.Collections;

public class CursorScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(networkView.isMine){
			Ray ray = GameObject.Find("Main Camera").camera.ScreenPointToRay(Input.mousePosition);
			transform.position = ray.origin + ray.direction * 10.0f;
		}
	}
}
