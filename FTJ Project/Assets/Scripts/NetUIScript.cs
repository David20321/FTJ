using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetUIScript : MonoBehaviour {
	// What screen is the player looking at
	enum State {NONE, MAIN_MENU, CREATE, CREATING, CREATE_FAIL, 
				JOIN, MASTER_SERVER_FAIL, JOINING, JOINING_GAME_BY_NAME, JOIN_FAIL, JOIN_PASSWORD, JOIN_SUCCESS}
	State state_ = State.MAIN_MENU;
	const string DEFAULT_GAME_NAME = "Unnamed Game";
	const string DEFAULT_PLAYER_NAME = "Unknown Player";
	const string GAME_IDENTIFIER = "WolfireFTJGamev36";
	const int DEFAULT_PORT = 25000;
	const int MAX_PLAYERS = 4;
	const int MAX_CONNECTIONS = MAX_PLAYERS-1;
	const int MIN_TEXT_FIELD_WIDTH = 200;
	HostData last_tried_server_ = null;
	bool help_shown_ = false;
	public GameObject cursor_prefab;
	public GameObject board_prefab;
	public GameObject play_area_prefab;
	public GameObject token_prefab;
	public GUISkin help_gui_skin;
	int first_state_ui_update = 0;
	List<GameObject> play_areas = new List<GameObject>();
	
	string queued_join_game_name_ = "";
	string game_name_ = "???";
	string player_name_ = "???";
	string display_err_ = "???"; 
	string password_ = "";
	
	void Start() {
		RequestPageURLForAutoJoin();
		//TryToCreateGame(true);
	}
	
	void SpawnHealthTokens() {
		foreach(GameObject play_area in play_areas){
			Transform token_spawns = play_area.transform.Find("TokenSpawns");
			foreach(Transform token_spawn in play_area.transform.FindChild("token_spawns")){
				GameObject token_object = (GameObject)Network.Instantiate(token_prefab, token_spawn.position, Quaternion.identity, 0);
			}
		}
	}
	
	void NetEventServerInitialized(){
		if(state_ == State.CREATING){
			SetState(State.NONE);
		}
		ConsoleScript.Log("Server initialized");
		int player_id = int.Parse(Network.player.ToString());
		TellServerPlayerName(player_name_);
		Network.Instantiate(board_prefab, GameObject.Find("board_spawn").transform.position, GameObject.Find("board_spawn").transform.rotation,0);
		int count = 0;
		foreach(Transform player_spawn in GameObject.Find("play_areas").transform){
			GameObject play_area_obj = (GameObject)Network.Instantiate(play_area_prefab, player_spawn.transform.position, player_spawn.transform.rotation,0);
			play_area_obj.GetComponent<PlayAreaScript>().SetColor(count);
			play_areas.Add (play_area_obj);
			++count;
		}
		SpawnHealthTokens();
		
		Network.Instantiate(cursor_prefab, new Vector3(0,0,0), Quaternion.identity, 0);
	}
	
	void NetEventConnectedToServer(){
		if(state_ == State.JOINING){
			SetState(State.JOIN_SUCCESS);
		}
		ConsoleScript.Log("Connected to server with ID: "+Network.player);
		TellServerPlayerName(player_name_);
		Network.Instantiate(cursor_prefab, new Vector3(0,0,0), Quaternion.identity, 0);
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
			if(net_event.error() == NetworkConnectionError.InvalidPassword){
				SetState(State.JOIN_PASSWORD);
			} else {
				display_err_ = ""+net_event.error();
				SetState(State.JOIN_FAIL);
			}
		}
		ConsoleScript.Log("Failed to connect: "+net_event.error());
	}
	
	void ConnectToServer(HostData server, string password){
		last_tried_server_ = server;
		game_name_ = server.gameName;
		SetState(State.JOINING);
		NetworkConnectionError err = Network.Connect(server, password);
		if(err != NetworkConnectionError.NoError){
			display_err_ = ""+err;
			SetState(State.JOIN_FAIL);
		}
	}
	
	void JoinHostListGameByName(string val){
		HostData[] servers = MasterServer.PollHostList();
		foreach(HostData server in servers){
			if(val == server.gameName){
				ConnectToServer(server, "");
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
		PlayerListScript.Instance().Remove(int.Parse(player.ToString()));
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
			Application.LoadLevel(Application.loadedLevel);
		}
	}
	
	void Update() {
		if(state_ == State.NONE){
			if(Input.GetKeyDown("/")){
				help_shown_ = !help_shown_;
			}
		}
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
	
	Dictionary<string,string> ParseURLQuery(string val){
		Dictionary<string,string> element_dictionary = new Dictionary<string, string>();
		string[] question_mark = val.Split('?');
		if(question_mark.Length > 1){
			string query = question_mark[1];
			for(int i=2; i<question_mark.Length; ++i){
				query += '?' + question_mark[i];
			}
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
			PlayerListScript.Instance().SetPlayerName(player_id, name);	
		}
	}
    
	void SetState(State state) {
		first_state_ui_update = 0;
		switch(state){
			case State.JOIN:
				MasterServer.RequestHostList(GAME_IDENTIFIER);
				password_ = "";
				break;
			case State.JOIN_SUCCESS:
				player_name_ = DEFAULT_PLAYER_NAME;
				break;
			case State.CREATE:
				game_name_ = DEFAULT_GAME_NAME;
				player_name_ = DEFAULT_PLAYER_NAME;
				password_ = "";
				break;
			case State.NONE:
				help_shown_ = false;
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
			case State.JOIN_PASSWORD:
				DrawJoinPasswordGUI();
				break;
			case State.JOIN_SUCCESS:
				DrawJoinSuccessGUI();
				break;
			case State.MASTER_SERVER_FAIL:
				DrawMasterServerFailGUI();
				break;
		}
		++first_state_ui_update;
	}
	
	void DrawGameGUI() {
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Exit Game")){
			Network.Disconnect();
			Application.LoadLevel(Application.loadedLevel);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Copy Join URL To Clipboard")){
			RequestPageURLForCopyGameJoin();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Restart Game")){
			ObjectManagerScript.Instance().RecoverDice();
			SpawnHealthTokens();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label(game_name_);
		GUILayout.EndHorizontal();
		Dictionary<int, PlayerInfo> player_info_list = PlayerListScript.Instance().GetPlayerInfoList();
		foreach (var pair in player_info_list){
			GUI.contentColor = pair.Value.color_;
			GUILayout.BeginHorizontal();
			GUILayout.Label(pair.Key + ": " + pair.Value.name_);
			GUILayout.EndHorizontal();
			GUI.contentColor = Color.white;
		}
		GUILayout.BeginArea(new Rect(Screen.width - 200, 0, 200, 200));
		if(!help_shown_){
			GUILayout.BeginHorizontal();
			GUILayout.Label("Press '?' for help", help_gui_skin.label);
			GUILayout.EndHorizontal();
		} else {
			GUILayout.BeginHorizontal();
			GUILayout.Label("Press '?' to close help", help_gui_skin.label);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Press 'Z' to zoom in", help_gui_skin.label);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Press 'WASD' to move while zoomed", help_gui_skin.label);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Press 'E' or 'R' to rotate cards", help_gui_skin.label);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Press 'F' to flip cards", help_gui_skin.label);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Press 'T' to tap tokens", help_gui_skin.label);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();
		
	}
	
	void TryToCreateGame(bool local){
		SetState(State.CREATING);
		NetworkConnectionError err = CreateGame(local);
		if(err != NetworkConnectionError.NoError){
			display_err_ = ""+err;
			SetState(State.CREATE_FAIL);
		}
	}
	
	void DrawMainMenuGUI() {
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Local Game")){
			TryToCreateGame(true);
		}
		GUILayout.EndHorizontal();
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
		GUI.SetNextControlName("GameNameField");
		game_name_ = GUILayout.TextField(game_name_, GUILayout.MinWidth(MIN_TEXT_FIELD_WIDTH));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Game password: ");
		password_ = GUILayout.TextField(password_, GUILayout.MinWidth(MIN_TEXT_FIELD_WIDTH));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Player name: ");
		GUI.SetNextControlName("PlayerNameField");
		player_name_ = GUILayout.TextField(player_name_, GUILayout.MinWidth(MIN_TEXT_FIELD_WIDTH));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Create")){
			TryToCreateGame(false);
		}
		if(GUILayout.Button("Back")){
			SetState(State.MAIN_MENU);
		}
		GUILayout.EndHorizontal();
		if(first_state_ui_update == 1){
			GUI.FocusControl("GameNameField");
			GUI.FocusControl("PlayerNameField");
			GUI.FocusControl("GameNameField");
		}
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
				ConnectToServer(server, "");
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

	void DrawJoinPasswordGUI() {
		GUILayout.BeginHorizontal();
		GUI.SetNextControlName("RandomLabel");
		GUILayout.Label(game_name_ + " requires a password:");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUI.SetNextControlName("PasswordField");
		password_ = GUILayout.TextField(password_, GUILayout.MinWidth(MIN_TEXT_FIELD_WIDTH));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Try Again")){
			ConnectToServer(last_tried_server_, password_);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Back")){
			SetState(State.JOIN);
		}
		GUILayout.EndHorizontal();
		if(first_state_ui_update == 1){
			GUI.FocusControl("RandomLabel");
			GUI.FocusControl("PasswordField");
		}
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
		GUI.SetNextControlName("ALabel");
		GUILayout.Label("Successfully joined game: "+game_name_);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUI.SetNextControlName("NameField");
		GUILayout.Label("Player name: ");
		player_name_ = GUILayout.TextField(player_name_, GUILayout.MinWidth(MIN_TEXT_FIELD_WIDTH));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Continue")){
			TellServerPlayerName(player_name_);
			SetState(State.NONE);
		}
		GUILayout.EndHorizontal();
		if(first_state_ui_update == 1){
			GUI.FocusControl("ALabel");
			GUI.FocusControl("NameField");
		}
	}
	
	NetworkConnectionError CreateGame(bool local) {
		NetworkConnectionError err;
		if(local){			
			err = Network.InitializeServer(1,DEFAULT_PORT,false);
		} else {
			Network.InitializeSecurity();
			Network.incomingPassword = password_;
			err = Network.InitializeServer(MAX_CONNECTIONS,DEFAULT_PORT,true);
			if(err == NetworkConnectionError.NoError){
				MasterServer.RegisterHost(GAME_IDENTIFIER, game_name_, "Comments could go here");
			}
		}			
		return err;
	}
	
	public static NetUIScript Instance() {
		return GameObject.Find("GlobalScriptObject").GetComponent<NetUIScript>();
	}
}
