using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TouchChecks : MonoBehaviour
{
    Material[] targetMaterial;
    bool isTouch;

    private void Awake()
    {
        isTouch = false;
        targetMaterial = transform.GetComponent<MeshRenderer>().materials;
    }

    //private void Update()
    //{
    //    TouchOk();
    //}

    //public bool TouchOk()
    //{
    //    Collider[] colliders = Physics.OverlapSphere(transform.position, 0.8f);

    //    if (colliders.Length > 1)
    //    {
    //        for (int i = 0; i < colliders.Length; i++)
    //        {
    //            if (colliders[i].name.Contains("finger"))
    //            {
    //                isTouch = true;
    //                return isTouch;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        return isTouch;
    //    }
    //}

}
