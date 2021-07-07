using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisInput : MonoBehaviour 
{
	[HideInInspector] public bool physicsMode;		// Set to true when attatched to an object with a PhysicsController. Otherwise, is set to false.
	[HideInInspector] public bool globalMode;		// Set to true when attatched to GameController. Otherwise, is set to false.
	public bool rawInput;							// Removes the lerp and tiltSensitivity, just applies rawInputVector or raw2DInputVector.
	public bool use2DInput;							// Removes the Z component from the accelVector to give a 2D vector. Useful for sidescrolling and 2D rotation. 
	public bool incrementInput;
	public bool normalizeInput;						// Normalizes accelVector if true.
	public bool keepLastInput;						// Remembers the last input and sets current input to that if no input is detected.
	public bool reverseInput;						// Sets input vector to inverse of itself if true.
	public bool setDefaultSmoothAmount;				// Locks the inputSmoothing to the const float 'DEFAULTSMOOTHAMOUNT' below.
	public bool setDefaultIncrementSens;			// Locks the incrementSens to the const float 'DEFAULTINCREMENTSENS' below.
	[Range(0, 100f)]
	public float inputSmoothing;					// How quickly the inputVector on the external script is lerped to match the local inputVector. Lower is smoother but less responsive, higher is more responsive but can be jittery. 
	[Range(0, 1000f)]
	public float incrementSens;
	public Vector3 rawInputVector;					// The current input reading.
	public Vector3 raw2DInputVector;				// The current input reading without z.
	public Vector3 finalInputVector;				// The final input vector sent out by the script.
	public bool noInput;

	private const float DEFAULTSMOOTHAMOUNT = 30f;
	private const float DEFAULTINCREMENTSENS = 230f;

	private GameController localGameController;
	private PhysicsController localPhysics;
	private Vector3 lastInputVector;
	private Vector3 last2DInputVector;

	public float incrementAngle;


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
		SetDefaults ();
		ReadInput ();
		if (physicsMode || globalMode) SetInputVector ();
	}

	void OnDisable()
	{
		if (physicsMode || globalMode) DisableInputVector ();
	}


	void SetDefaults()
	{
		if (setDefaultSmoothAmount) inputSmoothing = DEFAULTSMOOTHAMOUNT;
		if (setDefaultIncrementSens) incrementSens = DEFAULTINCREMENTSENS;
	}

	void ReadInput()
	{
		// Reads the raw input data.
		rawInputVector = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		// If the absolute input vector magnitude is less than or equal to 0, set noInput to true.
		if (Mathf.Abs (rawInputVector.magnitude) <= 0) 
		{
			if (!keepLastInput) noInput = true;
		} 

		// Else, noInput is false.
		else 
		{
			noInput = false;
			lastInputVector = rawInputVector;
			last2DInputVector = raw2DInputVector;
		}

		if (globalMode) localGameController.noInput = noInput;
		else localPhysics.noInput = noInput;

		// Sets raw2DInputVector using horizontal axis as x, and vertical axis as y instead of z.
		if (use2DInput && !incrementInput) raw2DInputVector = new Vector3 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

		if (incrementInput) 
		{ 
			keepLastInput = false;

			incrementAngle += Input.GetAxisRaw ("Horizontal") * incrementSens * Time.deltaTime;
			if (Mathf.Abs (incrementAngle) >= 360) ResetIncrementAngle ();

			if (use2DInput) raw2DInputVector = Quaternion.Euler (0, 0, incrementAngle) * Vector3.down;
			else rawInputVector = Quaternion.Euler (0, incrementAngle, 0) * Vector3.forward;
		}

		else
		{
			ResetIncrementAngle ();
		}

		// If reverseInput is true, sets the input vectors to inverse of themsleves.
		if (reverseInput)
		{
			rawInputVector = -rawInputVector;
			if (use2DInput) raw2DInputVector = -raw2DInputVector;
		}

		// If normalizeInput is true, normalizes the input vectors.
		if (normalizeInput) 
		{
			rawInputVector = rawInputVector.normalized;
			if (use2DInput) raw2DInputVector = raw2DInputVector.normalized;
		}

		if (keepLastInput) 
		{
			rawInputVector = lastInputVector;
			raw2DInputVector = last2DInputVector;
		}
	}

	void SetInputVector()
	{
		finalInputVector = rawInputVector;

		if (use2DInput) finalInputVector = raw2DInputVector;

		if (physicsMode)		// If physicsMode is true...
		{ 
			// Tell local physics it has found local input.
			localPhysics.localInputFound = true;

			// Find angle of inputVector compared to Vector3.up. Multiply by the sign of the x reading of input vector (to get -180 to 180).
			localPhysics.inputAngle = FindInputAngle();

			if (!localPhysics.useGlobalInput)		// If useGlobalInput is false...
			{
				// If rawInput is false, slerp the local physics' inputVector to localInputVector by inputSmoothing (and smoothDeltaTime for slerp smoothing).
				if (!rawInput) localPhysics.inputVector = Vector3.Slerp (localPhysics.inputVector, finalInputVector, inputSmoothing * Time.smoothDeltaTime);

				// Else, directly set the local physics script inputVector to localInputVector.
				else localPhysics.inputVector = finalInputVector;
			}
		}

		if (globalMode) 		// If globalMode is true...
		{
			// Tell local GameController that input has been found.
			localGameController.inputFound = true;

			// Find angle of inputVector compared to Vector3.up. Multiply by the sign of the x reading of input vector (to get -180 to 180).
			localGameController.inputAngle = FindInputAngle();

			// If rawInput is false, slerp the local game controller's globalInputVector to a normalized localInputVector by inputSmoothing (and smoothDeltaTime for slerp smoothing).
			if (!rawInput) localGameController.globalInputVector = Vector3.Slerp (localGameController.globalInputVector, finalInputVector, inputSmoothing * Time.smoothDeltaTime);

			// Else, directly set the local game controller script's globalInputVector to localInputVector.
			else localGameController.globalInputVector = finalInputVector;
		}
	}

	float FindInputAngle()
	{
		Vector3 localZeroDir = Vector3.forward;
		if (use2DInput) localZeroDir = Vector3.up;

		return (Vector3.Angle (localZeroDir, finalInputVector) * Mathf.Sign (finalInputVector.x));
	}

	void DisableInputVector()
	{
		if (physicsMode) localPhysics.localInputFound = false;
		if (globalMode) localGameController.inputFound = false;
	}

	void ResetIncrementAngle()
	{
		incrementAngle = 0;
	}
}
