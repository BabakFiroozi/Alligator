using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


[System.Serializable]
public class StateParameters
{
	[Range(.2f, .5f)]
	public float Wander_Speed;
	[Range(1, 50)]
	public int Wander_Randomness;
	[Range(3, 10)]
	public float Wander_Radius = 3;

	public StateParameters()
	{
		Wander_Speed = .3f;
		Wander_Randomness = 30;
		Wander_Radius = 5;
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

	public virtual void OnStateRun()
	{
		
	}

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

		_quadruped.StopMovement = true;
	}

	public override void OnStateRun ()
	{
		base.OnStateRun ();
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

	public QuadrupedState_Wander(Quadruped quadruped) : base(quadruped)
	{
	}

	public override void OnStateEnter ()
	{
		base.OnStateEnter ();

		_quadruped.StopMovement = false;

		float ang = Random.Range (0, Mathf.PI * 2);
		_targetPoint = new Vector3 (Mathf.Sin (ang), 0, Mathf.Cos (ang)) * _quadruped.StateParams.Wander_Radius;
	}

	public override void OnStateRun ()
	{
		base.OnStateRun ();

		var rigbody = _quadruped.Rigidbody;

		//optimal value
		float distance = 5 * (1 + rigbody.velocity.magnitude * 10) - 10;

		Vector3 rigbodyPos = rigbody.position;

		float wanderRadius = _quadruped.StateParams.Wander_Radius;

		Vector3 rigbodyDir = rigbody.rotation * Vector3.forward;

		float randomness = _quadruped.StateParams.Wander_Randomness * Time.fixedDeltaTime;
		_targetPoint += new Vector3 (Random.Range (-1f, 1f) * randomness, 0, Random.Range (-1f, 1f) * randomness);
		_targetPoint.Normalize ();
		_targetPoint *=  wanderRadius;
		Vector3 wanderPos = rigbodyPos + rigbodyDir * distance + _targetPoint;

		Vector3 wanderDir = (wanderPos - rigbodyPos).normalized;

		//As long as it's near a border point then steer target point to the some average side direction
		var _areaPoints = _quadruped.BorderPoints;
		for(int p = 0; p < _areaPoints.Count; ++p)
		{
			var point = _areaPoints [p];
			point.y = rigbodyPos.y;
			Vector3 toPointDir = point - rigbodyPos;
			float speedAdaptlimit = Quadruped.BORDER_STEP + rigbody.velocity.magnitude * .25f;
			if (toPointDir.magnitude < speedAdaptlimit && Vector3.Angle (toPointDir, rigbodyDir) < 75)
			{
				Vector3 avgVec = -toPointDir.normalized + new Vector3 (rigbodyDir.x, toPointDir.y, rigbodyDir.z).normalized;
				_targetPoint += avgVec.normalized * distance;
				break;
			}
		}
		//

		_quadruped.SetMoveDirection (wanderDir, _quadruped.StateParams.Wander_Radius);
	}

	public override void OnStateExit ()
	{
		base.OnStateExit ();
	}
}
