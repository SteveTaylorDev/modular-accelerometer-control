using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (CameraController))]
public class SpeedOffset : MonoBehaviour
{
	public bool allowSpeedOffset;
	public bool offsetZNegative;									// If true, sets the sign of the z reading of the offset to always be negative. This makes the camera zoom out when moving forwards instead of in.
	public bool offsetZPositive;									// If true, sets the sign of the z reading of the offset to always be positive. This makes the camera zoom in when moving forwards instead of out.
	public bool smoothOffset;
	[Range(0, 100)]
	public float smoothOffsetSpeed = 10f;							// The time it takes current offset to match new offset. Lerped camera adjustments use this as t (mostly by deltaTime)
	[Range(-20, 20)]
	public float speedOffsetFactor = 3.5f;							// Factor the accelerometer reading and current gravStrengthPercentage are multiplied by when setting the speed offset. (For speed based camera offset).

	private CameraController localCameraController;					// The main CameraController script on this object.
	private PhysicsController targetPhysics;						// The 'PhysicsController' script attached to the parent of cameraTarget (Pulled from 'CameraController' script).


	void Start ()
	{
		localCameraController = GetComponent<CameraController> ();
	}

	void Update()
	{
		// Pull targetPhysics from the CameraController script.
		if (targetPhysics == null) targetPhysics = localCameraController.targetPhysics;

		if (localCameraController.targetPhysics != null)
		{
			// Controls the camera offset based on the current speed (Based on camera target parent 'PhysicsController').
			SpeedModeManager ();
		} 
	}


	void SpeedModeManager()
	{
		if (allowSpeedOffset)
		{
			// Set an offsetDirection from the normalized targetPhysics rigidbody velocity.
			Vector3 offsetDirection = targetPhysics.localRB.velocity.normalized;

			if (offsetZNegative) offsetDirection.z = -Mathf.Abs(offsetDirection.z);
			if (offsetZPositive) offsetDirection.z = Mathf.Abs(offsetDirection.z);

			// Calculate a vector from the local offsetDirection, the speedOffsetFactor, and the current velocityPercentage.
			// Slerp the current offset to this plus the defaultOffset (mainly for the z offset) by lerpSpeed (and smoothDeltaTime to help with the constant rotating)
			if (smoothOffset) localCameraController.currentOffset = Vector3.Slerp (localCameraController.currentOffset, (offsetDirection * speedOffsetFactor * targetPhysics.velocityPercentage) + localCameraController.defaultOffset, smoothOffsetSpeed * Time.smoothDeltaTime);

			// Or set offset directly is smoothOffset is false.
			else localCameraController.currentOffset = (offsetDirection * speedOffsetFactor * targetPhysics.velocityPercentage) + localCameraController.defaultOffset;
		}

		else 		// If allowSpeedOffsetMode is false...
		{
			// If the CameraController current offset is not equal to the default offset...
			if (localCameraController.currentOffset != localCameraController.defaultOffset) 
			{
				// ...slerp to default offset by lerpSpeed and smoothDeltaTime.
				if (smoothOffset) localCameraController.currentOffset = Vector3.Slerp (localCameraController.currentOffset, localCameraController.defaultOffset, smoothOffsetSpeed * Time.smoothDeltaTime);

				// Or set current offset to default offset directly if smoothOffset is false.
				else localCameraController.currentOffset = localCameraController.defaultOffset;
			}
		}
	}
}
