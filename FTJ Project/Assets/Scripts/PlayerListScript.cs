using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInfo {
	public string name_;
	public Color color_;
}

public class PlayerListScript : MonoBehaviour {
	Dictionary<int, PlayerInfo> player_info_list_ = new Dictionary<int,PlayerInfo>();
	
	public void Remove(int which){
		player_info_list_.Remove(which);
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        if (stream.isWriting) {
        	//ConsoleScript.Log("Writing player list");
            Dictionary<int, PlayerInfo> player_info_list = player_info_list_;
            int num_players = player_info_list.Count;
            stream.Serialize(ref num_players);
            foreach(var pair in player_info_list){
            	int id = pair.Key;
            	stream.Serialize(ref id);
            	PlayerInfo player_info = pair.Value;
            	string name = player_info.name_;
            	int length = name.Length;
            	stream.Serialize(ref length);
            	foreach(char character in name){
            		char ref_char = character;
            		stream.Serialize(ref ref_char);
            	}
            	Vector3 color = new Vector3(player_info.color_.r,player_info.color_.g,player_info.color_.b);
        		stream.Serialize(ref color);
        	}
        } else {
        	//ConsoleScript.Log("Reading player list");
            int num_players = -1;
            stream.Serialize(ref num_players);
            Dictionary<int, PlayerInfo> player_info_list = new Dictionary<int, PlayerInfo>();
            for(int j=0; j<num_players; ++j){
            	int id = -1;
            	stream.Serialize(ref id);
            	PlayerInfo player_info = new PlayerInfo();
            	player_info.name_ = "";
            	int length = -1;
            	stream.Serialize(ref length);
            	for(int i=0; i<length; ++i){
            		char character = '\0';
            		stream.Serialize(ref character);
            		player_info.name_ += character;
            	}
            	Vector3 color = new Vector3(0,0,0);
        		stream.Serialize(ref color);
        		player_info.color_ = new Color(color.x, color.y, color.z);
            	player_info_list.Add(id, player_info);
        	}
            player_info_list_ = player_info_list;
        }
    }
    
    public Dictionary<int, PlayerInfo> GetPlayerInfoList() {
    	return player_info_list_;
    }
    
    [RPC]
    public void SetPlayerName(int id, string name){
		if(!player_info_list_.ContainsKey(id)){
			player_info_list_.Add(id, new PlayerInfo());
			player_info_list_[id].color_ = new Color(
				Random.Range(0.0f,1.0f),
				Random.Range(0.0f,1.0f),
				Random.Range(0.0f,1.0f));
		}
		player_info_list_[id].name_ = name;
    }
    
    public static PlayerListScript Instance() {
		GameObject go = GameObject.Find("GlobalScriptObject");
		Component component = go.GetComponent(typeof(PlayerListScript));
		return ((PlayerListScript)component);
    }
}
