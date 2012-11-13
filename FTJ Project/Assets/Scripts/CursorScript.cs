using UnityEngine;
using System.Collections;

public class Net {
	public static int GetMyID(){
		return int.Parse(Network.player.ToString());
	}
};

public class CursorScript : MonoBehaviour {
	int id_ = -1;
	
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
		BoardScript.Instance().RegisterCursorObject(gameObject);
		Screen.showCursor = false;
	}
	
	void OnDestroy() {
		BoardScript.Instance().UnRegisterCursorObject(gameObject);
		Screen.showCursor = true;
	}
	
	
	
	[RPC]
	public void TellBoardAboutDiceClick(int die_id, int player_id){
		BoardScript.Instance().ClientClickedOnDie(die_id, player_id);
	}
	
	[RPC]
	public void TellBoardAboutMouseRelease(int player_id){
		BoardScript.Instance().ClientReleasedMouse(player_id);
	}
	
	void Update () {
		SetColor(PlayerListScript.Instance().GetPlayerInfoList()[id_].color_);
		if(networkView.isMine){
			Vector3 pos = new Vector3();
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit raycast_hit = new RaycastHit();
				if(Physics.Raycast(ray, out raycast_hit, 100.0f, 1 << 8)){
					pos = raycast_hit.point - ray.direction;
				}
			}
			
			if(Input.GetMouseButton(0)){
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] raycast_hits;
				raycast_hits = Physics.RaycastAll(ray);
				bool picked_something_up = false;
				foreach(RaycastHit hit in raycast_hits){ 
					if(hit.collider.gameObject.layer != 10){
						continue;
					}
					DiceScript dice_script = hit.collider.gameObject.GetComponent<DiceScript>();
					if(dice_script.held_by_player_ == id_){
						continue;
					}
					picked_something_up = true;
					if(!Network.isServer){
						networkView.RPC("TellBoardAboutDiceClick",RPCMode.Server,dice_script.id_,id_);
					} else {
						TellBoardAboutDiceClick(dice_script.id_, id_);
					}
				}
			}
			if(Input.GetMouseButtonUp(0)){
				if(!Network.isServer){
					networkView.RPC("TellBoardAboutMouseRelease",RPCMode.Server,id_);
				} else {
					TellBoardAboutMouseRelease(id_);
				}
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
