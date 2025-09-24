using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltPlane : MonoBehaviour {

	Rigidbody rb;
	Quaternion baseRot;

	const float tiltRange = 10f;

	private void Awake() {
		rb = this.GetComponent<Rigidbody>();
		baseRot = transform.rotation;
	}

	private void Update () {
		float horiz = Input.GetAxis("Horizontal");
		float vert = Input.GetAxis("Vertical");

		float t = Time.deltaTime * 2f;
		Quaternion goalRot = baseRot * Quaternion.Euler(tiltRange*vert, -tiltRange*horiz, 0f);
		Quaternion rot = Quaternion.Lerp(rb.rotation, goalRot, t);
		rb.MoveRotation(rot);
	}
}
