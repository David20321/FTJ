using UnityEngine;
using System.Collections;

public class CoinScript : MonoBehaviour {
	public AudioClip[] impact_sound;
	public AudioClip[] pick_up_sound;
	float last_sound_time = 0.0f;
	const float PHYSICS_SOUND_DELAY = 0.1f;
	
	[RPC]
	public void PickUpSound() {
		if(Network.isServer){
			networkView.RPC("PickUpSound",RPCMode.Others);
		}
		PlayRandomSound(pick_up_sound, 0.1f);
	}
	
	void PlayRandomSound(AudioClip[] clips, float volume){
		audio.PlayOneShot(clips[Random.Range(0,clips.Length)], volume);
	}		
	
	[RPC]
	void ImpactSound(float volume){
		if(Network.isServer){
			networkView.RPC("ImpactSound",RPCMode.Others,volume);
		}
		PlayRandomSound(impact_sound, volume*0.3f);		
	}
	
	void OnCollisionEnter(Collision info) {
		if(info.relativeVelocity.magnitude > 1.0f && Time.time > last_sound_time + PHYSICS_SOUND_DELAY) { 
			float volume = info.relativeVelocity.magnitude*0.1f;
			ImpactSound(volume);
			last_sound_time = Time.time;
		}	
	}
}
