using UnityEngine;
using System.Collections;

public class DrawEmptyScript : MonoBehaviour {
	void OnDrawGizmos()	{
	  Gizmos.color = Color.blue;
	  Gizmos.DrawWireSphere(transform.position, 0.1f);
	}
}
