using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ObjReplacingChild : MonoBehaviour
{
    bool isOn = false;

    //�ڽ��� ������ ���� �ݶ��̴� Combine
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

        //�θ� ������ ������ �ޱ� ������ �θ��� ������ ��ŭ ���� �ش�. 
        //�θ��� ������ x, y, z ��� �����ϱ� ������ x�� ������� ������.
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

    #region OnCollision �̺�Ʈ
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
