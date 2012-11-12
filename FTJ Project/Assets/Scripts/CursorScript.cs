using UnityEngine;
using System.Collections;

public class CursorScript : MonoBehaviour {
	const float CURSOR_INERTIA = 0.001f;
	const float HOLD_FORCE = 10000.0f;
	const float MAX_DICE_VEL = 15.0f;
	Color color_;
	GameObject held_;
	
	void SetColor(Vector3 color){
		color_ = new Color(color.x, color.y, color.z);
		Transform pointer = transform.FindChild("Pointer");
		Transform pointer_tint = pointer.FindChild("pointer_tint");
		Transform default_obj = pointer_tint.FindChild("default");
		default_obj.renderer.material.color = color_;	
	}
	
	void Start () {
		if(networkView.isMine){
			SetColor(new Vector3(Random.Range(0.0f,1.0f),
								 Random.Range(0.0f,1.0f),
								 Random.Range(0.0f,1.0f)));
		}
	}
	
	void Update () {
		if(networkView.isMine){
			Vector3 pos = new Vector3();
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit raycast_hit = new RaycastHit();
				if(Physics.Raycast(ray, out raycast_hit, 100.0f, 1 << 8)){
					pos = raycast_hit.point - ray.direction;
				}
			}
			
			if(Input.GetMouseButtonDown(0) && Network.isServer){
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit raycast_hit = new RaycastHit();
				if(Physics.Raycast(ray, out raycast_hit)){
					if(raycast_hit.collider.gameObject.layer == 10){
						int id = raycast_hit.collider.gameObject.GetComponent<DiceScript>().id_;
						ConsoleScript.Log ("Clicked on die with id #: " + id);
						//held_ = raycast_hit.collider.gameObject;
					}
				}
			}
			if(Input.GetMouseButtonUp(0)){
				if(held_){
					if(held_.rigidbody.velocity.magnitude > MAX_DICE_VEL){
						held_.rigidbody.velocity = held_.rigidbody.velocity.normalized * MAX_DICE_VEL;
					}
					held_.rigidbody.angularVelocity = new Vector3(Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f) * 100.0f);			
				
					held_ = null;
				}
			}		
			rigidbody.position = pos;
			transform.position = pos;
		}
	}
	
	void FixedUpdate() {
		if(networkView.isMine){
			if(held_){
				held_.rigidbody.AddForce((transform.position - held_.rigidbody.position) * Time.deltaTime * HOLD_FORCE);
				held_.rigidbody.velocity *= 0.8f;			
				held_.rigidbody.angularVelocity *= 0.9f;			
				held_.rigidbody.WakeUp();
			}
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		// Send data to server
		if (stream.isWriting)
		{
			Vector3 color = new Vector3(color_.r, color_.g, color_.b);
			stream.Serialize(ref color);
		}
		// Read data from remote client
		else
		{
			Vector3 color = new Vector3();
			stream.Serialize(ref color);
			SetColor(color);
		}
	}
}
