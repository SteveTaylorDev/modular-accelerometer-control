using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PhysicsController))]
public class RotateWithGlobalGravity : MonoBehaviour 
{
	public bool whenGrounded;
	public bool whenInAir;
	public bool reverseUpVector;					// If true, sets the upward rotation destination to the inverse of itself.

	private GameController gameController;
	private PhysicsController localPhysics;


	void Start ()
	{
		gameController = GameObject.FindWithTag ("GameController").GetComponent<GameController>();
		localPhysics = GetComponent<PhysicsController>();
	}

	void Update () 
	{
		if (whenGrounded && localPhysics.isGrounded || whenInAir && !localPhysics.isGrounded) SetTargetRotation ();
	}


	void SetTargetRotation()
	{
		Vector3 localGravVector = -gameController.globalGravDirection;

		if (reverseUpVector) localGravVector = -localGravVector;

		localPhysics.targetRotation = Quaternion.FromToRotation (Vector3.up, localGravVector);
	}
}
