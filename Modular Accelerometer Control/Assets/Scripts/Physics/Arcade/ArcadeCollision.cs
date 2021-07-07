using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PhysicsController), typeof(CollisionDetection))]
public class ArcadeCollision : MonoBehaviour 
{
	public bool enableVelocityRedirect;
	public bool redirectOnPush;
	public bool isRedirecting;
	public bool enablePushbackForce;
	public bool pushbackOnPush;
	public bool pushbackOnCollide;
	public bool isPushingBack;
	public bool bounceOnCollide;

	[Range(1, 50)]
	public float pushbackFactor = 10f;

	private PhysicsController localPhysics;
	private CollisionDetection localCollision;


	void Start ()
	{
		localPhysics = GetComponent<PhysicsController> ();
		localCollision = GetComponent<CollisionDetection> ();
	}

	void Update () 
	{
		if (localCollision.isColliding && localCollision.isTerrain) 
		{
			if (enablePushbackForce) PushbackForce ();
			if (enableVelocityRedirect) RedirectVelocity ();
		}
	}

	void OnCollisionEnter(Collision other)
	{
		Bounce (other);
		if (pushbackOnCollide) isPushingBack = true;
		if (enablePushbackForce) PushbackForce ();
	}


	void Bounce(Collision other)
	{
		if (bounceOnCollide) 
		{
			Vector3 bounceVector = Vector3.Reflect (localPhysics.arcadeMoveDir, other.contacts [0].normal);
			if ((localPhysics.localRB.constraints & RigidbodyConstraints.FreezePositionZ) == RigidbodyConstraints.FreezePositionZ) bounceVector = new Vector3 (bounceVector.x, bounceVector.y, 0);
			localPhysics.SetArcadeVector (bounceVector.normalized * localPhysics.arcadeMoveVector.magnitude);
		}
	}
		
	void PushbackForce()
	{
		if (localCollision.currentCollision != null) 
		{
			float calcForce = 1 + Vector3.Dot (localPhysics.arcadeMoveDir, localCollision.currentCollision.contacts [0].normal);

			calcForce *= pushbackFactor;

			if (Mathf.Abs(calcForce) >= 1) calcForce = 1 * Mathf.Sign(calcForce);

			if (pushbackOnPush && localCollision.isPushing) isPushingBack = true;
			if (pushbackOnPush && !localCollision.isPushing) isPushingBack = false;

			if(isPushingBack) localPhysics.SetArcadeVector (localPhysics.arcadeMoveVector * calcForce);

			isPushingBack = false;
		}
	}

	void RedirectVelocity()
	{
		Vector3 redirVector = Vector3.ProjectOnPlane (localPhysics.arcadeMoveDir, localCollision.currentCollision.contacts [0].normal);

		if ((redirectOnPush && localCollision.isPushing) || !redirectOnPush) isRedirecting = true;
		if (redirectOnPush && !localCollision.isPushing) isRedirecting = false;

		if ((localPhysics.localRB.constraints & RigidbodyConstraints.FreezePositionZ) == RigidbodyConstraints.FreezePositionZ) redirVector = new Vector3 (redirVector.x, redirVector.y, 0);
		if (isRedirecting) localPhysics.SetArcadeDirection (redirVector.normalized);
	}
}
