using UnityEngine;
using System.Collections;

public class DiceScript : MonoBehaviour {
	public int id_;
	public int held_by_player_;
	public int owner_id_;
	public enum Type {DIE, TOKEN};
	public Type type_;
	
	void Start () {
		BoardScript.Instance().RegisterDiceObject(gameObject);
		if(Network.isServer){
			held_by_player_ = -1;
			owner_id_ = -1;
		}
	}
	
	void OnDestroy() {
		BoardScript.Instance().UnRegisterDiceObject(gameObject);
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
		if(stream.isWriting){
			int id = id_;
			stream.Serialize(ref id);
			int owner_id = owner_id_;
			stream.Serialize(ref owner_id);
		} else {
			int id = -1;
			stream.Serialize(ref id);
			id_ = id;
			int owner_id = -1;
			stream.Serialize(ref owner_id);
			owner_id_ = owner_id;
		}
	}
}
