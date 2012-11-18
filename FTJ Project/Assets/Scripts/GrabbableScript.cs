using UnityEngine;
using System.Collections;

public class GrabbableScript : MonoBehaviour {
	public int id_;
	public int held_by_player_ = -1;
	
	void Awake () {
		if(Network.isServer){
			held_by_player_ = -1;
		}
	}
	void Start () {
		if(Network.isServer){
			if(ObjectManagerScript.Instance()){
				ObjectManagerScript.Instance().RegisterGrabbableObject(gameObject);
			}
		}
	}
	
	void OnDestroy() {
		if(Network.isServer){
			if(ObjectManagerScript.Instance()){
				ObjectManagerScript.Instance().UnRegisterGrabbableObject(gameObject);
			}
		}
	}
	
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
