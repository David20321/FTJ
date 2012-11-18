using UnityEngine;
using System.Collections;

public class DiceScript : MonoBehaviour {
	public AudioClip[] dice_impact_wood;
	public AudioClip[] dice_impact_board;
	public AudioClip[] dice_impact_dice;
	public AudioClip[] dice_pick_up;
	float last_sound_time = 0.0f;
	const float PHYSICS_SOUND_DELAY = 0.1f;
	const float DICE_GLOBAL_SOUND_MULT = 0.3f;
	const float DICE_BOARD_SOUND_MULT = 0.4f;
	const float DICE_WOOD_SOUND_MULT = 1.0f;
	const float DICE_DICE_SOUND_MULT = 1.0f;
	
	void PlayRandomSound(AudioClip[] clips, float volume){
		audio.PlayOneShot(clips[Random.Range(0,clips.Length)], volume);
	}		
	
	[RPC]
	public void ShakeSound(){
		if(Network.isServer){
			networkView.RPC("ShakeSound",RPCMode.Others);
		}
		PlayRandomSound(dice_impact_dice, DICE_DICE_SOUND_MULT*0.05f);
	}
	
	[RPC]
	public void PickUpSound() {
		if(Network.isServer){
			networkView.RPC("PickUpSound",RPCMode.Others);
		}
		PlayRandomSound(dice_pick_up, 0.1f);
	}
	
	[RPC]
	public void PlayImpactSound(int layer, float volume) {
		if(Network.isServer){
			networkView.RPC("PlayImpactSound",RPCMode.Others,layer,volume);
		}
		int table_layer = LayerMask.NameToLayer("Table");
		int board_layer = LayerMask.NameToLayer("Board");
		int card_layer = LayerMask.NameToLayer("Cards");
		if(layer == table_layer){
			PlayRandomSound(dice_impact_wood, volume*DICE_WOOD_SOUND_MULT*DICE_GLOBAL_SOUND_MULT);
		} else if(layer == board_layer || layer == card_layer){			
			PlayRandomSound(dice_impact_board, volume*DICE_BOARD_SOUND_MULT*DICE_GLOBAL_SOUND_MULT);
		} else {
			PlayRandomSound(dice_impact_dice, volume*DICE_DICE_SOUND_MULT*DICE_GLOBAL_SOUND_MULT);
		}			
	}
	
	void OnCollisionEnter(Collision info) {
		if(info.relativeVelocity.magnitude > 1.0f && Time.time > last_sound_time + PHYSICS_SOUND_DELAY) { 
			float volume = info.relativeVelocity.magnitude*0.1f;
			int layer = info.collider.gameObject.layer;
			PlayImpactSound(layer,volume);
			last_sound_time = Time.time;
		}	
	}
}
