using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof (Rigidbody))]
public class PhysicsController : MonoBehaviour 
{
 // Hidden Variables 
	[HideInInspector] public Rigidbody localRB;
	[HideInInspector] public bool isGrounded;								// Hidden isGrounded bool used with external GroundDetection script. Directly tied to the GroundDetection isGrounded bool, set to false at start and on GroundDetection script disable.
	[HideInInspector] public bool isColliding;								// Hidden isColliding bool used with external CollisionDetection script. Directly tied to the CollisionDetection isColliding bool, set to false at start and on CollisionDetection script disable.
	[HideInInspector] public bool localInputFound;							// Is set to true by local input script. If this and useGlobalInput are false, inputVector is set to zero.
	[HideInInspector] public float velocityPercentage;						// The percentage of currentVelocity based on maxVelocity (100 if limitMaxVel is disabled).
	private GameController gameController;

 // General Physics
	public bool arcadePhysics;												// Uses the arcadeMoveVector as the velocity, sets the rigidbody velocity directly (Barely any unscripted external forces will affect object).
	public bool preservePhysicsVel;											// If true, sets rigidbody velocity to arcadeMoveVector when using rigidbody phyiscs. If false, preserves the arcadeMoveVector when using rigidbody physics.

 // Arcade
	public Vector3 arcadeMoveDir;
	public float arcadeMoveStrength;
	public Vector3 arcadeMoveVector;										// The calculated move vector in arcadePhysics mode. Gravity is calculated and applied (if enabled), alongside other forces.

 // Input
	public bool useGlobalInput;												// If true, the inputVector will be pulled from the global GameController script.
	public float inputAngle;												// This is found from the inputVector and the global 'GameController' gravityDirection.
	public Vector3 inputVector;												// Fed in by other input scripts (like AccelerometerInput). gravityDirection uses this when airInputGravity is true.
	public bool noInput;													// This is true when the input script's rawInputVector magnitude is 0 (or when no input is found).

 // Gravity
	public bool disableGravity;												// Stops all gravity forces.
	public bool reverseGravity;												// Set the localGravDirection to use the inverse of itself.	
	public Vector3 localGravDirection;										// The current downward gravity direction for this object. Either taken from inputVector (fed in externally) or the global gravityDirection.
	public bool normalizeGravDir = true;									// Normalizes localGravDirection. Only switch this off if you want to apply slight force with input amount (thruster in space, for example). Works best with rigidbody, non-arcade physics.
	public bool ignoreGlobalGravDir;										// If this is false, localGravDirection is set to globalGravDirection. If this is true, it ignores global gravity direction.
	public float localGravStrength;											// The amount that is multiplied by localGravDirection to make a local gravity vector for both direction and magnitude. (Set to gameController.globalGravStrength by default)
	public bool ignoreGlobalGravStrength;									// If this is false, localgravStrength is set to globalGravStrength. If this is true, it ignores global gravity strength.
	[Range(-10, 10)]
	public float localGravFactor = 1;										// Multiplied by localGravStrength to adjust the local strength without changing the global strength. Acts as object mass to gravity.
	public bool resetGravFactor;											// Sets gravity factor to 1, using raw gravity strength (which is set to GameController global gravity strength if IgnoreGlobalGravStrength is false).

 // Velocity
	public float currentVelocity;											// The current rigidbody velocity.
	public bool limitMaxVel;												// Limits the maximum velocity when calculating.
	public float maxVelocity = 40f;											// currentVelocity is limited to this amount.

 // Rotation
	public bool useTargetRotation;											// If true, sets current rotation to targetRotation, sent by an external script sends to this 'PhysicsController' script. If false, sets no rotation and external physics forces can affect rotation.
	public Quaternion targetRotation;										// The target quaternion the rigidbody is set to if useTargetRotation is true.
	public bool smoothTargetRotate;											// Slerps to the target rotation instead of setting it directly.
	[Range(0, 100)]
	public float smoothRotateSpeed = 30f;									// If smoothRotate is true, used as 't' in rotation quaternion lerping.


