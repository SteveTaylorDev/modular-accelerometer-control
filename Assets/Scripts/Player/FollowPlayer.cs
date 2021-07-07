using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour 
{
	[HideInInspector] public PhysicsController playerPhysics;
	public Transform playerTransform;

	public bool followPlayer;
	public bool rotateWithPlayer;

	private Vector3 targetPosition;


	void Start () 
	{
		playerTransform = GameObject.FindWithTag ("Player").transform;
		playerPhysics = playerTransform.GetComponent <PhysicsController>();
	}

	void Update () 
	{
		if (followPlayer) EnableFollow ();
		if (rotateWithPlayer) RotateWithPlayer ();
	}


	void EnableFollow()
	{
		// Sets targetPosition to the playerTransform position.
		targetPosition = playerTransform.position;

		transform.position = targetPosition;
	}

	void RotateWithPlayer ()
	{
		transform.rotation = playerTransform.rotation;
	}
}
