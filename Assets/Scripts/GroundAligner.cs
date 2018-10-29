using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundAligner : MonoBehaviour
{
	Transform _tr;
    Rigidbody _rigidbody;
    CapsuleCollider _bodyCollider;
    RaycastHit[] _groundHitInfos = new RaycastHit[2]; // Two raycasts, one for back and another for front

	[SerializeField] string[] _layerMaskChecks = null;

	bool _frontIsStair;
	public bool FrontIsStair{ get { return _frontIsStair; } }

	bool _isOnGround;
	float _bodylength;

	public bool IsOnGround	{ get { return _isOnGround; } }

	public float Bodylength	{ get { return _bodylength; } }


    // Use this for initialization
    void Start()
    {
        _tr = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _bodyCollider = GetComponent<CapsuleCollider>();

		_bodylength = _bodyCollider.height * .5f;
    }

    void FixedUpdate()
    {
        AlignOnGround();
    }

    void AlignOnGround()
    {
        const float ground_check_dist = 100;

		int layerMask = LayerMaskUtil.GetLayerMask(0);//Default layer mask
		foreach (var maskName in _layerMaskChecks) {
			if (string.IsNullOrEmpty (maskName))
				continue;
			layerMask |= LayerMaskUtil.GetLayerMask (maskName);
		}

        Vector3 bodyDir = _rigidbody.rotation * Vector3.forward;
        Vector3 centerWordPos = _tr.position + _bodyCollider.center;

        //Casts 2 rays, back and front both directed to down, then calculates average slope normal
        for (int c = 0; c < 2; ++c)
        {
            float vecSign = c == 0 ? -1 : 1;
			Vector3 origin = (centerWordPos + bodyDir * (_bodylength + .04f) * vecSign);
            RaycastHit hitInfo;
			bool hit = Physics.Raycast(origin, Vector3.down, out hitInfo, ground_check_dist, layerMask);
            if (!hit)
                break;
            _groundHitInfos[c] = hitInfo;
        }


		bool validGround = _groundHitInfos [0].collider != null && _groundHitInfos [1].collider != null;

		_frontIsStair = false;
		if(validGround)
		{
			Vector3 origin = centerWordPos + bodyDir * (_bodylength + .02f) + new Vector3 (0, -_bodyCollider.radius * .35f, 0);
			RaycastHit hitInfo;
			Vector3 forwardDir = _rigidbody.rotation * Vector3.forward;
			bool hit = Physics.Raycast(origin, forwardDir, out hitInfo, .2f, layerMask);
			_frontIsStair = hit && Mathf.Abs (hitInfo.normal.y) < .15f;
//			Debug.DrawRay (origin, forwardDir * .2f);
		}

		Vector3 upwardVector = Vector3.up;

		if (validGround)
        {
            upwardVector = _groundHitInfos[0].normal + _groundHitInfos[1].normal;
            upwardVector.Normalize();
        }

		_isOnGround = true;
		RaycastHit[] hitInfos = new RaycastHit[2];
		Physics.RaycastNonAlloc (_rigidbody.position, _rigidbody.rotation * Vector3.down, hitInfos, ground_check_dist);
		foreach(var hitInfo in hitInfos)
		{
			if (hitInfo.collider == null)
				continue;
			if (hitInfo.collider == _bodyCollider)
				continue;
			if(hitInfo.distance > _bodyCollider.radius + .03f)
			{
//				Debug.Log ("in air");
				_isOnGround = false;
				break;
			}
		}

		//set damping on ground an in the air
		_rigidbody.drag = _isOnGround ? 5 : 0;
		_rigidbody.angularDrag = _isOnGround ? 5 : 0;

		Vector3 forwardVec = _rigidbody.rotation * Vector3.forward;
		_rigidbody.rotation = Quaternion.LookRotation (forwardVec, upwardVector);

		//We change only rotation, so we dont need angular velocity
		_rigidbody.angularVelocity = Vector3.zero;
    }
}
