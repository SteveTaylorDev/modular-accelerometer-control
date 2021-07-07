using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PhysicsController))]
public class ArcadeFriction : MonoBehaviour
{
	public bool enableFiction;
	public bool applyFriction;
	public float frictionAmount;
	[Range(-100, 100)]
	public float frictionFactor;
	public bool withGround;
	public bool withCollision;
	public bool collisionWhenPushing;
	public bool disableWithInput;

	private PhysicsController localPhysics;
	private CollisionDetection localCollision;
	private GroundDetection localGroundDetect;


	void OnEnable () 
	{
		localPhysics = GetComponent<PhysicsController> ();
	}

	void Update()
	{
		if (withCollision) 
		{
			if(localCollision == null) localCollision = GetComponent<CollisionDetection> ();
			if (localCollision == null) Debug.LogError ("No CollisionDetection script found. Please attach one to use friction with collision.");

			if (localCollision.isColliding && localCollision.isTerrain) 
			{
				applyFriction = true;
				GetCollisionPhysicMaterial ();
			}
			if (!localCollision.isColliding || !localCollision.isTerrain) 
			{
				applyFriction = false;
			}
		}

		if (withGround)
		{
			if(localGroundDetect == null) localGroundDetect = GetComponent<GroundDetection> ();
			if (localGroundDetect == null) Debug.LogError ("No GroundDetection script found. Please attach one to use friction with ground detection.");

			if (localGroundDetect.isGrounded && localGroundDetect.isTerrain)
			{
				applyFriction = true;
				GetGroundPhysicMaterial ();
			}

			if (!localGroundDetect.isGrounded || !localGroundDetect.isTerrain) 
			{
				if (!withCollision) 
				{
					applyFriction = false;
				} 
				else 
				{
					if (!localCollision.isColliding || !localCollision.isTerrain) applyFriction = false;
				}
			}
		}

		if ((!withGround && !withCollision) || (collisionWhenPushing && !localCollision.isPushing && (!withGround || withGround && !localPhysics.isGrounded))) applyFriction = false;
	}

	void GetCollisionPhysicMaterial()
	{
		frictionAmount = localCollision.currentCollision.collider.material.dynamicFriction;
	}

	void GetGroundPhysicMaterial()
	{
		frictionAmount = localGroundDetect.groundRayHit.collider.material.dynamicFriction;
	}

	void FixedUpdate () 
	{
		if (disableWithInput && !localPhysics.noInput) applyFriction = false;
		if (enableFiction && applyFriction) ApplyFriction ();
	}

	void ApplyFriction()
	{
		Vector3 frictionVector = -localPhysics.arcadeMoveVector.normalized * frictionFactor * frictionAmount;

		if (localPhysics.arcadeMoveVector.magnitude > (frictionVector.magnitude * Time.deltaTime)) 
		{
			Debug.DrawRay (transform.position, -frictionVector * 0.1f, Color.green);

			localPhysics.AddArcadeForce (frictionVector);
		} 

		else 
		{
			localPhysics.SetArcadeStrength(0);
		}
	}
}
