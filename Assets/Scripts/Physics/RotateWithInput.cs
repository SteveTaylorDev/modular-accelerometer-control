using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PhysicsController))]
public class RotateWithInput : MonoBehaviour 
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
		if (localPhysics.localInputFound || localPhysics.useGlobalInput) 
		{
			SetTargetRotation ();
		}

		if ((!localPhysics.localInputFound && !localPhysics.useGlobalInput) && (whenGrounded || whenInAir)) Debug.LogWarning ("No input found. Attach an input script to the physics object or enable global input to use inputVector features.");
	}


	void SetTargetRotation()
	{
		Vector3 localInputVector = localPhysics.inputVector;

		if (reverseUpVector) localInputVector = -localInputVector;

		if (whenGrounded && localPhysics.isGrounded || whenInAir && !localPhysics.isGrounded) localPhysics.targetRotation = Quaternion.FromToRotation (Vector3.up, localInputVector);
	}
}
