using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundAligner : MonoBehaviour
{

	Transform _tr;
    Rigidbody _rigidBody;
    CapsuleCollider _bodyCollider;
    RaycastHit[] _groundHitInfos = new RaycastHit[2]; // Two raycasts, one for back and another for front

	[SerializeField] string[] _layerMaskChecks = null;


    // Use this for initialization
    void Start()
    {
        _tr = transform;
        _rigidBody = GetComponent<Rigidbody>();
        _bodyCollider = GetComponent<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        AlignOnGround();
    }

    void AlignOnGround()
    {
        const float ground_check_dist = 10;

		int layerMask = LayerMaskUtil.GetLayerMask(0);//Default layer mask
		foreach (var maskName in _layerMaskChecks) {
			if (string.IsNullOrEmpty (maskName))
				continue;
			layerMask |= LayerMaskUtil.GetLayerMask (maskName);
		}

        Vector3 bodyDir = _rigidBody.rotation * Vector3.forward;
        Vector3 centerWordPos = _tr.position + _bodyCollider.center;

        bool invalidGround = false;

        //Casts 2 rays, back and front both directed to down, then calculates average slope normal
        for (int c = 0; c < 2; ++c)
        {
            float vecSign = c == 0 ? -1 : 1;
            Vector3 origin = (centerWordPos + bodyDir * (_bodyCollider.height * .5f + .02f) * vecSign);
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(origin, new Vector3(0, -1, 0), out hitInfo, ground_check_dist, layerMask);
            if (!hit)
            {
                invalidGround = true;
                break;
            }
            _groundHitInfos[c] = hitInfo;
        }

        Vector3 normalAlignVec = Vector3.up;

        if (!invalidGround)
        {
            // average slope normal
            normalAlignVec = _groundHitInfos[0].normal + _groundHitInfos[1].normal;
            normalAlignVec.Normalize();
        }

//		//Align solution 1, auto gravity. gliding on surface causes move and smooth rotation, issues on complex surfaces
		_rigidBody.rotation = Quaternion.LookRotation(_tr.forward, normalAlignVec);

		//Align solution 2, manual gravity. needs to be managed gravity someway
        Vector3 forwardVec = _rigidBody.rotation * Vector3.forward;
        Vector3 projectionVec = forwardVec - (Vector3.Dot(forwardVec, normalAlignVec)) * normalAlignVec;
//        _rigidBody.transform.rotation = Quaternion.LookRotation(projectionVec, normalAlignVec);

    }
}
