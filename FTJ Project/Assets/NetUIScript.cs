using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetUIScript : MonoBehaviour {
	// What screen is the player looking at
	enum State {NONE, MAIN_MENU, CREATE, CREATING, CREATE_FAIL, JOIN, MASTER_SERVER_FAIL, JOINING, JOIN_FAIL, JOIN_SUCCESS}
	State state_ = State.MAIN_MENU;
	const string DEFAULT_GAME_NAME = "Unnamed Game";
	const string DEFAULT_PLAYER_NAME = "Unknown Player";
	const string GAME_IDENTIFIER = "WolfireFTJGame";
	const int DEFAULT_PORT = 25000;
	const int MAX_PLAYERS = 4;
	const int MAX_CONNECTIONS = MAX_PLAYERS-1;
	
	Dictionary<int, string> player_names_ = new Dictionary<int,string>();
	
	string game_name_ = "???";
	string player_name_ = "???";
	string display_err_ = "???"; 
	
	void Start() {
	}
	
	void Update() {
	}
	
	[RPC]
	void SetPlayerName(int id, string name){
		player_names_[id] = name;
		ConsoleScript.Log("Player "+id+" is named: "+name);
	}
	
	void OnServerInitialized() {
		if(state_ == State.CREATING){
			SetState(State.NONE);
		}
		ConsoleScript.Log("Server initialized");
		int player_id = int.Parse(Network.player.ToString());
		ConsoleScript.Log("Telling server that player "+player_id+" is named: "+player_name_);
		SetPlayerName(player_id, player_name_);
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
	
	void OnConnectedToServer() {
		if(state_ == State.JOINING){
			SetState(State.JOIN_SUCCESS);
		}
		ConsoleScript.Log("Connected to server with ID: "+Network.player);
		TellServerPlayerName(player_name_);
	}
	
	void OnFailedToConnect(NetworkConnectionError err) {
		if(state_ == State.JOINING){
			display_err_ = ""+err;
			SetState(State.JOIN_FAIL);
		}
		ConsoleScript.Log("Failed to connect: "+err);
	}
	
	void OnFailedToConnectToMasterServer(NetworkConnectionError err) {
		if(state_ == State.JOIN){
			display_err_ = ""+err;
			SetState(State.MASTER_SERVER_FAIL);
		}
		ConsoleScript.Log("Failed to connect to master server: "+err);
	}
	
	void OnMasterServerEvent(MasterServerEvent the_event) {
		switch(the_event){
			case MasterServerEvent.HostListReceived:
        		ConsoleScript.Log("Received a host list from the master server.");
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
    
	void OnPlayerConnected(NetworkPlayer player) {
		ConsoleScript.Log("Player "+player+" connected");
	}
	
	void OnPlayerDisconnected(NetworkPlayer player) {
		ConsoleScript.Log("Player "+player+" disconnected");
		player_names_.Remove(int.Parse(player.ToString()));
		Network.RemoveRPCs(player);
    	Network.DestroyPlayerObjects(player);
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		switch(info){
			case NetworkDisconnection.Disconnected:
				ConsoleScript.Log("Cleanly disconnected from server");
				break;
			case NetworkDisconnection.LostConnection:
				ConsoleScript.Log("Connection to server was lost unexpectedly");
				break;
		}
		if(state_ == State.NONE || state_ == State.JOIN_SUCCESS || state_ == State.JOINING){
			state_ = State.MAIN_MENU;
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
		GUILayout.Label(game_name_);
		GUILayout.EndHorizontal();
		foreach (var pair in player_names_){
			GUILayout.BeginHorizontal();
			GUILayout.Label(pair.Key + ": " + pair.Value);
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
