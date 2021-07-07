using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPositionToZero : MonoBehaviour 
{
	public bool onlyLocal;


	void Start () 
	{
		
	}

	void Update () 
	{
		SetPosition ();
	}


	void SetPosition()
	{
		if (!onlyLocal) transform.position = Vector3.zero;
		else transform.localPosition = Vector3.zero;
	}
}
