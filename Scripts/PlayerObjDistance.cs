using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjDistance : MonoBehaviour
{
    //public float standardHeight = 1.5f;

    float distance; //�Ÿ�
    float direction; // ����
    //private Transform target;

    private RaycastHit hit;
    private float maxDistance = 300f;

    public bool DistanceComparison(Transform target, Vector3 targetVector)
    {
        bool isTelePort = false;
        Transform playerCamera = transform.Find("SteamVRObjects/VRCamera"); ;
        //1. �� ������Ʈ ���� �Ÿ� ��� 
        distance = Vector3.Distance(playerCamera.position, targetVector);
        Debug.Log("�Ÿ� : " + distance);
        Debug.Log("Ÿ�� : " + target.name, target.gameObject);

        if (distance >= 7f) //2. Ư�� �Ÿ� �̻��� ��� (�ٸ� ���̵� ���� ���̵� ������ �̵�)
        {
            isTelePort = true;
        }
        else //3.���� �Ÿ� �̻��� ���� ������ �ٸ� ���� ���ɼ��� �ֱ⿡ �˻�
        {
            //4. �� ������Ʈ ���� ���� ���
            if ((playerCamera.position - target.position).y <= 0f) //6. ������ ��� Target�� ���� �ִٴ� ��
            {
                //7. �׷��ٸ� ���� Ray�� ���� Player�� Floor�� ���̸� ���Ѵ�.(8���� ��(floor)�� �ǹ�)
                if (Physics.Raycast(playerCamera.position, playerCamera.up, out hit, Mathf.Infinity/*maxDistance*/, 1 << LayerMask.NameToLayer("Floor")))
                {
                    //8. ���� Ray�� ���̿� Target ��ġ�� ���̸� �� 
                    if (hit.distance < Mathf.Abs((playerCamera.position - target.position).y)) //��ü�� �� �ʸӿ� �ִٴ� ��
                    {
                        isTelePort = true;
                    }
                }
            }
            else //7. ����� ��� Target�� �Ʒ��� �ִٴ� ��
            {
                if (Physics.Raycast(playerCamera.position, -playerCamera.up, out hit, maxDistance, 1 << LayerMask.NameToLayer("Floor")))
                {
                    if (hit.distance < Mathf.Abs((playerCamera.position - target.position).y)) //��ü�� �� �ʸӿ� �ִٴ� ��
                        isTelePort = true;
                }
            }
        }
        return isTelePort;
    }



    //private void Update()
    //{
    //    SetPlayerHeight();
    //}
    //������ Floor�� ������ǥy�� Player ��ǥ�� ����, Ű�� ���� ���� �������� ����
    public float playerHeight;
    public float floorHeight;
    public Transform currentFloor;
    public void SetPlayerHeight(Vector3 tg_Bounds)
    {
        Transform floor = currentFloor = MinimapManager.instance.FloorCheck(tg_Bounds);

        floorHeight = floor.position.y;

        Vector3 playerPos = transform.position;
        playerPos.y = floorHeight;
        transform.position = playerPos;

        playerHeight = transform.position.y;
        //if (floor)
        //{
        //    if (currentFloor != floor)
        //    {
        //        floorHeight = floor.position.y;

        //        Vector3 playerPos = transform.position;
        //        playerPos.y = floorHeight;
        //        transform.position = playerPos;
        //    }
        //}
        //currentFloor = floor;
        //Vector3 camPos = Camera.main.transform.position;
        //float heightFromFloor = camPos.y - detectedFloorHeight; //������ �ٴ����� ������ �Ÿ�
        //Vector3 playerPos = transform.position;
        //float diff = heightFromFloor - standardHeight; //���ذ� ���� Ű�� ����
        //if (diff > 0) //���� �Ÿ����� ũ��
        //    playerPos.y -= diff; // �� ���׸�ŭ Player ������
        //else if (diff < 0)
        //    playerPos.y += diff; // �� ���׸�ŭ Player �ø���
        //playerHeight = playerPos.y;
        //transform.position = playerPos;
    }
}