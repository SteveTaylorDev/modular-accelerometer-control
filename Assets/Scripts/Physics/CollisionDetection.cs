using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour 
{
	public bool isColliding;
	[Range (0, 360)]
	public float minPushAngle = 92;
	public bool isPushing;
	public float localCollisionAngle;
	public float globalCollisionAngle;
	public float moveCollisionAngle;
	public bool isTerrain;

	public Collision currentCollision;

	private GameController gameController;
	private PhysicsController localPhysics;
	private GroundDetection localGroundDetect;


	void OnEnable () 
	{
		gameController = GameObject.FindWithTag ("GameController").GetComponent<GameController> ();
		localPhysics = GetComponent<PhysicsController> ();

		isColliding = false;
	}

	void FixedUpdate ()
	{
		if (localPhysics == null) 
		{
			Debug.LogError ("No local PhysicsController script found. Add one to use CollisionDetection.");
		}

		else 
		{
			GetCollisionAngle ();
			GetPushAngle ();

			UpdatePhysicsScript ();
		}
	}

	void OnCollisionEnter (Collision other)
	{
		if (other.gameObject.layer != 8) isColliding = true;
		if (other.gameObject.layer == 9) isTerrain = true;
		else isTerrain = false;

		currentCollision = other;

		GetPushAngle ();
	}

	void OnCollisionStay (Collision other)
	{
		if (other.gameObject.layer != 8) isColliding = true;
		if (other.gameObject.layer == 9) isTerrain = true;
		else isTerrain = false;

		currentCollision = other;
		GetPushAngle ();
	}

	void OnCollisionExit (Collision other)
	{
		isColliding = false;
		isTerrain = false;
		currentCollision = null;
	}

	void OnDisable()
	{
		if (localPhysics != null) localPhysics.isColliding = false;
	}
		

	void GetCollisionAngle()
	{
		if (isColliding) 
		{
			localCollisionAngle = Vector3.Angle (-localPhysics.localGravDirection, currentCollision.contacts [0].normal);
			globalCollisionAngle = Vector3.Angle (-gameController.globalGravDirection, currentCollision.contacts [0].normal);
		} 

		else 
		{
			localCollisionAngle = 0;
			globalCollisionAngle = 0;
		}
	}

	void GetPushAngle()
	{
		if (isColliding) 
		{
			moveCollisionAngle = Vector3.Angle (localPhysics.arcadeMoveVector.normalized, currentCollision.contacts [0].normal);

			if (moveCollisionAngle >= minPushAngle) 
			{
				isPushing = true;
			}

			if (moveCollisionAngle < minPushAngle) 
			{
				isPushing = false;
			}
		}

		else 
		{
			isPushing = false;
		}
	}

	void UpdatePhysicsScript()
	{
		localPhysics.isColliding = isColliding;
	}
}
