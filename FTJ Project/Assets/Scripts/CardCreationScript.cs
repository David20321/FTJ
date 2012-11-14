using UnityEngine;
using System.Collections;

public class CardCreationScript : MonoBehaviour {
	public Texture2D[] back_textures;
	enum BackID{DUNGEON_ONE = 0, DUNGEON_TWO = 1, DUNGEON_THREE = 2, HERO = 3, NATURE = 4, TOWN = 5};
	
	// Use this for initialization
	void Start () {
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
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
