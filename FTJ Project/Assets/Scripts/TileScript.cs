using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {
	public Material multiply;
	
	public Material Bake() {
		var camera_obj = transform.FindChild("Camera").gameObject;
		var camera = camera_obj.GetComponent<Camera>();
		var render_texture = new RenderTexture(256,256,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default);
		render_texture.useMipMap = true;
		render_texture.filterMode = FilterMode.Trilinear;
		render_texture.mipMapBias = -0.5f;
		camera.targetTexture = render_texture; 
		
		transform.FindChild("Title").gameObject.layer = LayerMask.NameToLayer("Active Card Render Texture");
		transform.FindChild("Rules").gameObject.layer = LayerMask.NameToLayer("Active Card Render Texture");
		
		var tile_mesh = transform.FindChild("Tile_base").FindChild("default");
		var material = new Material(tile_mesh.renderer.material);
		material.mainTexture = render_texture;
		
		camera.Render();
		transform.FindChild("Title").gameObject.layer = LayerMask.NameToLayer("Card Render Texture");
		transform.FindChild("Rules").gameObject.layer = LayerMask.NameToLayer("Card Render Texture");
		
		var return_material = new Material(multiply);
		return_material.mainTexture = render_texture;
		return return_material;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
