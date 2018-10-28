using System;
using UnityEngine;
using System.Collections.Generic;


public interface IQuadruped
{
	List<Vector3> BorderPoints{get;}
	Vector3 MoveDirection{ get;}
}
