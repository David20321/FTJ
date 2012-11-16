using UnityEngine;
using System.Collections;

public class CardBakeScript : MonoBehaviour {
	public Texture2D[] content_textures;
	public Material cutout;
	public CardData card_data_ = new CardData();
	
	public void SetCardData(CardData card_data){
		card_data_ = card_data;
	}
	
	public Material Bake() {
		var title_text = transform.FindChild("Title").GetComponent<TextMesh>();
		title_text.text = card_data_.title;
		var type_text = transform.FindChild("Type").GetComponent<TextMesh>();
		type_text.text = card_data_.type;
		var rules_text = transform.FindChild("Rules").GetComponent<TextMesh>();
		rules_text.text = card_data_.rules;
		var flavour_text = transform.FindChild("Flavour").GetComponent<TextMesh>();
		flavour_text.text = card_data_.flavour;
		
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
		
		int active_card_layer = LayerMask.NameToLayer("Active Card Render Texture");
		transform.FindChild("Title").gameObject.layer = active_card_layer;
		transform.FindChild("Type").gameObject.layer = active_card_layer;
		transform.FindChild("Rules").gameObject.layer = active_card_layer;
		transform.FindChild("Flavour").gameObject.layer = active_card_layer;
		transform.FindChild("Contents").FindChild("default").gameObject.layer = active_card_layer;
		transform.FindChild("AlphaBackdrop").FindChild("default").gameObject.layer = active_card_layer;
		transform.FindChild("FrontBorder").FindChild("default").gameObject.layer = active_card_layer;
		camera.Render();
		int card_layer = LayerMask.NameToLayer("Card Render Texture");
		transform.FindChild("Title").gameObject.layer = card_layer;
		transform.FindChild("Type").gameObject.layer = card_layer;
		transform.FindChild("Rules").gameObject.layer = card_layer;
		transform.FindChild("Flavour").gameObject.layer = card_layer;
		transform.FindChild("Contents").FindChild("default").gameObject.layer = card_layer;
		transform.FindChild("AlphaBackdrop").FindChild("default").gameObject.layer = card_layer;
		transform.FindChild("FrontBorder").FindChild("default").gameObject.layer = card_layer;
		Material new_material = new Material(cutout);
		new_material.mainTexture = render_texture;

		return new_material;
	}
}
