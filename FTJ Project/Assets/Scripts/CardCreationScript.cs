using UnityEngine;
using System.Collections;

public class CardCreationScript : MonoBehaviour {
	public Texture2D[] back_textures;
	public Material pure_white;
	public Material cutout;
	enum BackID{DUNGEON_ONE = 0, DUNGEON_TWO = 1, DUNGEON_THREE = 2, HERO = 3, NATURE = 4, TOWN = 5};
	
	public void Bake() {
		var title_text = transform.FindChild("Title").GetComponent<TextMesh>();
		title_text.text = "Chasm Guardian";
		var type_text = transform.FindChild("Type").GetComponent<TextMesh>();
		type_text.text = "Arachnid Monster";
		var rules_text = transform.FindChild("Rules").GetComponent<TextMesh>();
		rules_text.text = "On 1 or 2: skip a turn\nOn 5 or 6: draw four, pick one";
		var flavour_text = transform.FindChild("Flavour").GetComponent<TextMesh>();
		flavour_text.text = "\"I hate spiders!\" -Some dude";
		var card_back = transform.FindChild("Back").transform.FindChild("default");
		card_back.renderer.material.mainTexture = back_textures[(int)BackID.DUNGEON_ONE];
	
		var camera_obj = transform.FindChild("Camera").gameObject;
		var camera = camera_obj.GetComponent<Camera>();
		var render_texture = new RenderTexture(512,512,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default);
		render_texture.useMipMap = true;
		render_texture.filterMode = FilterMode.Trilinear;
		camera.targetTexture = render_texture; 
		
		transform.FindChild("Title").gameObject.layer = 14;
		transform.FindChild("Type").gameObject.layer = 14;
		transform.FindChild("Rules").gameObject.layer = 14;
		transform.FindChild("Flavour").gameObject.layer = 14;
		transform.FindChild("Contents").FindChild("default").gameObject.layer = 14;
		transform.FindChild("AlphaBackdrop").FindChild("default").gameObject.layer = 14;
		transform.FindChild("FrontBorder").FindChild("default").gameObject.layer = 14;
		camera.Render();
		GameObject.Destroy(camera_obj);
		GameObject.Destroy(transform.FindChild("Title").gameObject);
		GameObject.Destroy(transform.FindChild("Type").gameObject);
		GameObject.Destroy(transform.FindChild("Rules").gameObject);
		GameObject.Destroy(transform.FindChild("Flavour").gameObject);
		GameObject.Destroy(transform.FindChild("Contents").gameObject);
		GameObject.Destroy(transform.FindChild("AlphaBackdrop").gameObject);
		Material new_material = new Material(cutout);
		new_material.mainTexture = render_texture;
		transform.FindChild("FrontBorder").FindChild("default").renderer.material = new_material;
	}
	
	// Use this for initialization
	void Start () {
		Bake();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
