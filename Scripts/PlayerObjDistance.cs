using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjDistance : MonoBehaviour
{
    //public float standardHeight = 1.5f;

    float distance; //거리
    float direction; // 방향
    //private Transform target;

    private RaycastHit hit;
    private float maxDistance = 300f;

    public bool DistanceComparison(Transform target, Vector3 targetVector)
    {
        bool isTelePort = false;
        Transform playerCamera = transform.Find("SteamVRObjects/VRCamera"); ;
        //1. 두 오브젝트 간의 거리 계산 
        distance = Vector3.Distance(playerCamera.position, targetVector);
        Debug.Log("거리 : " + distance);
        Debug.Log("타겟 : " + target.name, target.gameObject);

        if (distance >= 7f) //2. 특정 거리 이상일 경우 (다른 층이든 같은 층이든 무조건 이동)
        {
            isTelePort = true;
        }
        else //3.일정 거리 이상은 되지 않으나 다른 층일 가능성이 있기에 검사
        {
            //4. 두 오브젝트 간의 높이 계산
            if ((playerCamera.position - target.position).y <= 0f) //6. 음수일 경우 Target은 위에 있다는 것
            {
                //7. 그렇다면 위로 Ray를 쏴서 Player와 Floor의 길이를 구한다.(8번은 벽(floor)을 의미)
                if (Physics.Raycast(playerCamera.position, playerCamera.up, out hit, Mathf.Infinity/*maxDistance*/, 1 << LayerMask.NameToLayer("Floor")))
                {
                    //8. 구한 Ray의 길이와 Target 위치의 높이를 비교 
                    if (hit.distance < Mathf.Abs((playerCamera.position - target.position).y)) //물체가 층 너머에 있다는 것
                    {
                        isTelePort = true;
                    }
                }
            }
            else //7. 양수일 경우 Target은 아래에 있다는 것
            {
                if (Physics.Raycast(playerCamera.position, -playerCamera.up, out hit, maxDistance, 1 << LayerMask.NameToLayer("Floor")))
                {
                    if (hit.distance < Mathf.Abs((playerCamera.position - target.position).y)) //물체가 층 너머에 있다는 것
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
    //감지된 Floor의 월드좌표y를 Player 좌표로 대입, 키에 따른 높이 상이함을 보정
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
        //float heightFromFloor = camPos.y - detectedFloorHeight; //감지된 바닥으로 부터의 거리
        //Vector3 playerPos = transform.position;
        //float diff = heightFromFloor - standardHeight; //기준과 실제 키의 편차
        //if (diff > 0) //기준 거리보다 크면
        //    playerPos.y -= diff; // 그 차액만큼 Player 내리기
        //else if (diff < 0)
        //    playerPos.y += diff; // 그 차액만큼 Player 올리기
        //playerHeight = playerPos.y;
        //transform.position = playerPos;
    }
}