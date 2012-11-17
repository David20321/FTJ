using UnityEngine;
using System.Collections;

public class CoinScript : MonoBehaviour {
	public AudioClip[] impact_sound;
	public AudioClip[] pick_up_sound;
	float last_sound_time = 0.0f;
	const float PHYSICS_SOUND_DELAY = 0.1f;
	
	public void PickUpSound() {
		PlayRandomSound(pick_up_sound, 0.1f);
	}
	
	void PlayRandomSound(AudioClip[] clips, float volume){
		audio.PlayOneShot(clips[Random.Range(0,clips.Length)], volume);
	}		
	
	void OnCollisionEnter(Collision info) {
		if(info.relativeVelocity.magnitude > 1.0f && Time.time > last_sound_time + PHYSICS_SOUND_DELAY) { 
			float volume = info.relativeVelocity.magnitude*0.1f;
			PlayRandomSound(impact_sound, volume*0.3f);
			last_sound_time = Time.time;
		}	
	}
}
