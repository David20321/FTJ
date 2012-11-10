using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerListScript : MonoBehaviour {
	List<string> names_ = new List<string>();

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        if (stream.isWriting) {
        	ConsoleScript.Log("Writing player list");
            int num_players = names_.Count;
            List<string> names = names_;
            stream.Serialize(ref num_players);
            foreach(string name in names){
            	int length = name.Length;
            	stream.Serialize(ref length);
            	foreach(char character in name){
            		char ref_char = character;
            		stream.Serialize(ref ref_char);
            	}
        	}
        } else {
        	ConsoleScript.Log("Reading player list");
            int num_players = -1;
            stream.Serialize(ref num_players);
            List<string> names = new List<string>();
            for(int j=0; j<num_players; ++j){
            	string name = "";
            	int length = -1;
            	stream.Serialize(ref length);
            	for(int i=0; i<length; ++i){
            		char character = '\0';
            		stream.Serialize(ref character);
            		name += character;
            	}
            	names.Add(name);
        	}
            names_ = names;
        }
    }
    
    void SetPlayersLocal(List<string> player_names) {
    	names_ = player_names;
    }
    
    List<string> GetPlayerNamesLocal() {
    	return names_;
    }
    
    public static PlayerListScript Instance() {
		GameObject go = GameObject.Find("PlayerListObject");
		Component component = go.GetComponent(typeof(PlayerListScript));
		return ((PlayerListScript)component);
    }
    
    public static void SetPlayerNames(List<string> player_names) {
		Instance().SetPlayersLocal(player_names);
    }
    
    public static List<string> GetPlayerNames() {
		return Instance().GetPlayerNamesLocal();
    }
}
