using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class JSONTest : MonoBehaviour {
	public TextAsset file;
	
	void MiniJSONTest () {
        var jsonString = "{ \"array\": [1.44,2,3], " +
                        "\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
                        "\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
                        "\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
                        "\"int\": 65536, " +
                        "\"float\": 3.1415926, " +
                        "\"bool\": true, " +
                        "\"null\": null }";

        var dict = Json.Deserialize(jsonString) as Dictionary<string,object>;

        Debug.Log("deserialized: " + dict.GetType());
        Debug.Log("dict['array'][0]: " + ((List<object>) dict["array"])[0]);
        Debug.Log("dict['string']: " + (string) dict["string"]);
        Debug.Log("dict['float']: " + (double) dict["float"]); // floats come out as doubles
        Debug.Log("dict['int']: " + (long) dict["int"]); // ints come out as longs
        Debug.Log("dict['unicode']: " + (string) dict["unicode"]);

        var str = Json.Serialize(dict);

        Debug.Log("serialized: " + str);
    }
    
	// Use this for initialization
	void Start () {
		var dict = Json.Deserialize(file.text) as Dictionary<string,object>;
		foreach(var pair in dict){
			Debug.Log (pair.Key);
			List<object> list = (List<object>)pair.Value;
			foreach(var obj in list){
				Dictionary<string,object> dict2 = (Dictionary<string,object>)obj;
				foreach(var pair2 in dict2){
					Debug.Log (pair2.Key+", "+pair2.Value);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
