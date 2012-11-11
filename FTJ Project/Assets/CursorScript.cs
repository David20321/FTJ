using UnityEngine;
using System.Collections;

public class CursorScript : MonoBehaviour {
	const float CURSOR_INERTIA = 0.001f;
	Vector3 pos;
	Vector3 target_pos;

	void Start () {
	}
	
	void Update () {
		if(networkView.isMine){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			target_pos = ray.origin + ray.direction * 10.0f;
			pos = target_pos;
		} else {
			pos = Vector3.Lerp(target_pos, pos, Mathf.Pow(CURSOR_INERTIA, Time.deltaTime));
		}
		
		transform.position = pos;
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        if (stream.isWriting) {
            stream.Serialize(ref target_pos);
        } else {
        	stream.Serialize(ref target_pos);
        }
    }
}
