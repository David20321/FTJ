using UnityEngine;
using System.Collections;

public class TokenScript : MonoBehaviour {
	public int owner_id_;
	public AudioClip[] token_impact;
	float last_sound_time = 0.0f;
	const float PHYSICS_SOUND_DELAY = 0.1f;
	
	void Start () {
		if(Network.isServer){
			owner_id_ = -1;
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
		if(stream.isWriting){
			int owner_id = owner_id_;
			stream.Serialize(ref owner_id);
		} else {
			int owner_id = -1;
			stream.Serialize(ref owner_id);
			owner_id_ = owner_id;
		}
	}
	
	void PlayRandomSound(AudioClip[] clips, float volume){
		audio.PlayOneShot(clips[Random.Range(0,clips.Length)], volume);
	}		
	
	void OnCollisionEnter(Collision info) {
		if(info.relativeVelocity.magnitude > 1.0f && Time.time > last_sound_time + PHYSICS_SOUND_DELAY) { 
			float volume = info.relativeVelocity.magnitude*0.1f;
			PlayRandomSound(token_impact, volume*0.3f);
			last_sound_time = Time.time;
		}	
	}
}
