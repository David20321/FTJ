using UnityEngine;
using System.Collections;


public class DiceScript : MonoBehaviour {
	public int id_;
	public int held_by_player_;
	public int owner_id_;
	public enum Type {DIE, TOKEN};
	public Type type_;
	public AudioClip[] dice_impact_wood;
	public AudioClip[] dice_impact_board;
	public AudioClip[] dice_impact_dice;
	float last_sound_time = 0.0f;
	const float PHYSICS_SOUND_DELAY = 0.1f;
	const float DICE_GLOBAL_SOUND_MULT = 0.3f;
	const float DICE_BOARD_SOUND_MULT = 0.4f;
	const float DICE_WOOD_SOUND_MULT = 1.0f;
	const float DICE_DICE_SOUND_MULT = 1.0f;
	
	void PlayRandomSound(AudioClip[] clips, float volume = 1.0f){
		audio.PlayOneShot(clips[Random.Range(0,clips.Length)], volume);
	}		
	
	void Start () {
		BoardScript.Instance().RegisterDiceObject(gameObject);
		if(Network.isServer){
			held_by_player_ = -1;
			owner_id_ = -1;
		}
	}
	
	void OnDestroy() {
		if(BoardScript.Instance()){
			BoardScript.Instance().UnRegisterDiceObject(gameObject);
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
		if(stream.isWriting){
			int id = id_;
			stream.Serialize(ref id);
			int owner_id = owner_id_;
			stream.Serialize(ref owner_id);
		} else {
			int id = -1;
			stream.Serialize(ref id);
			id_ = id;
			int owner_id = -1;
			stream.Serialize(ref owner_id);
			owner_id_ = owner_id;
		}
	}
	
	void OnCollisionEnter(Collision info) {
		if(type_ == Type.DIE){
			if(info.relativeVelocity.magnitude > 1.0f && Time.time > last_sound_time + PHYSICS_SOUND_DELAY) { 
				float volume = info.relativeVelocity.magnitude*0.1f;
				switch(info.collider.gameObject.layer){
					case 8:
						PlayRandomSound(dice_impact_wood, volume*DICE_WOOD_SOUND_MULT*DICE_GLOBAL_SOUND_MULT);
						break;
					case 9:						
						PlayRandomSound(dice_impact_board, volume*DICE_BOARD_SOUND_MULT*DICE_GLOBAL_SOUND_MULT);
						break;
					default:
						PlayRandomSound(dice_impact_dice, volume*DICE_DICE_SOUND_MULT*DICE_GLOBAL_SOUND_MULT);
						break;
				}
				last_sound_time = Time.time;
			}	
		}
	}
}
