using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(TerritoryArea), true)]
[CanEditMultipleObjects]
public class TerritoryAreaEditor : Editor {

	const float Border_Draw_Height = 4;
	const int Init_Nodes_Count = 4;


	[MenuItem("Game Tools/Create Territory Area")]
	public static void CreateTerritoryArea()
	{
		var obj = new GameObject ();
		obj.name = "TerritoryAreya";
		obj.AddComponent<TerritoryArea> ();
		Vector3 pos = new Vector3 (0, Border_Draw_Height / 2, 0);
		obj.transform.position = pos;
		Selection.activeGameObject = obj;
		Undo.RegisterCreatedObjectUndo (obj, obj.name);
	}


	SerializedProperty _spawnsCountProp;
	SerializedProperty _spawnPrefabProp;

	GUIStyle _guiStyle;
	TerritoryArea _territoryArea;
	int _selectedNode = -1;
	Vector3 _areaLastPosition;

	void Awake()
	{
		_territoryArea = target as TerritoryArea;

		if(_territoryArea.Nodes.Count == 0)
		{
			SetFirstNodes ();
		}
	}

	void OnEnable()
	{
		_spawnsCountProp = serializedObject.FindProperty ("SpawnsCount");
		_spawnPrefabProp = serializedObject.FindProperty ("SpawnPrefab");

		_areaLastPosition = _territoryArea.transform.position;
		_guiStyle = new GUIStyle();

		for (int i = 0; i < _territoryArea.SpawnedObjects.Count; ++i) {
			var spawnedObj = _territoryArea.SpawnedObjects [i];
			if (!spawnedObj)
				_territoryArea.SpawnedObjects.Remove (spawnedObj);
		}
	}

	void OnDisable()
	{
		Tools.hidden = false;
	}
	void OnDestroy()
	{
		Debug.Log ("babakkk");
	}

	void SetFirstNodes()
	{
		const float radius = 5;
		_territoryArea.Nodes.Clear ();
		for(int i = 0; i < Init_Nodes_Count; ++i)
		{
			float ang = i * (Mathf.PI * 2 / Init_Nodes_Count);
			Vector3 vec = new Vector3 (Mathf.Sin (ang), 0, Mathf.Cos (ang));
			Vector3 pos = _territoryArea.transform.position + vec * radius;
			var node = new TerritoryAreaNode ();
			node.position = pos;
			node.area = _territoryArea;
			_territoryArea.Nodes.Add (node);
		}
		_selectedNode = -1;

		EditorUtility.SetDirty(target);
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		GUILayout.BeginVertical ();
		GUILayout.Space (20);
		GUILayout.BeginHorizontal ();
		GUILayout.Space (70);
		if( GUILayout.Button("Center", GUILayout.Width(60), GUILayout.Height(30)))
		{
			Vector3 sumVec = Vector3.zero;
			var nodesList = _territoryArea.Nodes;
			for (int i = 0; i < nodesList.Count; ++i)
				sumVec += nodesList [i].position;
			Vector3 centerPos = sumVec /nodesList.Count;

			Vector3 offsetVec = centerPos - _territoryArea.transform.position;
			for (int i = 0; i < nodesList.Count; ++i)
				nodesList [i].position -= offsetVec;
			_territoryArea.transform.position += offsetVec;
		}
		GUILayout.Space (30);
		if( GUILayout.Button("Reset", GUILayout.Width(60), GUILayout.Height(30)))
		{
			SetFirstNodes ();
		}

		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical ();
		GUILayout.Space (30);
		EditorGUILayout.PropertyField (_spawnsCountProp);
		GUILayout.Space (10);
		EditorGUILayout.ObjectField (_spawnPrefabProp);
		GUILayout.Space (10);
		if (GUILayout.Button ("Spawn Quadruped", GUILayout.Height (30)))
		{
			for(int c = 0; c < _territoryArea.SpawnsCount; ++c)
			{
				Vector3 spawnPos = _territoryArea.transform.position;
				Vector3 spawnUpVec = Vector3.up;
				Quaternion spawnRot = Quaternion.identity;
				RaycastHit hitInfo;
				if(Physics.Raycast(spawnPos, new Vector3(0, 1, 0), out hitInfo, 10))
				{
					spawnPos = hitInfo.point + new Vector3(0, .2f, 0);
					spawnUpVec = hitInfo.normal;
					spawnRot = Quaternion.identity;
				}

				var spawnObj = GameObject.Instantiate (_territoryArea.SpawnPrefab, spawnPos, spawnRot);
				spawnObj.name = _territoryArea.SpawnPrefab.name;
				spawnObj.transform.rotation = Quaternion.LookRotation (spawnObj.transform.forward, spawnUpVec);
				spawnObj.GetComponent<Quadruped> ().TerritoryArea = _territoryArea;

				_territoryArea.SpawnedObjects.Add (spawnObj);
			}
		}
		GUILayout.EndVertical ();

		GUILayout.Space (100);

		serializedObject.ApplyModifiedProperties();
	}

