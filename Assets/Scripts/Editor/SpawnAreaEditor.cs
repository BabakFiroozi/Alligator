using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(SpawnArea), true)]
public class SpawnAreaEditor : Editor {

	const int Init_Nodes_Count = 4;

	SerializedProperty radiusProp;

	GUIStyle _textStyle;

	SpawnArea _spawnArea;

	int _selectedNode = -1;

	Vector3 _areaLastPosition;


	void Awake()
	{
		_spawnArea = target as SpawnArea;

		if(_spawnArea.Nodes.Count == 0)
		{
			for(int i = 0; i < Init_Nodes_Count; ++i)
			{
				float ang = i * (Mathf.PI * 2 / Init_Nodes_Count);
				Vector3 vec = new Vector3 (Mathf.Sin (ang), 0, Mathf.Cos (ang));
				Vector3 pos = _spawnArea.transform.position + vec * _spawnArea.Radius;
				var node = new SpawnAreaNode ();
				node.position = pos;
				node.area = _spawnArea;
				_spawnArea.Nodes.Add (node);
			}
		}
	}

	void OnEnable()
	{
		radiusProp = serializedObject.FindProperty ("Radius");

//		Tools.hidden = true;
//		textStyle = new GUIStyle();
//		textStyle.alignment = TextAnchor.MiddleCenter;
//		textStyle.normal.textColor = Color.white;
	}

	void OnDisable()
	{
		//Tools.hidden = false;
	}

	void OnDestroy()
	{
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField (radiusProp);

		serializedObject.ApplyModifiedProperties();
	}

	[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
	static void DrawGizmo(SpawnArea spawnArea, GizmoType gizmoType)
	{
	}

	protected virtual void OnSceneGUI()
	{

		for (int i = 0; i < _spawnArea.Nodes.Count; ++i)
		{
			var node = _spawnArea.Nodes [i];
			Vector3 diff = _spawnArea.transform.position - _areaLastPosition;
			node.position += diff;	
		}


		for(int i = 0; i < _spawnArea.Nodes.Count; ++i)
		{
			Handles.color = Color.yellow;

			var node = _spawnArea.Nodes [i];
			if(Handles.Button (node.position, Quaternion.identity, .1f, .1f, Handles.DotHandleCap))
			{
				_selectedNode = i;
				break;
			}
		}

		if(_selectedNode != -1)
		{
			var node = _spawnArea.Nodes [_selectedNode];
			node.position = Handles.PositionHandle (node.position, Quaternion.identity);
		}


		ProcessEvent ();

		Repaint ();
		EditorUtility.SetDirty(target);

		_areaLastPosition = _spawnArea.transform.position;
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
		}

		if(curEvent.type == EventType.KeyDown)
		{
			if(curEvent.keyCode == KeyCode.Delete)
			{
				_spawnArea.Nodes.RemoveAt (_selectedNode);
				_selectedNode = -1;
				GUIUtility.hotControl = 0;
				curEvent.Use ();
				_spawnArea.OnAreaChanged ();
			}

			if(curEvent.keyCode == KeyCode.Escape)
			{
				_selectedNode = -1;
				GUIUtility.hotControl = 0;
				curEvent.Use ();
				_spawnArea.OnAreaChanged ();
			}
		}
	}
}
