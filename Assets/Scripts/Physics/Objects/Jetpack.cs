using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : MonoBehaviour 
{
	public bool localUpAsDirection;
	public bool gravityUpAsDirection;
	public bool physicsInputAsDirection;
	public bool normalizePhysicsInput;
	public bool ignoreMass;
	[Range (-500, 500)]
	public float jetStrength = 30;
	public Vector3 jetDirection;

	private PhysicsController localPhysics;


	void Start () 
	{
		localPhysics = GetComponent<PhysicsController> ();
		if (localPhysics == null) localPhysics = GetComponentInParent<PhysicsController> ();
	}

	void Update()
	{
		if (localPhysics == null) Debug.LogError ("No physics script found on local or parent object. Add one to use the Jetpack.");
		else
		{
			DirectionManager ();
		}
	}

	void FixedUpdate () 
	{
		if (Input.GetMouseButton (0)) 
		{
			ActivateJetpack ();
		}
	}


	void DirectionManager()
	{
		if (localUpAsDirection) 
		{
			gravityUpAsDirection = false;
			physicsInputAsDirection = false;
			jetDirection = transform.up;
		}

		if (gravityUpAsDirection) 
		{
			localUpAsDirection = false;
			physicsInputAsDirection = false;
			jetDirection = -localPhysics.localGravDirection;
		}

		if (physicsInputAsDirection)
		{
			localUpAsDirection = false;
			gravityUpAsDirection = false;
			jetDirection = localPhysics.inputVector;
			if (normalizePhysicsInput) jetDirection = jetDirection.normalized;
		}
	}

	void ActivateJetpack()
	{
		if (localPhysics.arcadePhysics) 
		{
			localPhysics.AddArcadeForce(jetDirection * jetStrength);
		}

		if (!localPhysics.arcadePhysics)
		{ 
			if (ignoreMass) localPhysics.localRB.AddForce (jetDirection * jetStrength * localPhysics.localRB.mass);
			else localPhysics.localRB.AddForce (jetDirection * jetStrength);
		}
	}
}
