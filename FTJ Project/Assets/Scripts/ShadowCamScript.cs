using UnityEngine;
using System.Collections;

public class ShadowCamScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	float old_light_intensity;
	Color old_ambient_light;
	
	void OnPreRender() {
		old_light_intensity = GameObject.Find ("Point light").light.intensity;
		GameObject.Find ("Point light").light.intensity = 0.0f;
		old_ambient_light = RenderSettings.ambientLight;
		RenderSettings.ambientLight = Color.black;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogStartDistance = -100.0f;
		RenderSettings.fogEndDistance = 100.0f;
		RenderSettings.fogDensity = 0.5f;
		RenderSettings.fogColor = Color.white;
		RenderSettings.fog = true;
	}
	
	void OnPostRender() {
		GameObject.Find ("Point light").light.intensity = old_light_intensity;
		RenderSettings.ambientLight = old_ambient_light;
		RenderSettings.fog = false;
	}
}
