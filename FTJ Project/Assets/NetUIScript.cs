using UnityEngine;
using System.Collections;

public class NetUIScript : MonoBehaviour {
	// What screen is the player looking at
	enum State {NONE, MAIN_MENU, CREATE, CREATING, CREATE_FAIL, JOIN, MASTER_SERVER_FAIL, JOINING, JOIN_FAIL, JOIN_SUCCESS}
	State state_ = State.MAIN_MENU;
	const string DEFAULT_GAME_NAME = "Unnamed Game";
	const string DEFAULT_PLAYER_NAME = "Unknown Player";
	const string GAME_IDENTIFIER = "WolfireFTJGame";
	const int DEFAULT_PORT = 25000;
	const int MAX_PLAYERS = 4;
	
	string game_name_ = "???";
	string player_name_ = "???";
	string display_err_ = "???"; 
	
	void Start() {
	}
	
	void Update() {
	}
	
	void OnServerInitialized() {
		if(state_ == State.CREATING){
			SetState(State.NONE);
		}
	}
	
	void OnConnectedToServer() {
		if(state_ == State.JOINING){
			SetState(State.JOIN_SUCCESS);
		}
	}
	
	void OnFailedToConnect(NetworkConnectionError err) {
		if(state_ == State.JOINING){
			display_err_ = ""+err;
			SetState(State.JOIN_FAIL);
		}
	}
	
	void OnFailedToConnectToMasterServer(NetworkConnectionError err) {
		if(state_ == State.JOIN){
			display_err_ = ""+err;
			SetState(State.MASTER_SERVER_FAIL);
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
	}
	
	void OnGUI() {
		switch(state_){
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
			if(GUILayout.Button(server.gameName)){
				NetworkConnectionError err = Network.Connect(server);
				if(err == NetworkConnectionError.NoError){
					game_name_ = server.gameName;
					SetState(State.JOINING);
				} else {
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
		if(GUILayout.Button("Back")){
			SetState(State.JOIN);
		}
		GUILayout.EndHorizontal();
	}
	
	NetworkConnectionError CreateGame() {
		Network.InitializeSecurity();
		NetworkConnectionError err = Network.InitializeServer(MAX_PLAYERS,DEFAULT_PORT,true);
		if(err == NetworkConnectionError.NoError){
			MasterServer.RegisterHost(GAME_IDENTIFIER, game_name_, "Comments could go here");
		}
		return err;
	}
}
