using UnityEngine;
using System.Collections;

public class CardCreationScript : MonoBehaviour {

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
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
