using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


[System.Serializable]
public class StateParameters
{
	[Range(.1f, .5f)]
	public float Wander_Speed;
	[Range(1, 10)]
	public int Wander_Randomness;

	public StateParameters()
	{
		Wander_Randomness = 10;
		Wander_Speed = .2f;
	}
}

public abstract class QuadrupedState
{
	protected Quadruped _quadruped;
	public QuadrupedState(Quadruped qaudruped)
	{
		_quadruped = qaudruped;
	}
	public virtual void OnStateEnter()
	{
	}

	public abstract void OnStateRun();

	public virtual void OnStateExit()
	{
	}
}

// Idle state
public class QuadrupedState_Idle : QuadrupedState
{
	public QuadrupedState_Idle(Quadruped quadruped) : base(quadruped)
	{
	}

	public override void OnStateEnter ()
	{
		base.OnStateEnter ();
	}

	public override void OnStateRun ()
	{
		_quadruped.Rigidbody.velocity = Vector3.zero;
	}

	public override void OnStateExit ()
	{
		base.OnStateExit ();
	}
}

// Wander state
public class QuadrupedState_Wander : QuadrupedState
{
	const float Wander_Distance = 10;
	const float Wander_Radius = 3;

	Vector3 _targetPoint;

	public QuadrupedState_Wander(Quadruped quadruped) : base(quadruped)
	{
	}

	public override void OnStateEnter ()
	{
		base.OnStateEnter ();

		float ang = Random.Range (0, Mathf.PI * 2);
		_targetPoint = new Vector3 (Mathf.Sin (ang), 0, Mathf.Cos (ang)) * Wander_Radius;
	}

	public override void OnStateRun ()
	{
		var rigidbody = _quadruped.Rigidbody;
		float maxSpeed = _quadruped.StateParams.Wander_Speed;

		Vector3 rigbodyPos = rigidbody.position;

		float needForce = 30 * (maxSpeed * 10);

		Vector3 rigbodyDir = rigidbody.rotation * Vector3.forward;

		float randomness = _quadruped.StateParams.Wander_Randomness * Time.fixedDeltaTime;
		_targetPoint += new Vector3 (Random.Range (-1f, 1f) * randomness, 0, Random.Range (-1f, 1f) * randomness);
		_targetPoint.Normalize ();
		_targetPoint *=  Wander_Radius;
		Vector3 wanderPos = rigbodyPos + rigbodyDir * Wander_Distance + _targetPoint;

		Vector3 wanderDir = (wanderPos - rigbodyPos).normalized;

		var _areaPoints = _quadruped.BorderPoints;
		for(int p = 0; p < _areaPoints.Count; ++p)
		{
			var point = _areaPoints [p];
			point.y = rigbodyPos.y;
			Vector3 dir = point - rigbodyPos;
			if (dir.magnitude < Quadruped.BORDER_STEP && Vector3.Angle (dir, rigbodyDir) < 90)
			{
				Vector3 avgVec = -dir.normalized + new Vector3 (rigbodyDir.x, dir.y, rigbodyDir.z).normalized;
				avgVec.Normalize ();
				_targetPoint += avgVec.normalized * Wander_Distance;
				break;
			}
		}

		Vector3 bodyVel = rigidbody.velocity;

		if(bodyVel.magnitude < maxSpeed)
		{
			float force = needForce;
			Vector3 forceVec = wanderDir * force * Time.fixedDeltaTime;
			rigidbody.AddForce (forceVec, ForceMode.Impulse);
			float angleStep = Time.fixedDeltaTime * Wander_Radius * 1.0f +_quadruped.StateParams.Wander_Randomness;
			Vector3 upwardVec = Vector3.Cross ((rigidbody.rotation * Vector3.left), (rigidbody.rotation * Vector3.forward));
			Quaternion fromQuat = Quaternion.LookRotation (rigbodyDir, upwardVec);
			Quaternion toQuat = Quaternion.LookRotation (wanderDir, upwardVec);
			rigidbody.rotation = Quaternion.RotateTowards (fromQuat, toQuat, angleStep);
		}
		else
		{
			rigidbody.velocity = bodyVel.normalized * maxSpeed;
		}

	}

	public override void OnStateExit ()
	{
		base.OnStateExit ();
	}
}
