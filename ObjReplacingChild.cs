using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ObjReplacingChild : MonoBehaviour
{
    bool isOn = false;

    //자신을 포함한 하위 콜라이더 Combine
    private void Start()
    {
        ColliderCombine();
    }

    public void ColliderCombine()
    {
        Collider targetCol = transform.GetComponentInChildren<Collider>();
        Bounds totalBounds = targetCol.bounds;

        Collider[] colliders = transform.GetComponentsInChildren<Collider>();
        int index = 0;

        foreach (Collider col in colliders)
        {
            if (index != 0)
                totalBounds.Encapsulate(col.bounds);

            index++;
        }

        if (!transform.GetComponent<BoxCollider>())
            transform.gameObject.AddComponent<BoxCollider>();

        //부모 사이즈 영향을 받기 때문에 부모의 스케일 만큼 나눠 준다. 
        //부모의 스케일 x, y, z 모두 동일하기 때문에 x를 대상으로 나눈다.
        gameObject.GetComponent<BoxCollider>().size = totalBounds.size/(float)transform.parent.localScale.x;
    }

    private void Update()
    {
        if (isOn)
        {
            if (SteamVR_Actions.default_InteractUI.GetStateDown(SteamVR_Input_Sources.Any))
                ObjReplacing.instance.CompeleteCheck(true);
        }
    }

    #region OnCollision 이벤트
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactive"))
        {
            isOn = true;
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactive"))
        {
            isOn = false;
        }
    }
    #endregion
}