	[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
	static void DrawGizmo(TerritoryArea area, GizmoType gizmoType)
	{
		var gizmoColor = Gizmos.color;
		Gizmos.color = Color.yellow;
		for (int i = 0; i < area.Nodes.Count; ++i)
		{
			var node1 = area.Nodes [i];
			var node2 = area.Nodes [(i + 1) % area.Nodes.Count];
			Gizmos.DrawLine (node1.position, node2.position);
		}
		Gizmos.color = gizmoColor;
	}

	protected virtual void OnSceneGUI()
	{
		Vector3 areaLastPositionOffset = _territoryArea.transform.position - _areaLastPosition;

        for (int i = 0; i < _territoryArea.SpawnedObjects.Count; ++i)
        {
            var spawnedObj = _territoryArea.SpawnedObjects[i];
            Vector3 objPos = spawnedObj.transform.position;
            RaycastHit[] hitResults = new RaycastHit[10];
            Physics.RaycastNonAlloc(objPos + new Vector3(0, 10, 0), new Vector3(0, -10, 0), hitResults);
            Vector3 hitNormalVec = spawnedObj.transform.up;
            foreach (var result in hitResults)
            {
                if (result.collider.gameObject == spawnedObj)
                    continue;
                objPos.y = result.point.y + spawnedObj.GetComponent<Collider>().bounds.extents.x;
                hitNormalVec = result.normal;
                Debug.Log(result.normal);
                break;
            }
            spawnedObj.transform.position = objPos + new Vector3(areaLastPositionOffset.x, 0, areaLastPositionOffset.z);

            Vector3 forwardVec = spawnedObj.transform.forward;
            Vector3 projectionVec = forwardVec - (Vector3.Dot(forwardVec, hitNormalVec)) * hitNormalVec;
            spawnedObj.transform.rotation = Quaternion.LookRotation(projectionVec, hitNormalVec);
        }

        var areaNodes = _territoryArea.Nodes;

        for (int i = 0; i < areaNodes.Count; ++i)
        {
            var node = areaNodes[i];
            node.position += areaLastPositionOffset;
        }        

		Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

		var handleColor = Handles.color;

		_guiStyle.alignment = TextAnchor.MiddleCenter;
		_guiStyle.normal.textColor = Color.black;
		_guiStyle.fontSize = 12;
		string spawnName = _territoryArea.SpawnPrefab == null ? "Set spawn prefab for" : _territoryArea.SpawnPrefab.name;
		string territoryName = string.Format ("{0}\n{1}", spawnName, _territoryArea.gameObject.name);
		Handles.Label (_territoryArea.transform.position, territoryName, _guiStyle);

		for(int i = 0; i < areaNodes.Count; ++i)
		{
			var node = areaNodes[i];

			Handles.color = Color.yellow;
			if(Handles.Button (node.position, Quaternion.identity, .25f, .25f, Handles.SphereHandleCap))
			{
				_selectedNode = i;
				break;
			}

			var nextNode = areaNodes[(i + 1) % areaNodes.Count];
			Vector3 toNextNodeDir = nextNode.position - node.position;
			Vector3 addNodeHandlePos = node.position + toNextNodeDir * .5f;
			Handles.color = Color.green;
			if(Handles.Button (addNodeHandlePos, Quaternion.identity, .15f, .15f, Handles.SphereHandleCap))
			{
				var newNode = new TerritoryAreaNode ();
				newNode.position = addNodeHandlePos + new Vector3 (-toNextNodeDir.z, 0, toNextNodeDir.x).normalized * .5f;
				newNode.area = _territoryArea;
				int insertIndex = i + 1;
                areaNodes.Insert(insertIndex, newNode);
				_selectedNode = insertIndex;
				break;
			}

			Vector3[] vertsArr = new Vector3[4];
			vertsArr [0] = node.position - new Vector3 (0, Border_Draw_Height, 0);
			vertsArr [1] = nextNode.position - new Vector3 (0, Border_Draw_Height, 0);
			vertsArr [2] = nextNode.position;
			vertsArr [3] = node.position;
			Handles.DrawSolidRectangleWithOutline (vertsArr, new Color (0, 1, .5f, .1f), new Color (.5f, 1, 1, .5f));
		}

		if(_selectedNode != -1)
		{
			var node = areaNodes [_selectedNode];
			float nodePosY = node.position.y;
			Vector3 handlePos = Handles.PositionHandle (node.position, Quaternion.identity);
			handlePos.y = nodePosY;
			node.position = handlePos;
		}

		Handles.color = handleColor;

		ProcessEvent ();

		Undo.RecordObject(target, _territoryArea.name);

		EditorUtility.SetDirty(target);

		_areaLastPosition = _territoryArea.transform.position;

		Tools.hidden = _selectedNode != -1;
	}

	void ProcessEvent()
	{
		var curEvent = Event.current;

		if(curEvent.type == EventType.ExecuteCommand)
		{
			if (curEvent.commandName == "FrameSelected")
			{
				curEvent.commandName = "";
				curEvent.Use();
			}
			if (curEvent.commandName == "Delete")
			{
			}
		}

		if(curEvent.type == EventType.KeyDown)
		{
			if(curEvent.keyCode == KeyCode.Delete)
			{
				if(_selectedNode != -1)
				{
					if(_territoryArea.Nodes.Count > 3)
					{
						_territoryArea.Nodes.RemoveAt (_selectedNode);
						_selectedNode = -1;
						GUIUtility.hotControl = 0;
						curEvent.Use ();
						_territoryArea.OnAreaChanged ();
					}
				}
			}

			if(curEvent.keyCode == KeyCode.Escape)
			{
				_selectedNode = -1;
				GUIUtility.hotControl = 0;
				curEvent.Use ();
				_territoryArea.OnAreaChanged ();
			}
		}

		Repaint ();
	}
}
