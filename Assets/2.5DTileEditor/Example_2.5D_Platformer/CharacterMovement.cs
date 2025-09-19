using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour {

	const float accelTime = 0.2f;
	const float walkSpeed = 6f;
	const float airTrshold = 0.25f;
	const float jumpSpeed = 12f;
	const float airSteering = 1f;
	const float drag = 1f;
	const float sharpEdgeTrshold = 2f;

	CharacterController cc;
	Vector3 velocity;
	bool onGround;


	private void Awake() {
		cc = this.GetComponent<CharacterController>();
	}

	private void Start() {
		cc.Move(-airTrshold * transform.up);
	}

	private void Update () {
		float horiz = Input.GetAxis("Horizontal");
		bool jump = Input.GetKey(KeyCode.Space);

		Vector3 movement = walkSpeed * horiz * this.transform.right;
		this.velocity = Vector3.ProjectOnPlane(this.velocity, this.transform.forward);
		
		if(onGround) {
			GroundMove(movement, jump);
		}
		else {
			AirMove(movement);
		}

        Animator anim = this.GetComponentInChildren<Animator>();
        if(anim)
            anim.SetBool("Move", velocity.magnitude > 0.05f);
	}

	/// <summary>
	/// Move character over ground over one Update frame.
	/// </summary>
	private void GroundMove(Vector3 movement, bool jump) {
		float dt = Time.deltaTime * (walkSpeed / accelTime);
		float horVel = Vector3.Dot(velocity, transform.right);
		horVel = Mathf.MoveTowards(horVel, Vector3.Dot(movement, this.transform.right), dt);
		velocity = horVel * transform.right;

		if(jump) {
			onGround = false;
			this.velocity.y = jumpSpeed;
			Move(Physics.gravity);
		}
		else {
			Walk();
			if(!onGround)
				velocity.y = 0f;
		}
	}

	/// <summary>
	/// Move character trough the air over one Update frame.
	/// </summary>
	private void AirMove(Vector3 movement) {
		Vector3 acceleration = movement * airSteering + Physics.gravity;
		acceleration -= this.velocity * drag;
		this.velocity += acceleration * Time.deltaTime;
		Move(acceleration);
	}

	/// <summary>
	/// Moves player over the ground at his velocity.
	/// </summary>
	void Walk() {
		Vector3 horizVel = Vector3.Project(velocity, this.transform.right);
		Vector3 horizMove = horizVel * Time.deltaTime;
		Vector3 vertShift = transform.up * airTrshold;

		onGround = false;

		cc.Move(vertShift);
		cc.Move(horizMove);
		cc.Move(-2 * vertShift);
	}

	/// <summary>
	/// Detects and handles colisions with terrain
	/// </summary>
	void OnControllerColliderHit(ControllerColliderHit hit) {
		float normalY = Vector3.Dot(hit.normal, transform.up);

		if(normalY > Mathf.Sin(Mathf.PI/6f)) 
			onGround = true;
		else if(Vector3.Dot(hit.normal, velocity) < 0f)
			velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
	}

	/// <summary>
	/// Move one Update frame at velocity and the given acceleration.
	/// </summary>
	private void Move(Vector3 acceleration) {
		float dt = Time.deltaTime;
		Vector3 toMove = velocity * dt;
		toMove = toMove + acceleration * dt * dt / 2f;
		toMove = Vector3.ProjectOnPlane(toMove, this.transform.forward);
		cc.Move(toMove);
	}
}
