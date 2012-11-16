using UnityEngine;
using System.Collections;

public class CardScript : MonoBehaviour {	
	[RPC]
	public void PrepareLocal(int card_id) {
		var card_back = transform.FindChild("Back").transform.FindChild("default");
		card_back.renderer.material = CardManagerScript.Instance().GetBackMaterial(card_id);
		var card_front = transform.FindChild("FrontBorder").transform.FindChild("default");
		card_front.renderer.material = CardManagerScript.Instance().GetFrontMaterial(card_id);
	}
	
	public void Prepare(int card_id) {
		if(Network.isServer && networkView){
			networkView.RPC("PrepareLocal",RPCMode.AllBuffered,card_id);
		} else {
			PrepareLocal(card_id);
		}
	}
}
