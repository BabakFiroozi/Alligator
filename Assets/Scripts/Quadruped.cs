﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GroundAligner))]
public class Quadruped : MonoBehaviour, IQuadruped
{
	public const float BORDER_STEP = 1;
	const float Move_Force = 20;

	[SerializeField] TerritoryArea _territoryArea;
	[SerializeField] Animator _animator = null;
	[SerializeField] float _walkAnimSpeedTweaker = 5;

	Transform _tr;
	Rigidbody _rigidbody;
	QuadrupedState _currentState;
	List<Vector3> _borderPoints = new List<Vector3>();

	public Animator Animator{ get { return _animator; } }
	public Rigidbody Rigidbody{ get { return _rigidbody; } }
	public float WalkAnimSpeedTweaker{ get { return _walkAnimSpeedTweaker; } }
	public TerritoryArea TerritoryArea	{ get { return _territoryArea; } set { _territoryArea = value; } }
	public List<Vector3> BorderPoints { get { return _borderPoints; } }
	public Vector3 MoveDirection{ get { return _moveDirection; } }

	[Space(10)]
	[SerializeField] StateParameters _stateParams = null;
	public StateParameters StateParams{ get { return _stateParams; } }

	Vector3 _moveDirection;


	// Use this for initialization
	void Start ()
	{
		_tr = transform;
		_rigidbody = GetComponent<Rigidbody> ();

		_rigidbody.sleepThreshold = .03f;

		var areaNodes = _territoryArea.Nodes;
		for(int n = 0; n < areaNodes.Count; ++n)
		{
			var node1 = areaNodes [n];
			var node2 = areaNodes [(n + 1) % areaNodes.Count];

			Vector3 dir = node2.position - node1.position;
			float dist = dir.magnitude;
			dir.Normalize ();

			float step = BORDER_STEP;
			for(float d = 0; d < dist; d += step)
			{
				Vector3 point = node1.position + dir * d;
				_borderPoints.Add (point);
			}
		}

		CurrentState = new QuadrupedState_Wander (this);
	}

	void FixedUpdate()
	{
		_currentState.OnStateRun ();

		float maxSpeed = _stateParams.Wander_Speed;

		float wanderRadius = _stateParams.Wander_Radius;

		var rigbody = _rigidbody;
		Vector3 rigbodyPos = rigbody.position;
		Vector3 rigbodyDir = rigbody.rotation * Vector3.forward;

		Vector3 bodyVel = rigbody.velocity;

		Vector3 moveDir = _moveDirection;

		if(bodyVel.magnitude < maxSpeed)
		{
			//Add 30 forces for .1 speed
			float needForce = 30 * (maxSpeed * 10);
			Vector3 forceVec = moveDir * needForce * Time.fixedDeltaTime;
			rigbody.AddForce (forceVec, ForceMode.Impulse);
			Vector3 upwardVec = Vector3.Cross ((rigbody.rotation * Vector3.left), (rigbody.rotation * Vector3.forward));
			Quaternion fromQuat = Quaternion.LookRotation (rigbodyDir, upwardVec);
			Quaternion toQuat = Quaternion.LookRotation (moveDir, upwardVec);
			float angleStep = Time.fixedDeltaTime * wanderRadius + _stateParams.Wander_Randomness;
			rigbody.rotation = Quaternion.RotateTowards (fromQuat, toQuat, angleStep);
		}
		else
		{
			rigbody.velocity = bodyVel.normalized * maxSpeed;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 vel = _rigidbody.velocity;
		_animator.SetFloat ("walkSpeedMult", vel.magnitude * _walkAnimSpeedTweaker);
		_animator.SetFloat ("velocity", vel.magnitude);
	}

	void LateUpdate()
	{
		Animator.transform.localRotation = Quaternion.identity;
		Animator.transform.localPosition = Vector3.zero;
	}

	public QuadrupedState CurrentState
	{
		get	{ return _currentState; }
		set
		{
			var state = value;
			if (state == _currentState)
				return;
			if (_currentState != null)
				_currentState.OnStateExit ();
			_currentState = state;
			_currentState.OnStateEnter ();
		}
	}


	void OnDrawGizmos()
	{
		foreach(var p in _borderPoints)
			Gizmos.DrawWireSphere(p, .1f);
	}

	public void SetMoveDirection(Vector3 moveDir)
	{
		_moveDirection = moveDir;
	}
}
