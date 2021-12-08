using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCastDrawer : MonoBehaviour
{
    RaycastHit hit;
    private int layerMask = 1 << 6;

    [SerializeField] private bool isEnable = false;

    private void OnDrawGizmos()
    {
        if (!isEnable) return;
        
        var radius = 0.2f;
        var positionOffset = Vector3.up * 0.19f;
        
        var isHit = Physics.CheckSphere(transform.position + positionOffset, radius, layerMask);
        if (isHit)
        {
            Gizmos.DrawWireSphere(transform.position + positionOffset, radius);
        }
        else
        {
            Gizmos.DrawRay (transform.position + positionOffset, -transform.up * 0.1f);
        }
    }
}