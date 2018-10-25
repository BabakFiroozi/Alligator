using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(SpawnArea), true)]
[CanEditMultipleObjects]
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
			SetFirstNodes ();
		}
	}

	void OnEnable()
	{
		radiusProp = serializedObject.FindProperty ("Radius");

		_areaLastPosition = _spawnArea.transform.position;

//		Tools.hidden = true;
//		textStyle = new GUIStyle();
//		textStyle.alignment = TextAnchor.MiddleCenter;
//		textStyle.normal.textColor = Color.white;
	}

	void SetFirstNodes()
	{
		_spawnArea.Nodes.Clear ();
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
		_selectedNode = -1;

		EditorUtility.SetDirty(target);
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

		GUILayout.Space (10);
		if( GUILayout.Button( "Reset", GUILayout.Width( 100f ), GUILayout.Height( 30f ) ) )
		{
			SetFirstNodes ();
		}
		GUILayout.Space (10);

		serializedObject.ApplyModifiedProperties();
	}

	[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
	static void DrawGizmo(SpawnArea spawnArea, GizmoType gizmoType)
	{
		var gizmoColor = Gizmos.color;
		Gizmos.color = Color.yellow;
		for (int i = 0; i < spawnArea.Nodes.Count; ++i)
		{
			var node1 = spawnArea.Nodes [i];
			var node2 = spawnArea.Nodes [(i + 1) % spawnArea.Nodes.Count];
			Gizmos.DrawLine (node1.position, node2.position);
		}
		Gizmos.color = gizmoColor;
	}

	protected virtual void OnSceneGUI()
	{
		Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

		var handleColor = Handles.color;

		const float rectHeight = 4;

		for(int i = 0; i < _spawnArea.Nodes.Count; ++i)
		{
			var node = _spawnArea.Nodes [i];

			Vector3 diff = _spawnArea.transform.position - _areaLastPosition;
			node.position += diff;	

			Handles.color = Color.yellow;
			if(Handles.Button (node.position, Quaternion.identity, .3f, .3f, Handles.SphereHandleCap))
			{
				_selectedNode = i;
				break;
			}

			var nextNode = _spawnArea.Nodes [(i + 1) % _spawnArea.Nodes.Count];
			Vector3 toNextNodeDir = nextNode.position - node.position;
			Vector3 pointPos = node.position + toNextNodeDir * .5f;
			Handles.color = Color.green;
			if(Handles.Button (pointPos, Quaternion.identity, .2f, .2f, Handles.SphereHandleCap))
			{
				var newNode = new SpawnAreaNode ();
				newNode.position = pointPos + new Vector3 (-toNextNodeDir.z, 0, toNextNodeDir.x).normalized * .5f;
				newNode.area = _spawnArea;
				int insertIndex = i + 1;
				_spawnArea.Nodes.Insert(insertIndex, newNode);
				_selectedNode = insertIndex;
				break;
			}

			Vector3[] vertsArr = new Vector3[4];
			vertsArr [0] = node.position - new Vector3 (0, rectHeight / 2, 0);
			vertsArr [1] = nextNode.position - new Vector3 (0, rectHeight / 2, 0);
			vertsArr [2] = nextNode.position + new Vector3 (0, rectHeight / 2, 0);
			vertsArr [3] = node.position + new Vector3 (0, rectHeight / 2, 0);
			Handles.DrawSolidRectangleWithOutline (vertsArr, new Color (0, 1, .5f, .1f), new Color (.5f, 1, 1, .5f));
		}

		if(_selectedNode != -1)
		{
			var node = _spawnArea.Nodes [_selectedNode];
			float nodePosY = node.position.y;
			Vector3 handlePos = Handles.PositionHandle (node.position, Quaternion.identity);
			handlePos.y = nodePosY;
			node.position = handlePos;
		}

		Handles.color = handleColor;

		ProcessEvent ();

		Undo.RecordObject(target, "Path");

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
				if(_spawnArea.Nodes.Count > 3)
				{
					_spawnArea.Nodes.RemoveAt (_selectedNode);
					_selectedNode = -1;
					GUIUtility.hotControl = 0;
					curEvent.Use ();
					_spawnArea.OnAreaChanged ();
				}
			}

			if(curEvent.keyCode == KeyCode.Escape)
			{
				_selectedNode = -1;
				GUIUtility.hotControl = 0;
				curEvent.Use ();
				_spawnArea.OnAreaChanged ();
			}
		}

		Repaint ();
	}
}
