using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PhysicsController))]
public class GroundDetection : MonoBehaviour 
{
	public bool detectGround;												// Allows raycast in gravity direction to set isGrounded to true when colliding with ground.
	public bool stickToGround;
	public bool setGravDirToGround;											// Set ground detection raycast direction to the opposite of the ground hit normal. 
	public bool useGravDirAsRaycast;										// Uses the local physics script gravityDirection as the raycast direction, otherwise uses the transform down (-transform.up).
	public bool isRayHit;													// Set to true if ground raycast hits an object collider.
	public RaycastHit groundRayHit;
	public bool isTerrain;
	public bool isGrounded;													// Set to true if rayHit is true and ignoreRayHit is false.
	public bool ignoreRayHit;												// If true, isGrounded is not set to true when rayHit is true.

	public float groundRayLength = 1.3f;									// The length of the raycast that detects the ground.
	public float globalGroundAngle;
	public float localGroundAngle;

	private GameController gameController;
	private PhysicsController localPhysics;


	void Start ()
	{
		gameController = GameObject.FindWithTag ("GameController").GetComponent<GameController> ();
		localPhysics = GetComponent<PhysicsController>();
	}

	void Update ()
	{
		if (detectGround) DetectGroundHit ();
		else localPhysics.isGrounded = false;

		SetGrounded ();
		UpdatePhysicsGrounded ();
		if (stickToGround) StickToGround ();
	}

	void OnDisable()
	{
		isRayHit = false;
		isGrounded = false;
		localPhysics.isGrounded = false;
	}


	void DetectGroundHit()
	{
		Vector3 localGroundDirection = -transform.up;
		if (useGravDirAsRaycast) localGroundDirection =  localPhysics.localGravDirection;

		if (localPhysics.isGrounded && setGravDirToGround) localPhysics.localGravDirection = -groundRayHit.normal;

		Debug.DrawRay (transform.position, localGroundDirection * groundRayLength, Color.red);

		if (Physics.Raycast (transform.position, localGroundDirection, out groundRayHit, groundRayLength)) 
		{
			isRayHit = true;
			if (groundRayHit.collider.gameObject.layer == 9) isTerrain = true;
			else isTerrain = false;
		} 

		else 
		{
			isRayHit = false;
			isTerrain = false;
		}

		if (isRayHit) 
		{
			globalGroundAngle = Vector3.Angle (-gameController.globalGravDirection, groundRayHit.normal) * Mathf.Sign (groundRayHit.normal.x);
			localGroundAngle = Vector3.Angle (-localPhysics.localGravDirection, groundRayHit.normal) * Mathf.Sign (groundRayHit.normal.x);
		}

		else 
		{
			globalGroundAngle = 0;
			localGroundAngle = 0;
		}
	}

	void SetGrounded()
	{
		if (isRayHit && !ignoreRayHit) isGrounded = true;
		if (!isRayHit || ignoreRayHit) isGrounded = false;
	}

	void UpdatePhysicsGrounded()
	{
		localPhysics.isGrounded = isGrounded;
	}

	void StickToGround()
	{
		if(isGrounded) transform.position -= groundRayHit.normal * (groundRayHit.distance - 1f);
	}
}
