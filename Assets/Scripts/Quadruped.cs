using UnityEngine;
using System.Collections;

public interface IQuadruped
{
	Animator Animator{ get; }
	Rigidbody Rigidbody{ get; }
	float MaxSpeed{ get; }
	float WalkAnimSpeedTweaker{ get; }
	float NeedForce{ get; }
	Transform Trans{ get; }
}


[RequireComponent(typeof(GroundAligner))]
public class Quadruped : MonoBehaviour, IQuadruped
{
	const float Move_Force = 20;

    [HideInInspector]
	[SerializeField] TerritoryArea _territoryArea;
	public TerritoryArea TerritoryArea
	{
		get { return _territoryArea; }
		set { _territoryArea = value; } 
	}

    [SerializeField] Animator _animator = null;
	[SerializeField] float _maxSpeed = .1f;
	[SerializeField] float _walkAnimSpeedTweaker = 5;

	float _needForce;
	Transform _tr;
	Rigidbody _rigidbody;
	QuadrupedState _currentState;

	public Animator Animator{ get { return _animator; } }
	public Rigidbody Rigidbody{ get { return _rigidbody; } }
	public float MaxSpeed{ get { return _maxSpeed; } }
	public float WalkAnimSpeedTweaker{ get { return _walkAnimSpeedTweaker; } }
	public float NeedForce{ get { return _needForce; } }
	public Transform Trans{ get { return _tr; } }


	// Use this for initialization
	void Start ()
	{
		_tr = transform;
		_rigidbody = GetComponent<Rigidbody> ();
		_needForce = Move_Force * (_maxSpeed * 10);

		CurrentState = new QuadrupedState_Wander (this);
	}

	void FixedUpdate()
	{
		_currentState.OnStateRunFixed ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		_currentState.OnStateRun ();
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
}
