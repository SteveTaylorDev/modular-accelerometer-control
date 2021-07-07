using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsController))]
public class AccelerationWithInput : MonoBehaviour 
{
	public bool enableAcceleration;
	public bool localTransformInput;
	public bool ignoreInput;
	public bool localKeepLastInput;							// Use this if you want to keep the last input even when the current input script has KeepLastInput disabled.
	public bool normalizeInput;
	public bool fauxNormalize;

	[Range(0, 200)]
	public float moveStrength = 12;
	public bool limitMaxAccelVelocity;
	public float maximumAccelVelocity;

	public Vector3 lastInputVector;
	public Vector3 inputVector;
	private Vector3 accelVector;

	private PhysicsController localPhysics;


	void Start () 
	{
		localPhysics = GetComponent<PhysicsController> ();

		inputVector = Vector3.zero;
		lastInputVector = Vector3.zero;
	}

	void Update () 
	{
		ReadInput ();
		MakeMoveVector ();
	}

	void FixedUpdate()
	{
		if (enableAcceleration && (!localPhysics.noInput || localKeepLastInput)) ApplyAcceleration ();
	}


	void ReadInput()
	{
		if (!ignoreInput) 
		{
			inputVector = localPhysics.inputVector;
			NormalizeInput ();
			if (localTransformInput) TransformInput ();

			if(!localPhysics.noInput) lastInputVector = inputVector;
		}

		else
		{
			if (localKeepLastInput) SetLastInput ();
			else inputVector = Vector3.zero;
		}

		if (localPhysics.noInput && localKeepLastInput) SetLastInput ();
	}

	void MakeMoveVector()
	{
		accelVector = inputVector * moveStrength;
	}


	void NormalizeInput()
	{
		if (normalizeInput)
		{
			if (!fauxNormalize) inputVector = inputVector.normalized;
		}

		if (fauxNormalize) 
		{
			normalizeInput = true;
			if (inputVector.magnitude >= 1) inputVector = inputVector.normalized * 1;
		}
	}

	void TransformInput()
	{
		inputVector = transform.TransformDirection (inputVector);
	}

	void SetLastInput()
	{
		inputVector = lastInputVector;
	}

	void ApplyAcceleration()
	{
		if (localPhysics.arcadePhysics)
		{
			if (limitMaxAccelVelocity) 
			{
				Vector3 deltaAccelVector = accelVector * Time.deltaTime;
				Vector3 localMoveVector = localPhysics.arcadeMoveVector;

				if ((localMoveVector + deltaAccelVector).magnitude <= maximumAccelVelocity || (localMoveVector + deltaAccelVector).magnitude <= localPhysics.arcadeMoveVector.magnitude)
				{
					localPhysics.AddArcadeForce (accelVector);
				}	
			} 

			else 
			{
				localPhysics.AddArcadeForce(accelVector);
			}
		} 
			
		else 
		{ 
			if (limitMaxAccelVelocity) 
			{
				Vector3 deltaAccelVector = accelVector * Time.deltaTime;
				Vector3 localMoveVector = localPhysics.localRB.velocity;
			
				if ((localMoveVector + deltaAccelVector).magnitude <= maximumAccelVelocity)
				{
					localPhysics.localRB.AddForce (accelVector);
					Debug.DrawRay (transform.position, -accelVector * 0.1f, Color.cyan);
				}	
			}
			else
			{
				localPhysics.localRB.AddForce (accelVector);
				Debug.DrawRay (transform.position, -accelVector * 0.1f, new Color (0, 128, 255));
			}
		}

		accelVector = Vector3.zero;
	}
}
