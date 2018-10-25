using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour, ISerializationCallbackReceiver {

	[HideInInspector]
	public List<SpawnAreaNode> Nodes = new List<SpawnAreaNode>();

	[Range(5, 20)]
	public float Radius = 5;

	public void OnAreaChanged() 
	{
	}

	public void OnAfterDeserialize()
	{
		for (int i = 0; i < Nodes.Count; ++i)
			Nodes [i].area = this;
		
		Debug.Log ("OnAfterDeserialize");
	}

	public void OnBeforeSerialize()
	{
	}
}

[System.Serializable]
public class SpawnAreaNode
{
	[UnityEngine.Serialization.FormerlySerializedAs("Position")]
	public Vector3 position;

	public SpawnArea area;
}

