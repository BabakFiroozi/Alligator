using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;


[System.Serializable]
public class StateParameters
{
	[Range(.1f, 1)]
	public float Wander_Speed;
	[Range(1, 100)]
	public int Wander_Randomness;
	[Range(.5f, 20)]
	public float Wander_Radius;

	public StateParameters()
	{
		Wander_Randomness = 10;
		Wander_Speed = .2f;
		Wander_Radius = 2;
	}
}

public abstract class QuadrupedState
{
	protected IQuadruped _quadruped;
	public QuadrupedState(IQuadruped qaudruped)
	{
		_quadruped = qaudruped;
	}
	public virtual void OnStateEnter()
	{
	}

	public abstract void OnStateRunFixed();

	public abstract void OnStateRun();

	public virtual void OnStateExit()
	{
	}
}

// Idle state
public class QuadrupedState_Idle : QuadrupedState
{
	public QuadrupedState_Idle(IQuadruped quadruped) : base(quadruped)
	{
	}

	public override void OnStateEnter ()
	{
		base.OnStateEnter ();
		_quadruped.Rigidbody.velocity = Vector3.zero;
		_quadruped.Animator.SetTrigger ("idle");
	}

	public override void OnStateRunFixed ()
	{
	}

	public override void OnStateRun ()
	{
	}

	public override void OnStateExit ()
	{
		base.OnStateExit ();
	}
}

// Wander state
public class QuadrupedState_Wander : QuadrupedState
{
	Vector3 _targetPoint;
	float _distance = 10;

	public QuadrupedState_Wander(IQuadruped quadruped) : base(quadruped)
	{
	}

	public override void OnStateEnter ()
	{
		base.OnStateEnter ();

		_quadruped.Animator.SetTrigger ("walk");

		float ang = Random.Range (0, Mathf.PI * 2);
		_targetPoint = new Vector3 (Mathf.Sin (ang), 0, Mathf.Cos (ang)) * _quadruped.StateParams.Wander_Radius;
	}

	public override void OnStateRunFixed ()
	{
		var rigidbody = _quadruped.Rigidbody;
		float maxSpeed = _quadruped.StateParams.Wander_Speed;

		float needForce = 30 * (maxSpeed * 10);

		Vector3 vel = rigidbody.velocity;

		rigidbody.velocity = Vector3.zero;

		float randomness = _quadruped.StateParams.Wander_Randomness * Time.fixedDeltaTime;
		_targetPoint += new Vector3 (Random.Range (-1f, 1f) * randomness, 0, Random.Range (-1f, 1f) * randomness);
		_targetPoint.Normalize ();
		_targetPoint *=  _quadruped.StateParams.Wander_Radius;
		Vector3 wanderPos = _quadruped.Trans.position + _quadruped.Trans.forward * _distance + _targetPoint;

		Vector3 wanderDir = (wanderPos - _quadruped.Trans.position).normalized;

		if(vel.magnitude < maxSpeed)
		{
			float force = needForce;
			Vector3 forceVec = wanderDir * force * Time.fixedDeltaTime;
			rigidbody.AddForce (forceVec, ForceMode.Impulse);
			rigidbody.rotation = Quaternion.RotateTowards (Quaternion.LookRotation(_quadruped.Trans.forward),
				Quaternion.LookRotation (wanderDir), Time.fixedDeltaTime * 45);
		}
		else
		{
			rigidbody.velocity = vel.normalized * maxSpeed;
		}

	}


	public override void OnStateRun ()
	{
		Vector3 vel = _quadruped.Rigidbody.velocity;
		_quadruped.Animator.SetFloat ("walkSpeedMult", vel.magnitude * _quadruped.WalkAnimSpeedTweaker);
	}

	public override void OnStateExit ()
	{
		base.OnStateExit ();
	}
}
