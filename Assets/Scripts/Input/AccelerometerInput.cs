using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerometerInput : MonoBehaviour 
{
	[HideInInspector] public bool physicsMode;		// Set to true when attatched to an object with a PhysicsController. Otherwise, is set to false.
	[HideInInspector] public bool globalMode;		// Set to true when attatched to GameController. Otherwise, is set to false.
	public bool rawInput;							// Removes the lerp and inputSmoothing, just applies rawAccelVector or raw2DAccelVector.
	public bool use2DInput;							// Removes the Z component from the accelVector to give a 2D vector. Useful for sidescrolling and 2D gravity rotation. 
	public bool normalizeInput;						// Normalizes accelVector if true.
	public bool reverseInput;						// Sets accelerometer vector to inverse of itself if true.
	public bool joystickMode;						// Sets accelerometer vector to represent a faux joystick input when device is held upright. (If 'RotateWithInput' is used, object matches device orientation if upright).
	public bool setDefaultSmoothAmount;				// Locks the inputSmoothing to the const float 'defaultSmoothAmount' below.
	[Range(0, 100f)]
	public float inputSmoothing;					// How quickly the input vector on external script lerps to local input vector. Lower is smoother but less responsive, higher is more responsive but can be jittery. 
	public Vector3 rawAccelVector;					// The current accelerometer reading.
	public Vector3 raw2DAccelVector;				// The current accelerometer reading without z.

	private const float DEFAULTSMOOTHAMOUNT = 30f;

	private GameController localGameController;
	private PhysicsController localPhysics;


	void OnEnable () 
	{
		localPhysics = GetComponent<PhysicsController> ();

		if (localPhysics == null) 
		{
			localGameController = GetComponent<GameController> ();

			if (localGameController == null) 
			{
				Debug.LogWarning ("No local physics or GameController script found. This script will now just display input data.");
			} 

			else 
			{
				globalMode = true;
			}

		} 

		else 
		{
			physicsMode = true;
		}

		if (physicsMode || globalMode) 
		{
			SetInputVector ();
		}
	}


	void Update () 
	{
		SetDefaultTiltSens ();
		ReadAccelerometer ();
		if (physicsMode || globalMode) SetInputVector ();
	}

	void OnDisable()
	{
		if (physicsMode || globalMode) DisableInputVector ();
	}


	void SetDefaultTiltSens()
	{
		if (setDefaultSmoothAmount) inputSmoothing = DEFAULTSMOOTHAMOUNT;
	}

	void ReadAccelerometer()
	{
		// Reads the raw accelerometer data.
		rawAccelVector = Input.acceleration;

		if (reverseInput) rawAccelVector = -rawAccelVector;

		if (joystickMode) rawAccelVector = new Vector3 (rawAccelVector.x, -rawAccelVector.y, -rawAccelVector.z);

		// Remove the z reading to create a 2D accelerometer vector.
		raw2DAccelVector = rawAccelVector;
		raw2DAccelVector.z = 0;

		// If normalizeInput is true, normalizes the accelerometer vectors.
		if (normalizeInput) 
		{
			rawAccelVector = rawAccelVector.normalized;

			if (use2DInput) raw2DAccelVector = raw2DAccelVector.normalized;
		}
	}

	void SetInputVector()
	{
		Vector3 localInputVector = rawAccelVector;

		if (use2DInput) localInputVector = raw2DAccelVector;

		if (physicsMode)		// If physicsMode is true...
		{ 
			// Tell local physics it has found local input.
			localPhysics.localInputFound = true;

			if (!localPhysics.useGlobalInput)		// If useGlobalInput is false...
			{
				// If rawInput is false, slerp the local physics' inputVector to localInputVector by inputSmoothing (and smoothDeltaTime for slerp smoothing).
				if (!rawInput) localPhysics.inputVector = Vector3.Slerp (localPhysics.inputVector, localInputVector, inputSmoothing * Time.smoothDeltaTime);

				// Else, directly set the local physics script inputVector to localInputVector.
				else localPhysics.inputVector = localInputVector;
			}
		}

		if (globalMode) 		// If globalMode is true...
		{
			// Tell local GameController that input has been found.
			localGameController.inputFound = true;

			// If rawInput is false, slerp the local game controller's globalInputVector to a normalized localInputVector by inputSmoothing (and smoothDeltaTime for slerp smoothing).
			if (!rawInput) localGameController.globalInputVector = Vector3.Slerp (localGameController.globalInputVector, localInputVector, inputSmoothing * Time.smoothDeltaTime);

			// Else, directly set the local game controller script's globalInputVector to localInputVector.
			else localGameController.globalInputVector = localInputVector;
		}
	}

	void DisableInputVector()
	{
		if (physicsMode) localPhysics.localInputFound = false;
		if (globalMode) localGameController.inputFound = false;
	}
}
