using System;
using UnityEngine;

public interface IQuadruped
{
	Animator Animator{ get; }
	Rigidbody Rigidbody{ get; }
	float WalkAnimSpeedTweaker{ get; }
	Transform Trans{ get; }
	TerritoryArea TerritoryArea{ get; }
	StateParameters StateParams{ get; }
}
