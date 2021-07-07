using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PhysicsController))]
public class RotateWithLocalGravity : MonoBehaviour 
{
	public bool whenGrounded;
	public bool whenInAir;
	public bool reverseUpVector;					// If true, sets the upward rotation destination to the inverse of itself.

	private PhysicsController localPhysics;


	void Start ()
	{
		localPhysics = GetComponent<PhysicsController>();
	}

	void Update () 
	{
		if (whenGrounded && localPhysics.isGrounded || whenInAir && !localPhysics.isGrounded) SetTargetRotation ();
	}


	void SetTargetRotation()
	{
		Vector3 localGravVector = -localPhysics.localGravDirection;

		if (reverseUpVector) localGravVector = -localGravVector;

		localPhysics.targetRotation = Quaternion.LookRotation (transform.forward, localGravVector);
	}
}
