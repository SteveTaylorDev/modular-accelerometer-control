using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PhysicsController))]
public class ArcadeRedirector : MonoBehaviour
{
	public bool enableRedirect;
	public bool withInput;
	public bool withGravity;
	public bool localTransformInput;
	public bool smoothRedirect;
	[Range(0, 0.3f)]
	public float smoothAmount;

	private PhysicsController localPhysics;


	void OnEnable () 
	{
		localPhysics = GetComponent<PhysicsController> ();
	}

	void FixedUpdate () 
	{
		if (enableRedirect) RedirectMoveVector ();
	}


	void RedirectMoveVector()
	{
		Vector3 redirVector = Vector3.zero;

		if (!localPhysics.localInputFound && !localPhysics.useGlobalInput && withInput) 
		{
			Debug.LogWarning ("No input found on PhysicsController script. Add local input script, use global input, or disable WithInput on this script.");
		}

		if (!localPhysics.noInput && withInput)	redirVector = localPhysics.inputVector.normalized;

		if (withGravity) redirVector = localPhysics.localGravDirection;

		if (localTransformInput) redirVector = transform.TransformDirection (redirVector);
			
		if (smoothRedirect) redirVector = Vector3.Lerp (localPhysics.arcadeMoveDir, redirVector, smoothAmount);

		if (redirVector.magnitude > 0) localPhysics.SetArcadeDirection(redirVector);
	}
}
