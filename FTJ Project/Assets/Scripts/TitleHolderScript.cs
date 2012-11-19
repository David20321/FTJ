using UnityEngine;
using System.Collections;

public class TitleHolderScript : MonoBehaviour {
	Vector3 light_start;
	Vector3 light_end;
	
	// Use this for initialization
	void Start () {
		light_end = transform.FindChild("Point light end").position;
		light_start = transform.FindChild("Title highlight").position;
	}
	
	// Update is called once per frame
	void Update () {
		float time = Time.time * 0.3f;
		transform.FindChild("Title highlight").position = Vector3.Lerp(light_start, light_end, time - (int)time);
	}
	
	public void Show() {
		transform.FindChild("Blackness").FindChild("default").renderer.enabled = true;
		transform.FindChild("Title").FindChild("default").renderer.enabled = true;
		transform.FindChild("Title highlight").light.intensity = 1.0f;
	}
	
	public void Hide() {
		transform.FindChild("Blackness").FindChild("default").renderer.enabled = false;
		transform.FindChild("Title").FindChild("default").renderer.enabled = false;
		transform.FindChild("Title highlight").light.intensity = 0.0f;
	}
}
