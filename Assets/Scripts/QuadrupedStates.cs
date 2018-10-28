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
	[Range(1, 5)]
	public float Wander_Radius = 3;

	public StateParameters()
	{
		Wander_Randomness = 5;
		Wander_Speed = .3f;
		Wander_Radius = 3;
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
	//Optimal value
	const float Wander_Distance = 10;

	Vector3 _targetPoint;

	public QuadrupedState_Wander(Quadruped quadruped) : base(quadruped)
	{
	}

	public override void OnStateEnter ()
	{
		base.OnStateEnter ();

		float ang = Random.Range (0, Mathf.PI * 2);
		_targetPoint = new Vector3 (Mathf.Sin (ang), 0, Mathf.Cos (ang)) * _quadruped.StateParams.Wander_Radius;
	}

	public override void OnStateRun ()
	{
		var rigbody = _quadruped.Rigidbody;

		Vector3 rigbodyPos = rigbody.position;

		float wanderRadius = _quadruped.StateParams.Wander_Radius;

		Vector3 rigbodyDir = rigbody.rotation * Vector3.forward;

		float randomness = _quadruped.StateParams.Wander_Randomness * Time.fixedDeltaTime;
		_targetPoint += new Vector3 (Random.Range (-1f, 1f) * randomness, 0, Random.Range (-1f, 1f) * randomness);
		_targetPoint.Normalize ();
		_targetPoint *=  wanderRadius;
		Vector3 wanderPos = rigbodyPos + rigbodyDir * Wander_Distance + _targetPoint;

		Vector3 wanderDir = (wanderPos - rigbodyPos).normalized;

		//As long as it's near a border point then steer target point to the some average side direction
		var _areaPoints = _quadruped.BorderPoints;
		for(int p = 0; p < _areaPoints.Count; ++p)
		{
			var point = _areaPoints [p];
			point.y = rigbodyPos.y;
			Vector3 toPointDir = point - rigbodyPos;
			if (toPointDir.magnitude < Quadruped.BORDER_STEP && Vector3.Angle (toPointDir, rigbodyDir) < 60)
			{
				Vector3 avgVec = -toPointDir.normalized + new Vector3 (rigbodyDir.x, toPointDir.y, rigbodyDir.z).normalized;
				_targetPoint += avgVec.normalized * Wander_Distance;
				break;
			}
		}
		//

		_quadruped.SetMoveDirection (wanderDir);
	}

	public override void OnStateExit ()
	{
		base.OnStateExit ();
	}
}
