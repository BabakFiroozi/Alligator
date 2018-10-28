using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(Quadruped), true)]
public class QuadrupedEditor : Editor {

	Quadruped _quadruped;


    void Awake()
	{
        _quadruped = target as Quadruped;

		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
	}

	void OnDestroy()
	{
		EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
	}

	public override void OnInspectorGUI()
	{
		GUILayout.Space (10);
		if( GUILayout.Button("Select Territory Area"))
		{
			if (_quadruped.TerritoryArea != null)
				Selection.activeObject = _quadruped.TerritoryArea.gameObject;
		}

		base.OnInspectorGUI();
	}

	protected virtual void OnSceneGUI()
	{
		Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

		var handleColor = Handles.color;

		if (_quadruped.TerritoryArea == null)
			return;

        var areaNodes = _quadruped.TerritoryArea.Nodes;

		Handles.color = Color.green;
        for (int i = 0; i < areaNodes.Count; ++i)
		{
			var node = areaNodes[i];
			var nextNode = areaNodes[(i + 1) % areaNodes.Count];
			Vector3[] vertsArr = new Vector3[4];
			vertsArr [0] = node.position - new Vector3 (0, TerritoryAreaEditor.Border_Draw_Height, 0);
			vertsArr [1] = nextNode.position - new Vector3 (0, TerritoryAreaEditor.Border_Draw_Height, 0);
			vertsArr [2] = nextNode.position;
			vertsArr [3] = node.position;
			Handles.DrawSolidRectangleWithOutline (vertsArr, new Color (0, 1, .5f, .1f), new Color (.5f, 1, 1, .5f));
		}

		ProcessEvent ();
	}

	void OnHierarchyGUI(int instanceID,Rect selectionRect)
	{
		ProcessEvent ();
	}

	void ProcessEvent()
	{
		var curEvent = Event.current;

		if(curEvent.type == EventType.ExecuteCommand)
		{
			if (curEvent.commandName == "Duplicate")
			{
				SpawnQuadrupedObj ();
				curEvent.commandName = "";
				curEvent.Use();
			}
		}

		if(curEvent.type == EventType.KeyDown)
		{
			if (curEvent.control && curEvent.keyCode == KeyCode.D)
			{
				SpawnQuadrupedObj ();
				curEvent.commandName = "";
				curEvent.Use();
			}
		}

		Repaint ();
	}

	void SpawnQuadrupedObj()
	{
		var territoryArea = _quadruped.TerritoryArea;
		if (territoryArea == null)
			return;

		Vector3 spawnPos = territoryArea.transform.position;
		Vector3 spawnUpVec = Vector3.up;
		var prefabObj = territoryArea.SpawnedObjects.Count > 0 ? territoryArea.SpawnedObjects[0] : territoryArea.SpawnPrefab;
		var spawnObj = PrefabUtility.InstantiatePrefab (territoryArea.SpawnPrefab) as GameObject;
		spawnObj.name = territoryArea.SpawnPrefab.name + "_" + territoryArea.SpawnedObjects.Count;
		spawnObj.transform.position = territoryArea.SpawnedObjects.Count > 0 ? prefabObj.transform.position : spawnPos;
		spawnObj.transform.rotation = Quaternion.Euler (0, UnityEngine.Random.Range (0, 360), 0);

		spawnObj.GetComponent<Quadruped> ().TerritoryArea = territoryArea;
		spawnObj.transform.SetSiblingIndex (territoryArea.transform.GetSiblingIndex ());
		territoryArea.SpawnedObjects.Add (spawnObj);
	}
}
