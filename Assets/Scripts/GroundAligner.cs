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

        //Casts 2 rays, back and front both directed to down, then calculates average slope normal
        for (int c = 0; c < 2; ++c)
        {
            float vecSign = c == 0 ? -1 : 1;
            Vector3 origin = (centerWordPos + bodyDir * (_bodyCollider.height * .5f + .04f) * vecSign);
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(origin, new Vector3(0, -1, 0), out hitInfo, ground_check_dist, layerMask);
            if (!hit)
                break;
            _groundHitInfos[c] = hitInfo;
        }


		bool isOnGround = _groundHitInfos [0].collider != null && _groundHitInfos [1].collider != null;

		if(isOnGround)
		{
			Vector3 origin = centerWordPos + bodyDir * (_bodyCollider.height * .5f + .02f);
			RaycastHit hitInfo;
			Vector3 forwardDir = _rigidBody.rotation * Vector3.forward;
			bool hit = Physics.Raycast(origin, forwardDir, out hitInfo, .15f, layerMask);
			if (hit)
			{
				if (hitInfo.normal.y < .01f)
					_rigidBody.position += Vector3.up * .2f;// (Vector3.up * _rigidBody.mass * 10);
			}
		}

		Vector3 upwardVector = Vector3.up;

		if (isOnGround)
        {
            upwardVector = _groundHitInfos[0].normal + _groundHitInfos[1].normal;
            upwardVector.Normalize();
        }

		Vector3 forwardVec = _rigidBody.rotation * Vector3.forward;
		_rigidBody.rotation = Quaternion.LookRotation (forwardVec, upwardVector);
    }
}
