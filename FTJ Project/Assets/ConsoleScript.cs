using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConsoleScript : MonoBehaviour {
	const int MAX_MESSAGES = 8;
	List<string> messages = new List<string>();
	
	void OnGUI() {
		GUILayout.BeginArea(new Rect(0,Screen.height-200,300,200));
		for(int i=0; i<messages.Count; ++i){
			GUILayout.BeginHorizontal();
			GUILayout.Label(messages[i]);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();
	}
	
	public void AddMessage(string msg){
		messages.Add(msg);
		if(messages.Count>MAX_MESSAGES){
			messages.RemoveAt(0);
		}
	}
}
