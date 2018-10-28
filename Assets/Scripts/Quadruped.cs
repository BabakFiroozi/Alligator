using UnityEngine;
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

	[Space(10)]
	[SerializeField] StateParameters _stateParams = null;
	public StateParameters StateParams{ get { return _stateParams; } }


	// Use this for initialization
	void Start ()
	{
		_tr = transform;
		_rigidbody = GetComponent<Rigidbody> ();

		_rigidbody.sleepThreshold = .03f;

		_borderPoints.Clear ();
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
}
