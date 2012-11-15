using UnityEngine;
using System.Collections;

public class CardScript : MonoBehaviour {
	public Texture2D[] back_textures;
	public Texture2D[] content_textures;
	public Material cutout;
	public CardData card_data_ = new CardData();
	
	public void SetCardData(CardData card_data){
		card_data_ = card_data;
	}
	
	public void Bake() {
		var title_text = transform.FindChild("Title").GetComponent<TextMesh>();
		title_text.text = card_data_.title;
		var type_text = transform.FindChild("Type").GetComponent<TextMesh>();
		type_text.text = card_data_.type;
		var rules_text = transform.FindChild("Rules").GetComponent<TextMesh>();
		rules_text.text = card_data_.rules;
		var flavour_text = transform.FindChild("Flavour").GetComponent<TextMesh>();
		flavour_text.text = card_data_.flavour;
		var card_back = transform.FindChild("Back").transform.FindChild("default");
		card_back.renderer.material = new Material(card_back.renderer.material);
		card_back.renderer.material.mainTexture = back_textures[card_data_.back];
		var card_contents = transform.FindChild("Contents").transform.FindChild("default");
		card_contents.renderer.material = new Material(card_contents.renderer.material);
		card_contents.renderer.material.mainTexture = content_textures[card_data_.image];
	
		var camera_obj = transform.FindChild("Camera").gameObject;
		var camera = camera_obj.GetComponent<Camera>();
		var render_texture = new RenderTexture(512,512,24,RenderTextureFormat.Default,RenderTextureReadWrite.Default);
		render_texture.useMipMap = true;
		render_texture.filterMode = FilterMode.Trilinear;
		render_texture.mipMapBias = -0.5f;
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
		transform.FindChild("FrontBorder").FindChild("default").gameObject.layer = 12;
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
