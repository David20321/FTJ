using UnityEngine;
using System.Collections;

public class TokenScript : MonoBehaviour {
	public int owner_id_;
	public AudioClip[] token_impact;
	public AudioClip[] pick_up_sound;
	float last_sound_time = 0.0f;
	const float PHYSICS_SOUND_DELAY = 0.1f;
	public GameObject[] mesh_prefabs;
	
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
		PlayRandomSound(token_impact, volume*0.3f);		
	}
	
	void OnCollisionEnter(Collision info) {
		if(info.relativeVelocity.magnitude > 1.0f && Time.time > last_sound_time + PHYSICS_SOUND_DELAY) { 
			float volume = info.relativeVelocity.magnitude*0.1f;
			ImpactSound(volume);
			last_sound_time = Time.time;
		}	
	}
	
	[RPC]
	public void SetBloodColor() {
		renderer.material.color = new Color(0.7f,0.2f,0.2f);
		if(Network.isServer){
			networkView.RPC("SetBloodColor",RPCMode.OthersBuffered);
		}
	}
}
