using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (CameraController))]
public class SpeedZoom : MonoBehaviour
{
	public bool allowSpeedZoom;
	public bool smoothZoom;
	[Range(0, 100)]
	public float smoothZoomSpeed = 10f;								// The time it takes current zoom amount to match new zoom amount. Lerped camera adjustments use this as t (mostly by deltaTime)

	[Range(0, 200)]
	public float defaultRectSize = 10f;								// Camera rect size by default.
	[Range(0, 200)]
	public float maxRectSize = 30f;									// Maximum allowed camera rect size.
	[Range(0, 10)]
	public float speedZoomFactor = 0.4f;							// Factor the current speed is multiplied by to determine rect size. (For speed based camera zoom).

	private CameraController localCameraController;					// The main CameraController script on this object.
	private Camera localCamera;
	private PhysicsController targetPhysics;						// The 'PhysicsController' script attached to the parent of cameraTarget (Pulled from 'CameraController' script).


	void Start ()
	{
		localCameraController = GetComponent<CameraController> ();
		localCamera = localCameraController.localCamera;
	}

	void Update()
	{
		if (localCameraController == null) Debug.LogError ("No CameraController component found on object. Add one to use SpeedZoom.");

		// Pull targetPhysics from the CameraController script.
		if (targetPhysics == null) targetPhysics = localCameraController.targetPhysics;

		if (localCameraController.targetPhysics != null)
		{
			// Controls the camera rect size based on the current speed (Based on camera target parent 'PhysicsController').
			SpeedModeManager ();
		} 

		else 
		{
			localCamera.orthographicSize = defaultRectSize;
		}
	}

	void OnDisable()
	{
		localCamera.orthographicSize = defaultRectSize;
	}


	void SpeedModeManager()
	{
		if (allowSpeedZoom)
		{
			float speedRectSize = Mathf.Abs(targetPhysics.currentVelocity) * speedZoomFactor;						// Multiplies absolute currentGravStrength by the speedZoomFactor to calculate the rect size.
			if (speedRectSize >= maxRectSize) speedRectSize = maxRectSize;											// Limits calculated rect size to maxRectSize.

			if (speedRectSize > defaultRectSize) 																	// If the calculated rect size is greater than the default rect size...
			{
				// ...lerp the local camera's orthagraphicSize to the calculated speedRectSize by lerpSpeed.
				if (smoothZoom) localCamera.orthographicSize = Mathf.Lerp (localCamera.orthographicSize, speedRectSize, smoothZoomSpeed * Time.deltaTime);
				else localCamera.orthographicSize = speedRectSize;
			}

			if (speedRectSize <= defaultRectSize) 																	// If the calculated rect size is less than the default rect size...
			{ 
				// ...lerp the local camera's orthagraphicSize from the current size to the default rect size by lerpSpeed.
				if (smoothZoom) localCamera.orthographicSize = Mathf.Lerp (localCamera.orthographicSize, defaultRectSize, smoothZoomSpeed * Time.deltaTime);
				else localCamera.orthographicSize = defaultRectSize;
			}
		} 

		else 		// If allowSpeedZoomMode is false, lerp localCamera.orthagraphicSize to defaultRectSize.
		{
			localCamera.orthographicSize = Mathf.Lerp (localCamera.orthographicSize, defaultRectSize, smoothZoomSpeed * Time.deltaTime);
		}
	}
}
