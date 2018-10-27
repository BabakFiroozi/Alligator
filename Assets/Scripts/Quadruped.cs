using UnityEngine;
using System.Collections;

public interface IQuadruped
{
	Animator Animator{ get; }
	Rigidbody Rigidbody{ get; }
	float WalkAnimSpeedTweaker{ get; }
	Transform Trans{ get; }
	TerritoryArea TerritoryArea{ get; }
	StateParameters StateParams{ get; }
}


[RequireComponent(typeof(GroundAligner))]
public class Quadruped : MonoBehaviour, IQuadruped
{
	const float Move_Force = 20;

	[HideInInspector] [SerializeField] TerritoryArea _territoryArea;
	[SerializeField] Animator _animator = null;
	[SerializeField] float _walkAnimSpeedTweaker = 5;

	Transform _tr;
	Rigidbody _rigidbody;
	QuadrupedState _currentState;

	public Animator Animator{ get { return _animator; } }
	public Rigidbody Rigidbody{ get { return _rigidbody; } }
	public float WalkAnimSpeedTweaker{ get { return _walkAnimSpeedTweaker; } }
	public Transform Trans{ get { return _tr; } }
	public TerritoryArea TerritoryArea{ get { return _territoryArea; } set { _territoryArea = value; } }

	[Space(10)]
	[SerializeField] StateParameters _stateParams = null;
	public StateParameters StateParams{ get { return _stateParams; } }


	// Use this for initialization
	void Start ()
	{
		_tr = transform;
		_rigidbody = GetComponent<Rigidbody> ();

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
