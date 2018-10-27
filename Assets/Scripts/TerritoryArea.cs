using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerritoryArea : MonoBehaviour, ISerializationCallbackReceiver {

	[SerializeField] GameObject _spawnPrefab = null;
	public GameObject SpawnPrefab{ get { return _spawnPrefab; } }

	[Range(1, 10)]
	[HideInInspector] 
	[SerializeField] int _spawnsCount = 1;
	public int SpawnsCount{ get { return _spawnsCount; } }

	[HideInInspector]
	[SerializeField] List<GameObject> _spawnedObjects = new List<GameObject> ();
	public List<GameObject> SpawnedObjects{ get { return _spawnedObjects; } }

	[HideInInspector]
	List<TerritoryAreaNode> _nodes = new List<TerritoryAreaNode> ();
	public List<TerritoryAreaNode> Nodes { get { return _nodes; } }

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

