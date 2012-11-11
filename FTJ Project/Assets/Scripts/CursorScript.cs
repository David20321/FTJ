using UnityEngine;
using System.Collections;

public class CursorScript : MonoBehaviour {
	const float CURSOR_INERTIA = 0.001f;
	const float HOLD_FORCE = 10000.0f;
	const float MAX_DICE_VEL = 15.0f;
	Vector3 pos;
	Vector3 target_pos;
	GameObject held;

	void Start () {
		renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),1.0f);
	}
	
	void Update () {
		if(networkView.isMine){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit raycast_hit = new RaycastHit();
			if(Physics.Raycast(ray, out raycast_hit, 100.0f, 1 << 8)){
				target_pos = raycast_hit.point - ray.direction;
			}
			pos = target_pos;
		} else {
			pos = Vector3.Lerp(target_pos, pos, Mathf.Pow(CURSOR_INERTIA, Time.deltaTime));
		}		
		transform.position = pos;
		
		if(Input.GetMouseButtonDown(0)){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit raycast_hit = new RaycastHit();
			if(Physics.Raycast(ray, out raycast_hit)){
				if(raycast_hit.collider.gameObject.layer == 10){
					//ConsoleScript.Log ("Clicked on die");
					held = raycast_hit.collider.gameObject;
				}
			}
		}
		
		if(Input.GetMouseButtonUp(0)){
			if(held){
				if(held.rigidbody.velocity.magnitude > MAX_DICE_VEL){
					held.rigidbody.velocity = held.rigidbody.velocity.normalized * MAX_DICE_VEL;
				}
				held.rigidbody.angularVelocity = new Vector3(Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f) * 100.0f);			
			
				held = null;
			}
		}		
	}
	
	void FixedUpdate() {
		if(held){
			held.rigidbody.AddForce((pos - held.rigidbody.position) * Time.deltaTime * HOLD_FORCE);
			held.rigidbody.velocity *= 0.8f;			
			held.rigidbody.angularVelocity *= 0.9f;			
			held.rigidbody.WakeUp();
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        if (stream.isWriting) {
            stream.Serialize(ref target_pos);
        } else {
        	stream.Serialize(ref target_pos);
        }
    }
}
