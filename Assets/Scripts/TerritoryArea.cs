using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerritoryArea : MonoBehaviour, ISerializationCallbackReceiver {

	public GameObject SpawnPrefab;

	[HideInInspector]
	public List<GameObject> SpawnedObjects = new List<GameObject> ();

	[HideInInspector]
	public List<TerritoryAreaNode> Nodes = new List<TerritoryAreaNode>();

	[Range(1, 10)]
	public int SpawnsCount = 1;

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

	void OnDrawGizmos()
	{
		Gizmos.DrawIcon (transform.position, "territoryArea");
	}

	void OnDestroy()
	{
		if(Application.isEditor)
		{
			foreach(var obj in SpawnedObjects)
				GameObject.DestroyImmediate (obj);
			SpawnedObjects.Clear ();
		}
	}
}

[System.Serializable]
public class TerritoryAreaNode
{
	[UnityEngine.Serialization.FormerlySerializedAs("Position")]
	public Vector3 position;

	public TerritoryArea area;
}

