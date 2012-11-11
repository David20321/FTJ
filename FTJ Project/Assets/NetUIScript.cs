using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetUIScript : MonoBehaviour {
	// What screen is the player looking at
	enum State {NONE, MAIN_MENU, CREATE, CREATING, CREATE_FAIL, 
				JOIN, MASTER_SERVER_FAIL, JOINING, JOINING_GAME_BY_NAME, JOIN_FAIL, JOIN_SUCCESS}
	State state_ = State.MAIN_MENU;
	const string DEFAULT_GAME_NAME = "Unnamed Game";
	const string DEFAULT_PLAYER_NAME = "Unknown Player";
	const string GAME_IDENTIFIER = "WolfireFTJGame";
	const int DEFAULT_PORT = 25000;
	const int MAX_PLAYERS = 4;
	const int MAX_CONNECTIONS = MAX_PLAYERS-1;
	
	Dictionary<int, string> player_names_ = new Dictionary<int,string>();
	
	string queued_join_game_name_ = "";
	string game_name_ = "???";
	string player_name_ = "???";
	string display_err_ = "???"; 
	
	void Start() {
		RequestPageURLForAutoJoin();
	}
	
	void NetEventServerInitialized(){
		if(state_ == State.CREATING){
			SetState(State.NONE);
		}
		ConsoleScript.Log("Server initialized");
		int player_id = int.Parse(Network.player.ToString());
		ConsoleScript.Log("Telling server that player "+player_id+" is named: "+player_name_);
		TellServerPlayerName(player_name_);
	}
	
	void NetEventConnectedToServer(){
		if(state_ == State.JOINING){
			SetState(State.JOIN_SUCCESS);
		}
		ConsoleScript.Log("Connected to server with ID: "+Network.player);
		TellServerPlayerName(player_name_);
	}
	
	void NetEventFailedToConnectToMasterServer(NetEvent net_event) {
		if(state_ == State.JOIN){
			display_err_ = ""+net_event.error();
			SetState(State.MASTER_SERVER_FAIL);
		}
		ConsoleScript.Log("Failed to connect to master server: "+net_event.error());
	}
	
	void NetEventFailedToConnect(NetEvent net_event){		
		if(state_ == State.JOINING){
			display_err_ = ""+net_event.error();
			SetState(State.JOIN_FAIL);
		}
		ConsoleScript.Log("Failed to connect: "+net_event.error());
	}
	
	void ConnectToServer(HostData server){
		game_name_ = server.gameName;
		SetState(State.JOINING);
		NetworkConnectionError err = Network.Connect(server);
		if(err != NetworkConnectionError.NoError){
			display_err_ = ""+err;
			SetState(State.JOIN_FAIL);
		}
	}
	
	void JoinHostListGameByName(string val){
		HostData[] servers = MasterServer.PollHostList();
		foreach(HostData server in servers){
			if(val == server.gameName){
				ConnectToServer(server);
				return;
			}
		}
		display_err_ = "No game named \""+val+"\" in host list";
		SetState(State.JOIN_FAIL);
	}
	
	void NetEventMasterServerEvent(NetEvent net_event){
		switch(net_event.master_server_event()){
			case MasterServerEvent.HostListReceived:
        		ConsoleScript.Log("Received a host list from the master server.");
        		if(queued_join_game_name_.Length > 0){
        			JoinHostListGameByName(queued_join_game_name_);
        			queued_join_game_name_ = "";
        		}
        		break;
			case MasterServerEvent.RegistrationFailedGameName:
        		ConsoleScript.Log("Registration failed because an empty game name was given.");
        		break;
			case MasterServerEvent.RegistrationFailedGameType:
        		ConsoleScript.Log("Registration failed because an empty game type was given.");
        		break;
			case MasterServerEvent.RegistrationFailedNoServer:
        		ConsoleScript.Log("Registration failed because no server is running.");
        		break;
			case MasterServerEvent.RegistrationSucceeded:
        		ConsoleScript.Log("Registration to master server succeeded, received confirmation.");
        		break;
		}
	}
	
	void NetEventPlayerDisconnected(NetEvent net_event) {
		NetworkPlayer player = net_event.network_player();
		ConsoleScript.Log("Player "+player+" disconnected");
		player_names_.Remove(int.Parse(player.ToString()));
		UpdatePlayerList();
		Network.RemoveRPCs(player);
    	Network.DestroyPlayerObjects(player);
	}
	
	void NetEventDisconnectedFromServer(NetEvent net_event) {
		switch(net_event.network_disconnection()){
			case NetworkDisconnection.Disconnected:
				ConsoleScript.Log("Cleanly disconnected from server");
				break;
			case NetworkDisconnection.LostConnection:
				ConsoleScript.Log("Connection to server was lost unexpectedly");
				break;
		}
		if(state_ == State.NONE || state_ == State.JOIN_SUCCESS || state_ == State.JOINING){
			SetState(State.MAIN_MENU);
		}
	}
	
	void Update() {
		NetEvent net_event = NetEventScript.Instance().GetEvent();
		while(net_event != null){
			switch(net_event.type()){
				case NetEvent.Type.SERVER_INITIALIZED:
					NetEventServerInitialized();
					break;
				case NetEvent.Type.CONNECTED_TO_SERVER:
					NetEventConnectedToServer();
					break;
				case NetEvent.Type.FAILED_TO_CONNECT:
					NetEventFailedToConnect(net_event);
					break;
				case NetEvent.Type.FAILED_TO_CONNECT_TO_MASTER_SERVER:
					NetEventFailedToConnectToMasterServer(net_event);
					break;
				case NetEvent.Type.MASTER_SERVER_EVENT:
					NetEventMasterServerEvent(net_event);
					break;
				case NetEvent.Type.PLAYER_CONNECTED:
					ConsoleScript.Log("Player "+net_event.network_player()+" connected");
					break;
				case NetEvent.Type.PLAYER_DISCONNECTED:
					NetEventPlayerDisconnected(net_event);
					break;
				case NetEvent.Type.DISCONNECTED_FROM_SERVER:
					NetEventDisconnectedFromServer(net_event);
					break;
			}
			net_event = NetEventScript.Instance().GetEvent();
		}
	}
	
	void UpdatePlayerList() {
		List<string> player_names = new List<string>();		
		foreach (var pair in player_names_){
			player_names.Add(pair.Key+": "+pair.Value);
		}
		PlayerListScript.SetPlayerNames(player_names);
	}
	
	[RPC]
	void SetPlayerName(int id, string name){
		player_names_[id] = name;
		ConsoleScript.Log("Player "+id+" is named: "+name);
		UpdatePlayerList();
	}
	
	Dictionary<string,string> ParseURLQuery(string val){
		Dictionary<string,string> element_dictionary = new Dictionary<string, string>();
		string[] question_mark = val.Split('?');
		if(question_mark.Length > 1){
			string query = question_mark[1];
			string[] elements = query.Split('&');
			ConsoleScript.Log("Query parts:");
			foreach(string element in elements){
				string[] parts = element.Split('=');
				if(parts.Length > 1){
					ConsoleScript.Log(parts[0] + ": " + parts[1]);
					element_dictionary[parts[0]] = parts[1];
				}
			}
		}
		return element_dictionary;
	}
	
	// Chain of parallel functions for CopyGameJoin
	void RequestPageURLForCopyGameJoin(){
		ConsoleScript.Log("Requesting page url");
		Application.ExternalEval("GetUnity().SendMessage(\"GlobalScriptObject\", \"ReceivePageURLForCopyGameJoin\", decodeURIComponent(document.location.href));");
	}
	void ReceivePageURLForCopyGameJoin(string val){
		ConsoleScript.Log("Received page url");
		string join_url = val.Split('?')[0] + "?join="+game_name_;
		CopyTextToClipboard(join_url);
	}
	void CopyTextToClipboard(string str){	
		TextEditor te = new TextEditor();
		te.content = new GUIContent(str);
		te.SelectAll();
		te.Copy();
	}
	
	// Chain of parallel functions for AutoJoin
	void RequestPageURLForAutoJoin(){
		ConsoleScript.Log("Requesting page url");
		Application.ExternalEval("GetUnity().SendMessage(\"GlobalScriptObject\", \"ReceivePageURLForAutoJoin\", decodeURIComponent(document.location.href));");
	}
	void ReceivePageURLForAutoJoin(string val){
		ConsoleScript.Log("Received page url");
		Dictionary<string,string> elements = ParseURLQuery(val);
		if(elements.ContainsKey("join")){
			JoinGameByName(elements["join"]);
		}
	}
	void JoinGameByName(string val){
		ConsoleScript.Log("Attempting to join game: "+val);
		MasterServer.RequestHostList(GAME_IDENTIFIER);
		SetState(State.JOINING);
		queued_join_game_name_ = val;
		game_name_ = val;
	}
	
	void TellServerPlayerName(string name){		
		int player_id = int.Parse(Network.player.ToString());
		ConsoleScript.Log("Telling server that player "+player_id+" is named: "+player_name_);
		if(Network.isClient){
			networkView.RPC("SetPlayerName", RPCMode.Server, player_id, name);
		} else {
			SetPlayerName(player_id, name);	
		}
	}
    
	
	void SetState(State state) {
		switch(state){
			case State.JOIN:
				MasterServer.RequestHostList(GAME_IDENTIFIER);
				break;
			case State.JOIN_SUCCESS:
				player_name_ = DEFAULT_PLAYER_NAME;
				break;
			case State.CREATE:
				game_name_ = DEFAULT_GAME_NAME;
				player_name_ = DEFAULT_PLAYER_NAME;
				break;
		}
		state_ = state;
		ConsoleScript.Log("Set state: "+state);
	}
	
	void OnGUI() {
		switch(state_){
			case State.NONE:
				DrawGameGUI();
				break;
			case State.MAIN_MENU:
				DrawMainMenuGUI();
				break;
			case State.CREATE:
				DrawCreateGUI();
				break;
			case State.CREATING:
				DrawCreatingGUI();
				break;
			case State.CREATE_FAIL:
				DrawCreateFailGUI();
				break;
			case State.JOIN:
				DrawJoinGUI();
				break;
			case State.JOINING:
				DrawJoiningGUI();
				break;
			case State.JOIN_FAIL:
				DrawJoinFailGUI();
				break;
			case State.JOIN_SUCCESS:
				DrawJoinSuccessGUI();
				break;
			case State.MASTER_SERVER_FAIL:
				DrawMasterServerFailGUI();
				break;
		}
	}
	
	void DrawGameGUI() {
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Copy Join URL To Clipboard")){
			RequestPageURLForCopyGameJoin();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label(game_name_);
		GUILayout.EndHorizontal();
		List<string> player_names = PlayerListScript.GetPlayerNames();
		foreach (string name in player_names){
			GUILayout.BeginHorizontal();
			GUILayout.Label(name);
			GUILayout.EndHorizontal();
		}
	}
	
	void DrawMainMenuGUI() {
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Create Game")){
			SetState(State.CREATE);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Join Game")){
			SetState(State.JOIN);
		}
		GUILayout.EndHorizontal();
	}
	
	void DrawCreateGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Game name: ");
		game_name_ = GUILayout.TextField(game_name_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Player name: ");
		player_name_ = GUILayout.TextField(player_name_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Create")){
			SetState(State.CREATING);
			NetworkConnectionError err = CreateGame();
			if(err != NetworkConnectionError.NoError){
				display_err_ = ""+err;
				SetState(State.CREATE_FAIL);
			}
		}
		if(GUILayout.Button("Back")){
			SetState(State.MAIN_MENU);
		}
		GUILayout.EndHorizontal();
	}
	
	void DrawCreatingGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Attempting to create game: "+game_name_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Cancel")){
			Network.Disconnect();
			SetState(State.CREATE);
		}
		GUILayout.EndHorizontal();
	}
	
	void DrawCreateFailGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Game creation failed.");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Error: "+display_err_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Back")){
			SetState(State.CREATE);
		}
		GUILayout.EndHorizontal();
	}
	
	void DrawJoinGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Available servers:");
		GUILayout.EndHorizontal();
		HostData[] servers = MasterServer.PollHostList();
		foreach(HostData server in servers){
			GUILayout.BeginHorizontal();
			string display_name = server.gameName + " " + server.connectedPlayers + "/" + server.playerLimit;
			if(GUILayout.Button(display_name)){
				game_name_ = server.gameName;
				SetState(State.JOINING);
				NetworkConnectionError err = Network.Connect(server);
				if(err != NetworkConnectionError.NoError){
					display_err_ = ""+err;
					SetState(State.JOIN_FAIL);
				}
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Back")){
			SetState(State.MAIN_MENU);
		}
		GUILayout.EndHorizontal();
	}
	
	void DrawJoiningGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Joining game: "+game_name_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Cancel")){
			Network.Disconnect();
			SetState(State.JOIN);
		}
		GUILayout.EndHorizontal();
	}
	
	void DrawJoinFailGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Failed to join game: "+game_name_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Error: "+display_err_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Back")){
			SetState(State.JOIN);
		}
		GUILayout.EndHorizontal();
	}

	
	void DrawMasterServerFailGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Failed to connect to master server.");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Error: "+display_err_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Retry")){
			SetState(State.JOIN);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Back")){
			SetState(State.MAIN_MENU);
		}
		GUILayout.EndHorizontal();
	}
	
	void DrawJoinSuccessGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("Successfully joined game: "+game_name_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Player name: ");
		player_name_ = GUILayout.TextField(player_name_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Continue")){
			TellServerPlayerName(player_name_);
			SetState(State.NONE);
		}
		GUILayout.EndHorizontal();
	}
	
	NetworkConnectionError CreateGame() {
		Network.InitializeSecurity();
		NetworkConnectionError err = Network.InitializeServer(MAX_CONNECTIONS,DEFAULT_PORT,true);
		if(err == NetworkConnectionError.NoError){
			MasterServer.RegisterHost(GAME_IDENTIFIER, game_name_, "Comments could go here");
		}
		return err;
	}
}
