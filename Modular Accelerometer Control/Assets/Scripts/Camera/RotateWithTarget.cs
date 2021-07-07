using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (CameraController))]
public class RotateWithTarget : MonoBehaviour 
{
	public bool upWithPhysicsInput;
	public bool upWithPhysicsGravity;
	public bool upWithTransform;
	public bool reverseUpDirection;

	private CameraController localCameraController;					// The main follow target script on the camera.
	private PhysicsController targetPhysics;						// The 'PhysicsController' script attached to the parent of cameraTarget (Pulled from 'FollowCameraTarget' script).


	void Start () 
	{
		localCameraController = GetComponent<CameraController> ();
	}

	void Update () 
	{
		// Pull targetPhysics from the CameraController script.
		if (targetPhysics == null) targetPhysics = localCameraController.targetPhysics;

		if (targetPhysics != null) SetCameraRotation ();
	}


	void SetCameraRotation()
	{
		Vector3 localForward = transform.forward;
		Vector3 localUp = transform.up;

		if (upWithPhysicsInput) localUp = targetPhysics.inputVector;

		if (upWithPhysicsGravity) localUp = -targetPhysics.localGravDirection;

		if (upWithTransform) localUp = targetPhysics.transform.up;

		if (reverseUpDirection && localUp != transform.up) localUp = -localUp;

		Quaternion localRotation = Quaternion.LookRotation (localForward, localUp);

		// Sets the camera controller cameraRotation to a LookRotation made from localForward and localUp.
		localCameraController.targetRotation = localRotation;
	}
}
