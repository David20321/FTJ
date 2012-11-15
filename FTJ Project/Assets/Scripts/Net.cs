using UnityEngine;
using System.Collections;

public class Net {
	public static int GetMyID(){
		return int.Parse(Network.player.ToString());
	}
};
