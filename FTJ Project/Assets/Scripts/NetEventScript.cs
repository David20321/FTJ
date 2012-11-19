using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetEvent {
	public enum Type{SERVER_INITIALIZED,
					 CONNECTED_TO_SERVER,
					 FAILED_TO_CONNECT,
					 FAILED_TO_CONNECT_TO_MASTER_SERVER,
					 MASTER_SERVER_EVENT,
					 PLAYER_DISCONNECTED,
					 PLAYER_CONNECTED,
					 DISCONNECTED_FROM_SERVER};
	public Type type(){return type_;}
	virtual public NetworkConnectionError error(){
		throw new System.InvalidOperationException("error() should only be called on NetErrorEvent");
		//return NetworkConnectionError.NoError;
	}
	virtual public MasterServerEvent master_server_event(){
		throw new System.InvalidOperationException("master_server_event() should only be called on NetMasterServerEvent");
		//return MasterServerEvent.RegistrationSucceeded;
	}
	virtual public NetworkPlayer network_player(){
		throw new System.InvalidOperationException("network_player() should only be called on NetPlayerEvent");
		//return new NetworkPlayer();
	}
	virtual public NetworkDisconnection network_disconnection(){
		throw new System.InvalidOperationException("network_disconnection() should only be called on NetDisconnectionEvent");
		//return NetworkDisconnection.Disconnected;
	}
	public NetEvent(Type type){
		type_ = type;
	}
	
	private Type type_;
}
	
public class NetErrorEvent : NetEvent {
	override public NetworkConnectionError error(){return error_;}
	public NetErrorEvent(Type type, NetworkConnectionError err):base(type){
		error_ = err;
	}
	NetworkConnectionError error_;
}
	
public class NetMasterServerEvent : NetEvent {
	override public MasterServerEvent master_server_event(){return master_server_event_;}
	public NetMasterServerEvent(Type type, MasterServerEvent master_server_event):base(type){
		master_server_event_ = master_server_event;
	}	
	private MasterServerEvent master_server_event_;
}

public class NetPlayerEvent : NetEvent {
	override public NetworkPlayer network_player(){return network_player_;}
	public NetPlayerEvent(Type type, NetworkPlayer network_player):base(type){
		network_player_ = network_player;
	}	
	private NetworkPlayer network_player_;
}
	
public class NetDisconnectionEvent : NetEvent {
	override public NetworkDisconnection network_disconnection(){return network_disconnection_;}
	public NetDisconnectionEvent(Type type, NetworkDisconnection network_disconnection):base(type){
		network_disconnection_ = network_disconnection;
	}	
	private NetworkDisconnection network_disconnection_;
}
	
public class NetEventScript : MonoBehaviour {
	Queue<NetEvent> event_queue = new Queue<NetEvent>();

	void OnServerInitialized() {
		event_queue.Enqueue(new NetEvent(NetEvent.Type.SERVER_INITIALIZED));
	}
	
	void OnConnectedToServer() {
		event_queue.Enqueue(new NetEvent(NetEvent.Type.CONNECTED_TO_SERVER));
	}
	
	void OnFailedToConnect(NetworkConnectionError err) {
		event_queue.Enqueue(new NetErrorEvent(NetEvent.Type.FAILED_TO_CONNECT, err));
	}
	
	void OnFailedToConnectToMasterServer(NetworkConnectionError err) {
		event_queue.Enqueue(new NetErrorEvent(NetEvent.Type.FAILED_TO_CONNECT_TO_MASTER_SERVER, err));
	}
	
	void OnMasterServerEvent(MasterServerEvent the_event) {
		event_queue.Enqueue(new NetMasterServerEvent(NetEvent.Type.MASTER_SERVER_EVENT, the_event));
	}
	
	void OnPlayerConnected(NetworkPlayer player) {
		event_queue.Enqueue(new NetPlayerEvent(NetEvent.Type.PLAYER_CONNECTED, player));
	}
	
	void OnPlayerDisconnected(NetworkPlayer player) {
		event_queue.Enqueue(new NetPlayerEvent(NetEvent.Type.PLAYER_DISCONNECTED, player));
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		event_queue.Enqueue(new NetDisconnectionEvent(NetEvent.Type.DISCONNECTED_FROM_SERVER, info));
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
