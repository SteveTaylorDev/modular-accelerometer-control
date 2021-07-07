using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PhysicsController))]
public class AdaptiveCollision : MonoBehaviour 
{
	public bool enableAdaptiveCollision = true;								// If true, rigidbody CollisionDetectionMode is set depending on current velocity.
	public bool adaptWithTimestep = true;									// If true, also adjusts the collision mode based on the current physics timestep.
	[Range (0, 1000)]
	public float continuousMinVel = 70;										// Minimum velocity before continuous collision mode is set (If adaptive collision mode is enabled).
	[Range (0, 1000)]
	public float contDynamicMinVel = 250;									// Minimum velocity before continuous dynamic collision mode is set (If adaptive collision mode is enabled).

	[Range (0, 1)]
	public float continuousMinTimestep = 0.05f;								// Minimum timestep before continuous collision mode is set (If adaptive collision mode is enabled).
	[Range (0, 1)]
	public float contDynamicMinTimestep = 0.1f;								// Minimum timestep before continuous dynamic collision mode is set (If adaptive collision is enabled).

	private PhysicsController localPhysics;


	void Start () 
	{
		localPhysics = GetComponent<PhysicsController> ();
	}

	void Update () 
	{
		if (enableAdaptiveCollision) AdaptCollisionMode ();
	}


	void AdaptCollisionMode()
	{
		float timestep = Time.fixedDeltaTime;

		if (!adaptWithTimestep) timestep = 0f;

		if (localPhysics.currentVelocity < continuousMinVel && timestep < continuousMinTimestep) 
		{
			localPhysics.localRB.collisionDetectionMode = CollisionDetectionMode.Discrete;
			return;
		}

		if ((localPhysics.currentVelocity >= continuousMinVel && localPhysics.currentVelocity < contDynamicMinVel) || timestep >= continuousMinTimestep)
		{
			localPhysics.localRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
			return;
		}

		if ((localPhysics.currentVelocity >= contDynamicMinVel) || timestep >= contDynamicMinTimestep)
		{
			localPhysics.localRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			return;
		}
	}
}
