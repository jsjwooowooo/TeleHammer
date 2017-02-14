﻿using System;
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;


public class Thor : MonoBehaviour,
	Receiver<Message.EnemyKilledByHammer>
{
    enum ThorState
	{
		DEFAULT,
		TELEPORTING,
	}

	public float teleportTime = 1.0f;
	public float explosionRadius = 5.0F;
	public float explosionPower = 10.0F;
	public AudioClip explosionSound;

	private AudioSource regularSource;
	private ThorState thorState;
	private Vector3 teleportFrom;
	private Vector3 teleportTo;
	private float elapsedTeleportTime = 0.0f;

	// Use this for initialization
	void Start ()
	{
		regularSource = gameObject.AddComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update ()
	{
		if ( thorState == ThorState.DEFAULT )
		{
		}
		else if (thorState == ThorState.TELEPORTING)
		{
			elapsedTeleportTime += Time.deltaTime;
			float percentagePickupTime = Mathf.Clamp01( elapsedTeleportTime / teleportTime );

			transform.position = Vector3.Lerp( teleportFrom, teleportTo, percentagePickupTime );

			if ( elapsedTeleportTime >= teleportTime )
			{
				regularSource.PlayOneShot( explosionSound );
				Vector3 explosionPos = transform.position;
				Collider[] colliders = Physics.OverlapSphere( explosionPos, explosionRadius );
				foreach ( Collider hit in colliders )
				{
					Rigidbody rb = hit.GetComponent<Rigidbody>();

					if ( rb != null && hit.gameObject != gameObject)
						rb.AddExplosionForce( explosionPower, explosionPos, explosionRadius, 0.0f, ForceMode.Impulse );
				}

				thorState = ThorState.DEFAULT;
			}
		}
	}

	public void receive ( EnemyKilledByHammer o, GameObject sender )
	{
		thorState = ThorState.TELEPORTING;
		teleportFrom = gameObject.transform.position;
		teleportTo = o.enemyKillLocation;
		elapsedTeleportTime = 0.0f;
	}

	void OnCollisionEnter ( Collision collision )
	{
		if ( thorState == ThorState.TELEPORTING )
		{
			gameObject.sendMessage( new Message.PlayerAttack() { hitObject = collision.gameObject } );
		}
	}

	void OnTriggerEnter ( Collider collider )
	{
		if ( thorState == ThorState.TELEPORTING )
		{
			gameObject.sendMessage( new Message.PlayerAttack() { hitObject = collider.gameObject } );
		}
	}

}
