using UnityEngine;
using System.Collections;

public class ZoomCameraScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		var board_script = BoardScript.Instance();
		if(board_script){
			var cursor = board_script.GetMyCursorObject();
			if(cursor){
				transform.LookAt(cursor.transform.position, GameObject.Find("Main Camera").transform.up);
				//transform.up = GameObject.Find("Main Camera").transform.up;
			}
		}
	}
}
