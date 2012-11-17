using UnityEngine;
using System.Collections;

public class ParentTokenScript : MonoBehaviour {
	public int owner_id_;
	public AudioClip[] token_impact;
	public AudioClip[] pick_up_sound;
	float last_sound_time = 0.0f;
	const float PHYSICS_SOUND_DELAY = 0.1f;
	public GameObject[] mesh_prefabs;
	GameObject mesh_object = null;
	
	void Start () {
		if(Network.isServer){
			owner_id_ = -1;
		}
	}
	
	
	public void AssignMesh (int which) {
		mesh_object = (GameObject)GameObject.Instantiate(mesh_prefabs[which], transform.position, transform.rotation);
		mesh_object.transform.parent = transform;
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
	
	public void PickUpSound() {
		PlayRandomSound(pick_up_sound, 0.1f);
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
