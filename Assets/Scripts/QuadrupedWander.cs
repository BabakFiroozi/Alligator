using UnityEngine;
using System.Collections;

public class QuadrupedWander : MonoBehaviour
{
	WanderState _currentState;

	[SerializeField] Animator _animator = null;
	[SerializeField] float _maxSpeed = .1f;

	[SerializeField] float _walkAnimSpeedTweaker = 5;

	const float Move_Force = 20;

	float _needForce;

	Transform _tr;
	Rigidbody _rigidbody;

	public Animator Animator{ get { return _animator; } }
	public Rigidbody Rigidbody{ get { return _rigidbody; } }
	public float MaxSpeed{ get { return _maxSpeed; } }
	public float WalkAnimSpeedTweaker{ get { return _walkAnimSpeedTweaker; } }
	public float NeedForce{ get { return _needForce; } }
	public Transform Tr{ get { return _tr; } }


	// Use this for initialization
	void Start ()
	{
		_tr = transform;
		_rigidbody = GetComponent<Rigidbody> ();

		_needForce = Move_Force * (_maxSpeed * 10);

		CurrentState = new WanderState_Move (this);
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


	public WanderState CurrentState
	{
		get	{ return _currentState; }
		set
		{
			var state = value;
			if (_currentState != null)
				_currentState.OnStateExit ();
			_currentState = state;
			_currentState.OnStateEnter ();
		}
	}
}

