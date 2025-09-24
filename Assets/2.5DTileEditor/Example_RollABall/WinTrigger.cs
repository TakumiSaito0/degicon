using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTrigger : MonoBehaviour {

	public GameObject winObject;

	void OnTriggerEnter(Collider other) {
		winObject.SetActive(true);
	}
}