	void OnEnable () 
	{
		gameController = GameObject.FindWithTag ("GameController").GetComponent<GameController> ();
		localRB = GetComponent<Rigidbody> ();

		// If gravity is enabled, instantly set localRB useGravity to false. (As we're completely replacing unity's global gravity in this script).
		if (!disableGravity && localRB.useGravity)	localRB.useGravity = false;

		isGrounded = false;
		isColliding = false;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown (1) && gameObject.CompareTag("Player")) arcadePhysics = !arcadePhysics;
		if (Input.GetKeyDown(KeyCode.R) && gameObject.CompareTag ("Player")) SceneManager.LoadScene ("Test");

		PreservePhysicsVelocity ();

		InputVectorManager ();

		GravityDirectionManager ();
		GravStrengthManager ();

		if (useTargetRotation) TargetRotationManager ();

		MakeArcadeVector ();
		if (limitMaxVel) LimitMaxVelocity ();

		ArcadeMoveObject ();

		SetCurrentVelocity ();
		CalcVelocityPercentage ();						// Calculates grav velocity percentage based on current and max (100 if no max vel).
	}

	void FixedUpdate () 
	{					
		if (!disableGravity) 				// If disableGravity is false...
		{
			if (arcadePhysics) 				// ...and arcadePhysics is true...
			{ 		
				ArcadeGravForce ();			// Calculate global gravity strength on current velocity. 
			}

			if (!arcadePhysics) 			// If arcadePhysics is false...
			{
				RBGravForce ();				// Apply rigidbody gravity force.
			}
		}
	}
		

 // Public Functions

	public void AddArcadeForce(Vector3 forceVector)
	{
		arcadeMoveVector += forceVector * Time.deltaTime;

		arcadeMoveDir = arcadeMoveVector.normalized;
		arcadeMoveStrength = arcadeMoveVector.magnitude;

		MakeArcadeVector ();

		Debug.DrawRay (transform.position, -forceVector * 0.1f, Color.cyan);
	}

	public void SetArcadeDirection(Vector3 newDirection)
	{
		arcadeMoveDir = newDirection.normalized;

		MakeArcadeVector ();
	}

	public void SetArcadeStrength (float newStrength)
	{
		arcadeMoveStrength = newStrength;

		MakeArcadeVector ();
	}

	public void SetArcadeVector(Vector3 newMoveVector)
	{
		arcadeMoveVector = newMoveVector;

		arcadeMoveDir = arcadeMoveVector.normalized;
		arcadeMoveStrength = arcadeMoveVector.magnitude;

		MakeArcadeVector ();
	}

 // end of Public Functions 


	void PreservePhysicsVelocity()
	{
		if (!arcadePhysics && preservePhysicsVel) 
		{
			SetArcadeVector (localRB.velocity);
		}
	}

	void InputVectorManager()
	{
		// If localInputFound and useGlobalInput are false...
		if (!localInputFound && !useGlobalInput) 
		{
			// Set inputVector to Vector3.zero.
			Debug.LogWarning ("No local or global input found. Setting input to zero.");
			inputVector = Vector3.zero;
			inputAngle = 0;
			noInput = true;
		}

		// If localInputFound or useGlobalInput are true...
		if (localInputFound || useGlobalInput)
		{
			// If useGlobalInput is true...
			if (useGlobalInput) 
			{
				// If no input is found on GameController, log a warning.
				if (!gameController.inputFound) Debug.LogWarning ("No global input script found on GameController. Add global input script or set 'Use Global Input' to false to use local input.");

				// Set the local inputVector to the globalInputVector if it is found.
				else inputVector = gameController.globalInputVector; 

				inputAngle = gameController.inputAngle;
				noInput = gameController.noInput;
			}

			// Draw the local inputVector as a Debug.DrawRay.
			Debug.DrawRay (transform.position, inputVector, Color.yellow);
		}
	}
		
	void GravityDirectionManager()
	{
		// If ignoreGlobalGravDir is false, set localGravDirection to the gameController globalGravDirection.
		if (!ignoreGlobalGravDir) localGravDirection = gameController.globalGravDirection;

		// If reverse gravity is enabled, set the gravityDirection to be the inverse of itself.
		if (reverseGravity) localGravDirection = -localGravDirection;

		// If enabled, normalize the gravity direction.
		if (normalizeGravDir) localGravDirection = localGravDirection.normalized;

		Debug.DrawRay (transform.position, localGravDirection, Color.blue);
	}

	void GravStrengthManager()
	{
		// Set rigidbody useGravity to false (As we're completely replacing global gravity in this script).
		if (localRB.useGravity && !disableGravity)	localRB.useGravity = false;

		// If resetGravFactor is true, run ResetGravFactor function. Sets gravFactor to 1 and resetGravFactor bool to false.
		if (resetGravFactor) ResetGravFactor();

		if (!ignoreGlobalGravStrength) localGravStrength = gameController.globalGravStrength;

		// Multiply localGravStrength by localGravFactor.
		localGravStrength *= localGravFactor;
	}

	void ResetGravFactor()
	{
		localGravFactor = 1;
		resetGravFactor = false;
	}

	void MakeArcadeVector()
	{
		arcadeMoveVector = arcadeMoveDir.normalized * arcadeMoveStrength;
	}

	void LimitMaxVelocity()
	{
		// If the current rigidbody velocity magnitude is higher than the maximum velocity, set the velocity vector to the normalized velocity direction multiplied by maximum velocity.
		if (localRB.velocity.magnitude >= maxVelocity && !arcadePhysics) localRB.AddForce(-localRB.velocity.normalized * (localRB.velocity.magnitude - maxVelocity), ForceMode.Impulse);

		// If arcadePhysics is true, and arcadeMoveStrength (or arcadeMoveVector.magnitude) is greater than maxVelocity, set the arcadeMoveStrength and arcadeMoveVector.magnitude to the maximum velocity.
		if (arcadePhysics && (arcadeMoveStrength >= maxVelocity || arcadeMoveVector.magnitude >= maxVelocity)) 
		{
			arcadeMoveStrength = maxVelocity;
			arcadeMoveVector = arcadeMoveVector.normalized * maxVelocity;
		}
	}

	void SetCurrentVelocity()
	{
		// If arcadePhysics is false, set the currentVelocity float to the current rigidbody velocity magnitude.
		if (!arcadePhysics) currentVelocity = localRB.velocity.magnitude;

		// Else, set the currentVelocity to the arcadeMoveStrength.
		else currentVelocity = arcadeMoveStrength;
	}

	void CalcVelocityPercentage()
	{
		float localMaxVel = maxVelocity;

		// Calculate percentage using 100 as max if limitMaxVel is disabled.
		if (!limitMaxVel) localMaxVel = 100;

		// Calculate percentage based on currentVelocity and localMaxVel.
		velocityPercentage = currentVelocity / localMaxVel;
	}

	void TargetRotationManager()
	{
		// If smoothRotate is true, slerps from current rotation to targetRotation by smoothRotateSpeed and smoothDeltaTime.
		if (smoothTargetRotate) transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, smoothRotateSpeed * Time.smoothDeltaTime);

		// Else, sets the rotation directly to targetRotation.
		else transform.rotation = targetRotation;
	}
		
	void ArcadeGravForce()
	{
		if (!isGrounded)
		{ 
			// Create gravityVector from localGravStrength and localGravDirection.
			Vector3 gravityVector = localGravStrength * localGravDirection;

			// Apply gravityVector to arcadeMoveVector by Time.deltaTime.
			AddArcadeForce(gravityVector);
		}
	}

	void RBGravForce()
	{
		// Create gravForce vector using localGravDirection and localGravStrength (multiplied by the rigidbody mass to negate force difference).
		Vector3 gravForce = localGravDirection * localGravStrength * localRB.mass;

		// Create our own gravity force by using localRB.AddForce and applying a gravity vector calculated from gravityDirection and localGravStrength.
		localRB.AddForce (gravForce);

		Debug.DrawRay(transform.position, -localRB.velocity * 0.1f, Color.magenta);
	}

	void ArcadeMoveObject()
	{
		if (arcadePhysics) 				// If arcadePhysics are enabled...
		{
			// Directly set the rigidbody velocity to the arcadeMoveVector.
			localRB.velocity = arcadeMoveVector;

			// Draw a Debug.DrawRay of arcadeMoveVector.
			Debug.DrawRay(transform.position, -arcadeMoveVector * 0.1f, Color.magenta);
		}	
	}
}
