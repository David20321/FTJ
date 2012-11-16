using UnityEngine;
using System.Collections;

public class DrawSpawnerScript : MonoBehaviour {
	public GameObject prefab;
	void OnDrawGizmos()	{
	  Gizmos.color = Color.blue;
	  Gizmos.matrix = transform.localToWorldMatrix;
	  Gizmos.DrawWireCube(prefab.GetComponent<BoxCollider>().center, prefab.GetComponent<BoxCollider>().extents * 2.0f);
	}
}
