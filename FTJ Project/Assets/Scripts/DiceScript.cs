using UnityEngine;
using System.Collections;

public class DiceScript : MonoBehaviour {
	public AudioClip[] dice_impact_wood;
	public AudioClip[] dice_impact_board;
	public AudioClip[] dice_impact_dice;
	float last_sound_time = 0.0f;
	const float PHYSICS_SOUND_DELAY = 0.1f;
	const float DICE_GLOBAL_SOUND_MULT = 0.3f;
	const float DICE_BOARD_SOUND_MULT = 0.4f;
	const float DICE_WOOD_SOUND_MULT = 1.0f;
	const float DICE_DICE_SOUND_MULT = 1.0f;
	
	void PlayRandomSound(AudioClip[] clips, float volume){
		audio.PlayOneShot(clips[Random.Range(0,clips.Length)], volume);
	}		
	
	void OnCollisionEnter(Collision info) {
		if(info.relativeVelocity.magnitude > 1.0f && Time.time > last_sound_time + PHYSICS_SOUND_DELAY) { 
			float volume = info.relativeVelocity.magnitude*0.1f;
			int table_layer = LayerMask.NameToLayer("Table");
			int board_layer = LayerMask.NameToLayer("Board");
			int layer = info.collider.gameObject.layer;
			if(layer == table_layer){
				PlayRandomSound(dice_impact_wood, volume*DICE_WOOD_SOUND_MULT*DICE_GLOBAL_SOUND_MULT);
			} else if(layer == board_layer){			
				PlayRandomSound(dice_impact_board, volume*DICE_BOARD_SOUND_MULT*DICE_GLOBAL_SOUND_MULT);
			} else {
				PlayRandomSound(dice_impact_dice, volume*DICE_DICE_SOUND_MULT*DICE_GLOBAL_SOUND_MULT);
			}
			last_sound_time = Time.time;
		}	
	}
}
