using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallmanBehaviour : MonoBehaviour 
{
	
 // Collision
	public bool redirectVelocity;
	public bool onlyRedirIfPushing;
	public bool pushbackForce;
	public bool pushbackIfPushing;
	public float pushbackFactor;

 // Friction
	public bool frictionWithGround;
	public bool frictionWithCollision;
	public bool onlyPushFriction;
	public float frictionAmount;
	public float frictionFactor;

 // State Bools
	public bool isRedirecting;
	public bool isPushingBack;

 // Vectors
	public Vector3 localMoveVector;

 // Components
	private PhysicsController localPhysics;
	private CollisionDetection localCollision;


	void Start () 
	{
		localPhysics = GetComponent<PhysicsController> ();
		localCollision = GetComponent<CollisionDetection> ();
	}

	void Update () 
	{
		localMoveVector = localPhysics.arcadeMoveVector;
		//if(!localPhysics.noInput && !localCollision.isColliding) localMoveVector = Vector3.Lerp (localMoveVector, localPhysics.inputVector.normalized * localMoveVector.magnitude, 20 * Time.deltaTime);

		if (localCollision.isColliding && localCollision.isTerrain) 
		{
			PushbackForce ();
			RedirectVelocity ();
		}
		if((frictionWithGround && localPhysics.isGrounded) || (frictionWithCollision && localCollision.isColliding)) Friction ();
	}

	void FixedUpdate()
	{
		SetArcadeVector ();
	}


	void PushbackForce()
	{
		float calcForce = 1 + Vector3.Dot (localMoveVector.normalized, localCollision.currentCollision.contacts [0].normal);

		calcForce *= pushbackFactor;

		if (Mathf.Abs(calcForce) >= 1) calcForce = 1 * Mathf.Sign(calcForce);
		if (pushbackIfPushing && localCollision.isPushing || !pushbackIfPushing) isPushingBack = true;
		if (isPushingBack) 
		{
			localMoveVector = localMoveVector * calcForce;
		}

		isPushingBack = false;
	}

	void RedirectVelocity()
	{
		if ((onlyRedirIfPushing && localCollision.isPushing) || !onlyRedirIfPushing) isRedirecting = true;
		if (onlyRedirIfPushing && !localCollision.isPushing) isRedirecting = false;

		if (isRedirecting) 
		{
			Vector3 redirVector = Vector3.ProjectOnPlane (localMoveVector.normalized, localCollision.currentCollision.contacts [0].normal);
			if ((localPhysics.localRB.constraints & RigidbodyConstraints.FreezePositionZ) == RigidbodyConstraints.FreezePositionZ) redirVector = new Vector3 (redirVector.x, redirVector.y, 0);

			localMoveVector = redirVector.normalized * localMoveVector.magnitude;
		}
	}

	void Friction ()
	{
		bool localPushing = false;
		float localMinPushAngle = 90;

		if (localCollision.moveCollisionAngle >= localMinPushAngle) localPushing = true;

		if ((onlyPushFriction && localPushing) || !onlyPushFriction) 
		{
			float inputAngle = Vector3.Angle (-localPhysics.inputVector, localCollision.currentCollision.contacts [0].normal);
			Debug.Log (inputAngle);

			Vector3 frictionVector = -localMoveVector.normalized * frictionFactor * frictionAmount;

			if (localMoveVector.magnitude > (frictionVector.magnitude * Time.deltaTime)) 
			{
				Debug.DrawRay (transform.position, -frictionVector * 0.1f, Color.green);
				localMoveVector += frictionVector * Time.deltaTime;
			}

			else 
			{
				localMoveVector = Vector3.zero;
			}
		}
	}

	void SetArcadeVector()
	{
		localPhysics.SetArcadeVector (localMoveVector);
	}
}
