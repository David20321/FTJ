using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetEvent {
	public enum Type{SERVER_INITIALIZED};
	public Type type(){return type_;}
	public NetEvent(Type type){
		type_ = type;
	}
	
	Type type_;
}
	
public class NetEventScript : MonoBehaviour {
	Queue<NetEvent> event_queue = new Queue<NetEvent>();

	void OnServerInitialized() {
		event_queue.Enqueue(new NetEvent(NetEvent.Type.SERVER_INITIALIZED));
	}
	
	public static NetEventScript Instance() {
		GameObject go = GameObject.Find("GlobalScriptObject");
		Component component = go.GetComponent(typeof(NetEventScript));
		return ((NetEventScript)component);
    }
	
	public NetEvent GetEvent() {
		if(event_queue.Count==0){
			return null;
		}
		return event_queue.Dequeue();
	}
}
