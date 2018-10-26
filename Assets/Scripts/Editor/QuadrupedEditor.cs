using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(Quadruped), true)]
public class QuadrupedEditor : Editor {

	const float Border_Draw_Height = 4;
    Quadruped _territoryArea;


    void Awake()
	{
        _territoryArea = target as Quadruped;
	}

	public override void OnInspectorGUI()
	{
        base.OnInspectorGUI();
	}

	protected virtual void OnSceneGUI()
	{
		Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

		var handleColor = Handles.color;

        var areaNodes = _territoryArea.TerritoryArea.Nodes;

        for (int i = 0; i < areaNodes.Count; ++i)
		{
			var node = areaNodes[i];

			Handles.color = Color.yellow;

			var nextNode = areaNodes[(i + 1) % areaNodes.Count];
			Vector3 toNextNodeDir = nextNode.position - node.position;
			Vector3 addNodeHandlePos = node.position + toNextNodeDir * .5f;
			Handles.color = Color.green;

			Vector3[] vertsArr = new Vector3[4];
			vertsArr [0] = node.position - new Vector3 (0, Border_Draw_Height, 0);
			vertsArr [1] = nextNode.position - new Vector3 (0, Border_Draw_Height, 0);
			vertsArr [2] = nextNode.position;
			vertsArr [3] = node.position;
			Handles.DrawSolidRectangleWithOutline (vertsArr, new Color (0, 1, .5f, .1f), new Color (.5f, 1, 1, .5f));
		}
	}
}
