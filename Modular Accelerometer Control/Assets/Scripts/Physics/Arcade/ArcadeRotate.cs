using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PhysicsController))]
public class ArcadeRotate : MonoBehaviour 
{
	public bool forwardWithInput;
	public bool forwardWithMoveDir;
	public bool ignoreVertMove;
	public Vector3 targetForward;

	private PhysicsController localPhysics;


	void Start () 
	{
		localPhysics = GetComponent<PhysicsController> ();
	}

	void Update () 
	{
		CalcRotation ();
	}


	void CalcRotation()
	{
		if(forwardWithInput) targetForward = localPhysics.inputVector;
		if (forwardWithMoveDir)
		{
			if (forwardWithInput) forwardWithInput = false;
			targetForward = localPhysics.arcadeMoveDir;
			if (ignoreVertMove) targetForward.y = 0;
		}

		if(!localPhysics.noInput) localPhysics.targetRotation = Quaternion.LookRotation (targetForward, transform.up);
	}
}
