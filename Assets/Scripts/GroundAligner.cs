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

	bool _frontIsStair;
	public bool FrontIsStair{ get { return _frontIsStair; } }


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
			bool hit = Physics.Raycast(origin, Vector3.down, out hitInfo, ground_check_dist, layerMask);
            if (!hit)
                break;
            _groundHitInfos[c] = hitInfo;
        }


		bool groundHit = _groundHitInfos [0].collider != null && _groundHitInfos [1].collider != null;

		_frontIsStair = false;
		if(groundHit)
		{
			Vector3 origin = centerWordPos + bodyDir * (_bodyCollider.height * .5f + .03f) + new Vector3 (0, -_bodyCollider.radius * .3f, 0);
			RaycastHit hitInfo;
			Vector3 forwardDir = _rigidBody.rotation * Vector3.forward;
			bool hit = Physics.Raycast(origin, forwardDir, out hitInfo, .15f, layerMask);
			//Debug.DrawRay (origin, forwardDir * .15f);
			if (hit)
			{
				_frontIsStair = Mathf.Abs (hitInfo.normal.y) < .1f;
				Debug.Log ("Hit stair");
			}
		}

		Vector3 upwardVector = Vector3.up;

		if (groundHit)
        {
            upwardVector = _groundHitInfos[0].normal + _groundHitInfos[1].normal;
            upwardVector.Normalize();
        }

		Vector3 forwardVec = _rigidBody.rotation * Vector3.forward;
		_rigidBody.rotation = Quaternion.LookRotation (forwardVec, upwardVector);
    }
}
