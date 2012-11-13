using UnityEngine;
using System.Collections;

public class MipmapRenderTextureScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Camera>().targetTexture.useMipMap = true;
		GetComponent<Camera>().targetTexture.filterMode = FilterMode.Trilinear;
		GetComponent<Camera>().targetTexture.anisoLevel = 1;
		
		GetComponent<Camera>().Render();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnPostRender(){
	}
}
