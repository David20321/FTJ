using UnityEngine;
using System.Collections;

public class DiceScript : MonoBehaviour {
	public int id_;
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
		if(stream.isWriting){
			int id = id_;
			stream.Serialize(ref id);
		} else {
			int id = -1;
			stream.Serialize(ref id);
			id_ = id;
		}
	}
}
