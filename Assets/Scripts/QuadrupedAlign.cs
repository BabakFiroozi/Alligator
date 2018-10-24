﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedAlign : MonoBehaviour {

	Transform _tr;
	Rigidbody _rigidBody;
	CapsuleCollider _bodyCollider;

	/// <summary>
	/// Use 2 raycasts, one for back and another for front
	/// </summary>
	RaycastHit[] _groundHitInfos = new RaycastHit[2];


	// Use this for initialization
	void Start ()
	{
		_tr = transform;
		_rigidBody = GetComponent<Rigidbody> ();
		_bodyCollider = GetComponent<CapsuleCollider> ();
	}

	void FixedUpdate()
	{
		const float ground_check_dist = 10;

		int layerMask = LayerMaskUtil.GetLayerMask(0);//Default
		//layerMask |= LayerMaskUtil.GetLayerMask ("adas");

		Vector3 bodyDir = _rigidBody.rotation * Vector3.forward;
		Vector3 centerWordPos = _tr.position + _bodyCollider.center;

		bool invalidGround = false;

		//Cats 2 rays, back to down, front to down, then calculates average slope normal
		for(int c = 0; c < 2; ++c)
		{
			float vecSign = c == 0 ? -1 : 1;
			Vector3 origin = (centerWordPos + bodyDir * (_bodyCollider.height * .5f + .02f) * vecSign);
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (origin, Vector3.down, out hitInfo, ground_check_dist, layerMask);
			if (!hit)
			{
				invalidGround = true;
				break;
			}
			_groundHitInfos [c] = hitInfo;
		}

		Vector3 upVec = Vector3.up;
		if(!invalidGround)
			upVec = _groundHitInfos [0].normal + _groundHitInfos [1].normal;

		_rigidBody.rotation = Quaternion.LookRotation (_tr.forward, upVec);
	}
}
