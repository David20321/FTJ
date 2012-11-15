using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {
	public Material pure_white;
	
	public void Bake() {
		var camera_obj = transform.FindChild("Camera").gameObject;
		var camera = camera_obj.GetComponent<Camera>();
		var render_texture = new RenderTexture(256,256,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default);
		render_texture.useMipMap = true;
		render_texture.filterMode = FilterMode.Trilinear;
		render_texture.mipMapBias = -0.5f;
		camera.targetTexture = render_texture; 
		
		transform.FindChild("Title").gameObject.layer = 14;
		transform.FindChild("Rules").gameObject.layer = 14;
		
		var tile_mesh = transform.FindChild("Tile_base").FindChild("default");
		var material = new Material(tile_mesh.renderer.material);
		material.mainTexture = render_texture;
		tile_mesh.renderer.material = pure_white;
		
		camera.Render();
		GameObject.Destroy(camera_obj);
		GameObject.Destroy(transform.FindChild("Title").gameObject);
		GameObject.Destroy(transform.FindChild("Rules").gameObject);
		
		tile_mesh.renderer.material = material;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
