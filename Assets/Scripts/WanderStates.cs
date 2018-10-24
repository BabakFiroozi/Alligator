using UnityEngine;
using System.Collections;

public abstract class WanderState
{
	protected QuadrupedWander _quadruped;

	public WanderState(QuadrupedWander qaudruped)
	{
		_quadruped = qaudruped;
	}

	public abstract void OnStateRun();
	public abstract void OnStateRunFixed();

	public virtual void OnStateEnter()
	{
	}
	public virtual void OnStateExit()
	{
	}
}

/// <summary>
/// Idle state implementation for wander
/// </summary>
public class WanderState_Idle : WanderState
{
	public WanderState_Idle(QuadrupedWander quadruped) : base(quadruped)
	{
	}

	public override void OnStateRun ()
	{
	}

	public override void OnStateRunFixed ()
	{
	}

	public override void OnStateEnter ()
	{
		base.OnStateEnter ();
		_quadruped.Animator.SetTrigger ("idle");
	}

	public override void OnStateExit ()
	{
		base.OnStateExit ();
	}
}

/// <summary>
/// Move state implementation for wander
/// </summary>
public class WanderState_Move : WanderState
{
	public WanderState_Move(QuadrupedWander quadruped) : base(quadruped)
	{
	}

	public override void OnStateRun ()
	{
		Vector3 vel = _quadruped.Rigidbody.velocity;
		_quadruped.Animator.SetFloat ("walkSpeedMult", vel.magnitude * _quadruped.WalkAnimSpeedTweaker);
	}

	public override void OnStateRunFixed ()
	{
		Vector3 moveDir = _quadruped.Tr.forward;
		var _rigidbody = _quadruped.Rigidbody;
		float _maxSpeed = _quadruped.MaxSpeed;

		Vector3 vel = _rigidbody.velocity;
		_rigidbody.velocity = Vector3.zero;

		if(vel.magnitude < _maxSpeed)
		{
			float force = _quadruped.NeedForce;
			Vector3 forceVec = moveDir * force * Time.fixedDeltaTime;
			_rigidbody.AddForce (forceVec, ForceMode.Impulse);
		}
	}

	public override void OnStateEnter ()
	{
		base.OnStateEnter ();
		_quadruped.Animator.SetTrigger ("walk");
	}

	public override void OnStateExit ()
	{
		base.OnStateExit ();
	}
}
