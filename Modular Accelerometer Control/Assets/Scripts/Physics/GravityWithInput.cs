using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PhysicsController))]
public class GravityWithInput : MonoBehaviour 
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
		if (whenGrounded && localPhysics.isGrounded || whenInAir && !localPhysics.isGrounded) 
		{
			if (localPhysics.localInputFound || localPhysics.useGlobalInput)
			{
				SetGravityDirection ();
			} 
		} 

		if (!whenInAir && !localPhysics.isGrounded || (!localPhysics.localInputFound && !localPhysics.useGlobalInput))
		{
			localPhysics.ignoreGlobalGravDir = false;

			if ((!localPhysics.localInputFound && !localPhysics.useGlobalInput) && (whenGrounded || whenInAir)) Debug.LogWarning ("No input found. Attach an input script to the physics object or enable global input to use inputVector features.");
		}
	}

	void OnDisable()
	{
		localPhysics.ignoreGlobalGravDir = false;
	}


	void SetGravityDirection()
	{
		Vector3 localInputVector = localPhysics.inputVector;

		if (reverseUpVector) localInputVector = -localInputVector;

		if (Mathf.Abs (localInputVector.magnitude) > 0) 
		{
			localPhysics.ignoreGlobalGravDir = true;
			localPhysics.localGravDirection = localInputVector;
		}
	}
}
