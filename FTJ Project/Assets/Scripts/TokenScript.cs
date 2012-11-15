using UnityEngine;
using System.Collections;

public class TokenScript : MonoBehaviour {
	public int owner_id_;
	
	void Start () {
		if(Network.isServer){
			owner_id_ = -1;
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
		if(stream.isWriting){
			int owner_id = owner_id_;
			stream.Serialize(ref owner_id);
		} else {
			int owner_id = -1;
			stream.Serialize(ref owner_id);
			owner_id_ = owner_id;
		}
	}
}
