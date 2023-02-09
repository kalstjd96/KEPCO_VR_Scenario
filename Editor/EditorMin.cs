using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorMin : MonoBehaviour
{
    public List<Transform> phoneList;

    #region Phone BoxCollider
    [MenuItem("Editor/Collider Update %t")]
    static void ColliderUpdate()
    {
        Transform selectObject = Selection.activeGameObject.transform;

        foreach (Transform item in selectObject.GetComponentsInChildren<Transform>())
        {
            if (item.name.Equals("receiver"))
            {
                Debug.Log("전화기 : " + item,  item);
                item.GetComponent<BoxCollider>().isTrigger = true;
                //if (item.GetComponent<MeshCollider>())
                //    DestroyImmediate(item.GetComponent<MeshCollider>());
                //if (!item.GetComponent<BoxCollider>())
                //{
                //    item.gameObject.AddComponent<BoxCollider>();
                //    item.GetComponent<BoxCollider>().size = new Vector3(item.GetComponent<BoxCollider>().size.x * 1.5f,
                //        item.GetComponent<BoxCollider>().size.y * 1.5f, item.GetComponent<BoxCollider>().size.z * 1.5f);
                //}
            }
        }
        Debug.Log("완료");
    }

    #endregion

}
