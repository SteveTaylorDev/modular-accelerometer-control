using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (CameraController))]
public class LookAtTarget : MonoBehaviour 
{	
	public bool lookAtTarget;
	public bool ignoreDefaultOffset;

	private CameraController localCameraController;					// The main CameraController script on this object.


	void Start ()
	{
		localCameraController = GetComponent<CameraController> ();
	}

	void Update () 
	{
		// If lookAtTarget is true, sets camera forward to face target object. (This is set to execute last in script execution order, keeping upward rotation intact).
		if (lookAtTarget) SetCameraRotation ();
	}


	void SetCameraRotation()
	{
		// Find localForward by subtracting camera position from target position.
		Vector3 localForward = ((localCameraController.targetPosition - new Vector3 (0, 0, localCameraController.defaultOffset.z)) - transform.position).normalized;

		// If ignoreDefaultOffset is true, subract the default offset from the targetPosition when finding the local forward.
		if (ignoreDefaultOffset) localForward = ((localCameraController.targetPosition - localCameraController.defaultOffset) - transform.position).normalized;

		// Find the 'up' vector for the new LookRotation based off of the current localCameraController 'cameraRotation' quaternion.
		// Uses the z euler angle of cameraRotation in a Quaternion.Euler, as the z component, multiplied by Vector3.up to create a Vector3.
		Vector3 camRotZVector = Quaternion.Euler (0, 0, localCameraController.targetRotation.eulerAngles.z) * Vector3.up;

		// Sets the camera controller cameraRotation to a LookRotation made from localForward and camRotZVector.
		localCameraController.targetRotation = Quaternion.LookRotation (localForward, camRotZVector);
	}
}
