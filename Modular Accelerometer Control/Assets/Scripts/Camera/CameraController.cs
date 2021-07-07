using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Camera))]
public class CameraController : MonoBehaviour 
{
	[HideInInspector] public PhysicsController targetPhysics;		// The physics component of the parent object of the camera target.
	[HideInInspector] public Vector3 currentPosition;				// Current position of the this object stored in a temporary Vector3. Set when following target position.
	[HideInInspector] public Vector3 finalPosition;					// Transform.position is set to the this vector if followTarget is true (after lerping if smoothFollow is true).

	public Camera localCamera;

	public bool followTarget;
	public bool smoothFollow;
	public bool smoothRotate;
	public bool useTargetRotation;

	[Range(0, 100)]
	public float smoothRotateSpeed = 30;
	[Range(0, 100)]
	public float smoothFollowSpeed = 30;

	public bool useExternalOffset;									// Set to true if external offset script is present (When false, offset is always default offset).
	public Vector3 defaultOffset = new Vector3 (0, 0, -30);
	public Vector3 currentOffset;									// The current camera offset from the target object.

	public Vector3 targetPosition;									// Current position of the cameraTarget object combined with the current offset.
	public Quaternion targetRotation;								// The intended rotation destination, rotation is slerped to this by rotateSpeed. (Passed in externally).

	private GameObject cameraTarget;								// Focal object tagged with "CameraTarget".
	private FollowPlayer targetFollowPlayer;


	void OnEnable () 
	{
		cameraTarget = GameObject.FindWithTag ("CameraTarget");
		localCamera = GetComponent<Camera> ();
	}

	void Update()
	{
		if (targetPhysics == null) 
		{
			targetPhysics = cameraTarget.GetComponentInParent<PhysicsController> ();

			if (targetPhysics == null) targetFollowPlayer = cameraTarget.GetComponentInParent<FollowPlayer> ();
			if (targetFollowPlayer != null) targetPhysics = targetFollowPlayer.playerPhysics;
			if (targetPhysics == null && targetFollowPlayer == null) Debug.LogWarning ("No 'PhysicsController' found on cameraTarget parent. Physics and Input based camera features are disabled.");
		}

		if (targetPhysics == null || !useExternalOffset) 
		{
			currentOffset = defaultOffset;
		}
	}

	void LateUpdate () 
	{
		if (cameraTarget != null) FollowTarget ();
		else Debug.LogError ("No object tagged 'CameraTarget' found. Add this tag to the object you want to focus on.");

		if (useTargetRotation) RotateObject ();
	}


	void FollowTarget()
	{
		// Stores current position in temp value.
		currentPosition = transform.position;

		// Sets targetPosition to the camera target position plus the offset.
		targetPosition = cameraTarget.transform.position + currentOffset;

		if (followTarget)
		{
			if (smoothFollow) finalPosition = Vector3.Lerp (currentPosition, targetPosition, smoothFollowSpeed * Time.deltaTime);
			else finalPosition = targetPosition;

			transform.position = finalPosition;
		}
	}

	void RotateObject ()
	{
		if (!smoothRotate) transform.rotation = targetRotation;
		else transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, smoothRotateSpeed * Time.deltaTime);
	}
}
