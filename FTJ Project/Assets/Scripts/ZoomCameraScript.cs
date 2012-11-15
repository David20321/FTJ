using UnityEngine;
using System.Collections;

public class ZoomCameraScript : MonoBehaviour {
	Quaternion old_rotation;
	const float ZOOM_INERTIA = 0.0001f;
	
	// Use this for initialization
	void Start () {
		old_rotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		var cursor = ObjectManagerScript.Instance().GetMyCursorObject();
		if(cursor){
			old_rotation = transform.rotation;
			transform.LookAt(cursor.transform.position, GameObject.Find("Main Camera").transform.up);
			transform.rotation = Quaternion.Lerp(transform.rotation, old_rotation, Mathf.Pow(ZOOM_INERTIA, Time.deltaTime));
			//transform.up = GameObject.Find("Main Camera").transform.up;
		}
	}
}
