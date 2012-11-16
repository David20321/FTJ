using UnityEngine;
using System.Collections;

public class RaycastHitComparator : IComparer
{
    public int Compare(object x, object y)
    {
    	RaycastHit a = (RaycastHit)x;
    	RaycastHit b = (RaycastHit)y;
    	if(a.distance < b.distance){
    		return -1;
    	} else if(b.distance < a.distance){
    		return 1;
    	} else {
    		return 0;
    	}
    }
}

public class CursorScript : MonoBehaviour {
	int id_ = -1;
	float deck_held_time_ = 0.0f;
	int deck_held_id_ = -1;
	const float DECK_HOLD_THRESHOLD = 0.5f;
	
	public int id() {
		return id_;
	}
	
	void SetColor(Color color){
		Transform pointer = transform.FindChild("Pointer");
		Transform pointer_tint = pointer.FindChild("pointer_tint");
		Transform default_obj = pointer_tint.FindChild("default");
		default_obj.renderer.material.color = color;	
	}
	
	void Start () {
		if(networkView.isMine){
			id_ = Net.GetMyID();
		}
		if(ObjectManagerScript.Instance()){
			ObjectManagerScript.Instance().RegisterCursorObject(gameObject);
		}
		Screen.showCursor = false;
	}
	
	void OnDestroy() {
		if(ObjectManagerScript.Instance()){
			ObjectManagerScript.Instance().UnRegisterCursorObject(gameObject);
		}
		Screen.showCursor = true;
	}
		
	[RPC]
	public void TellObjectManagerAboutGrab(int grab_id, int player_id){
		ConsoleScript.Log ("Telling object manager that player "+player_id+" clicked on "+grab_id);
		ObjectManagerScript.Instance().ClientGrab(grab_id, player_id);
	}
	
	[RPC]
	public void TellObjectManagerAboutCardPeel(int grab_id, int player_id){
		ObjectManagerScript.Instance().ClientCardPeel(grab_id, player_id);
	}
	
	[RPC]
	public void TellObjectManagerAboutMouseRelease(int player_id){
		ObjectManagerScript.Instance().ClientReleasedMouse(player_id);
	}
	
	void Grab(int grab_id, int player_id){
		if(!Network.isServer){
			networkView.RPC("TellObjectManagerAboutGrab",RPCMode.Server,grab_id,player_id);
		} else {
			TellObjectManagerAboutGrab(grab_id, player_id);
		}
	}
	
	void Update () {
		var player_list = PlayerListScript.Instance().GetPlayerInfoList();
		if(player_list.ContainsKey(id_)){
			SetColor(player_list[id_].color_);
		}
		if(networkView.isMine){
			var main_camera = GameObject.Find("Main Camera").camera;
			Vector3 pos = new Vector3();
			{
				Ray ray = main_camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit raycast_hit = new RaycastHit();
				if(Physics.Raycast(ray, out raycast_hit, 100.0f, 1 << 8)){
					pos = raycast_hit.point - ray.direction;
				}
			}
			
			if(Input.GetMouseButton(0)){
				Ray ray = main_camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] raycast_hits;
				raycast_hits = Physics.RaycastAll(ray);
				System.Array.Sort(raycast_hits, new RaycastHitComparator());
				int hit_deck_id = -1;
				foreach(RaycastHit hit in raycast_hits){ 
					var hit_obj = hit.collider.gameObject;
					if(hit_obj.layer != LayerMask.NameToLayer("Dice") && 
					   hit_obj.layer != LayerMask.NameToLayer("Tokens") && 
					   hit_obj.layer != LayerMask.NameToLayer("Cards"))
				    {
						continue;
					}
					GrabbableScript grabbable_script = hit_obj.GetComponent<GrabbableScript>();
					if(!grabbable_script){
						hit_obj = hit_obj.transform.parent.gameObject;
						grabbable_script = hit_obj.GetComponent<GrabbableScript>();
					}
					if(hit_obj.GetComponent<DeckScript>()){
						hit_deck_id = grabbable_script.id_;
					}
					if(grabbable_script.held_by_player_ == id_){
						continue;
					}
					if(hit_obj.GetComponent<DeckScript>() && deck_held_time_ > 0.0f && grabbable_script.id_ == deck_held_id_){
						deck_held_time_ += Time.deltaTime;
						if(deck_held_time_ > DECK_HOLD_THRESHOLD){
							Grab(grabbable_script.id_, id_);
						}
					}
					if(!hit_obj.GetComponent<DiceScript>() && !Input.GetMouseButtonDown(0)){
						continue;
					}
					if(hit_obj.GetComponent<DeckScript>()){
						deck_held_time_ = Time.deltaTime;
						deck_held_id_ = grabbable_script.id_;
						continue;
					}					
					Grab(grabbable_script.id_, id_);
				}
				if(hit_deck_id != deck_held_id_ && deck_held_time_ > 0.0f){
					if(!Network.isServer){
						networkView.RPC("TellObjectManagerAboutCardPeel",RPCMode.Server,deck_held_id_,id_);
					} else {
						TellObjectManagerAboutCardPeel(deck_held_id_,id_);
					}
					deck_held_time_ = 0.0f;
				}
			}
			if(Input.GetMouseButtonUp(0)){
				if(!Network.isServer){
					networkView.RPC("TellObjectManagerAboutMouseRelease",RPCMode.Server,id_);
				} else {
					TellObjectManagerAboutMouseRelease(id_);
				}
				deck_held_time_ = 0.0f;
			}
			rigidbody.position = pos;
			transform.position = pos;
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		// Send data to server
		if (stream.isWriting)
		{
			int id = id_;
			stream.Serialize(ref id);
		}
		// Read data from remote client
		else
		{
			int id = id_;
			stream.Serialize(ref id);
			id_ = id;
		}
	}
}
