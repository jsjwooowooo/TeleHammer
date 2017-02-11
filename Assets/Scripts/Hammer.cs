﻿using System;
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class Hammer : MonoBehaviour,
	Receiver<Message.ChargeHammer>,
	Receiver<Message.ThrowHammer>,
	Receiver<Message.HammerCollision>
{

	enum HammerState
	{
		HELD,
		CHARGING,
		TRANSITIONING_TO_HELD,
		THROWN
	}

	public float chargeRate = 10.0f;
	public float maxThrow = 100.0f;
	public float hammerPickupTime = 1.0f;
	public GameObject hammerOwner;


	private float elapsedHammerPickupTime = 0.0f;
	private Vector3 droppedLocalPosition;
	private Quaternion droppedLocalRotation;

	private HammerState hammerState;
	private float throwPower = 0.0f;
	private Rigidbody rbody = null;

	private Vector3 initialLocalPosition;
	private Quaternion initialLocalRotation;
	private Transform initialParentTransform;



	// Use this for initialization
	void Start ()
	{
		initialParentTransform = gameObject.transform.parent;
		initialLocalPosition = transform.localPosition;
		initialLocalRotation = transform.localRotation;

		rbody = gameObject.AddOrGetComponent<Rigidbody>();
		rbody.isKinematic = true;
	}
	
	// Update is called once per frame
	void Update ()
	{

		if ( hammerState == HammerState.CHARGING )
		{
			throwPower += chargeRate * Time.deltaTime;

			if ( throwPower > maxThrow )
			{
				throwPower = maxThrow;
			}
		}
		else if (hammerState == HammerState.TRANSITIONING_TO_HELD)
		{
			elapsedHammerPickupTime += Time.deltaTime;
			float percentagePickupTime = Mathf.Clamp01(elapsedHammerPickupTime / hammerPickupTime);

			transform.localPosition = Vector3.Lerp( droppedLocalPosition, initialLocalPosition, percentagePickupTime );
			transform.localRotation = Quaternion.Lerp( droppedLocalRotation, initialLocalRotation, percentagePickupTime );

			if ( percentagePickupTime == 1.0f)
			{
				int x = 0;
			}

			if ( elapsedHammerPickupTime >= hammerPickupTime )
			{
				elapsedHammerPickupTime = 0.0f;
				hammerState = HammerState.HELD;
			}

		}
	}

	public void receive ( ChargeHammer o, GameObject sender )
	{
		if (hammerState != HammerState.HELD)
		{
			return;
		}

		Debug.Log( "Hammer charging" );
		hammerState = HammerState.CHARGING;
	}

	public void receive ( ThrowHammer o, GameObject sender )
	{
		if ( hammerState != HammerState.CHARGING )
		{
			return;
		}

		Debug.Log( "Thrown with power " + throwPower );

		rbody.isKinematic = false;
		rbody.AddRelativeForce( 0, 0, throwPower, ForceMode.Impulse );
		rbody.AddRelativeTorque( throwPower, 0, 0, ForceMode.Impulse );

		throwPower = 0.0f;
		hammerState = HammerState.THROWN;
		transform.parent = null;
	}

	public void receive ( HammerCollision o, GameObject sender )
	{
		if (sender != gameObject || o.hitObject != hammerOwner || hammerState != HammerState.THROWN)
		{
			return;
		}

		rbody.isKinematic = true;
		transform.parent = initialParentTransform;
		hammerState = HammerState.TRANSITIONING_TO_HELD;

		droppedLocalPosition = transform.localPosition;
		droppedLocalRotation = transform.localRotation;
	}

	void OnCollisionEnter( Collision collision)
	{
		gameObject.sendMessage( new Message.HammerCollision() { hitObject = collision.gameObject } );
	}

	void OnTriggerEnter ( Collider collider )
	{
		gameObject.sendMessage( new Message.HammerCollision() { hitObject = collider.gameObject } );
	}
}
