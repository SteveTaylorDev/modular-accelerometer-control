using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour 
{
	public bool noInput;
	public float globalGravStrength = 10;						// Global gravity strength. This can be treated as the "planet" gravity; is fed to the PhysicsController so it can be altered locally (for local gravity effects).

	[HideInInspector] public bool inputFound;
	public Vector3 globalInputVector;
	public float inputAngle;
	public Vector3 globalGravDirection = Vector3.down;

	private GameObject playerObject;
	private GameObject spawnpoint;


	void Start () 
	{
		playerObject = GameObject.FindWithTag("Player");
		spawnpoint = GameObject.FindWithTag ("Spawnpoint");

		if (playerObject == null) Debug.LogError ("No object tagged 'Player' found.");
		if (spawnpoint == null && playerObject != null)	Debug.LogWarning ("No object tagged 'Spawnpoint' found. Placing player at world origin.");
		if (spawnpoint == null && playerObject == null) Debug.LogWarning ("No object tagged 'Spawnpoint' found.");

		// Sets playerObject to the spawnpoint position if both player and spawnpoint exist.
		if(playerObject != null && spawnpoint != null) 
		{
			playerObject.GetComponent<PhysicsController> ().targetRotation = Quaternion.LookRotation(spawnpoint.transform.forward, transform.up);
			playerObject.transform.rotation = Quaternion.LookRotation(spawnpoint.transform.forward, transform.up);
			playerObject.transform.position = spawnpoint.transform.position;
		}	
	}

	void Update()
	{
		InputManager ();
		NormalizeGravityDirection ();
	}


	void NormalizeGravityDirection()
	{
		globalGravDirection = globalGravDirection.normalized;
	}

	void InputManager()
	{
		if (!inputFound) 
		{
			Debug.LogWarning ("No input script found on GameController. Setting global input to zero.");
			globalInputVector = Vector3.zero;
			noInput = true;
		}
	}
}
