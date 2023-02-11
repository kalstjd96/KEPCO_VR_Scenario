using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjDistance : MonoBehaviour
{
    float distance; //거리
    Vector3 direction; // 방향
    public const float maxDistance = 7f; 

    private RaycastHit hit;
    private float maxRaycast = 300f;

    /// <summary>
    /// Player와 Target 간의 거리 비교 후 대상 위치로 텔레포트
    /// </summary>
    /// <param name="target">해당 절차에 처리해야할 대상</param>
    /// <param name="targetVector">처리해야할 대상의 RendererCenter</param>
    /// <returns>텔레포트 해야할 지에 대한 여부 </returns>
    public bool DistanceComparison(Transform target, Vector3 targetVector)
    {
        //Transform playerCamera = transform.Find("SteamVRObjects/VRCamera"); ;
        Transform playerCamera = transform;
        Vector3 reSetting = playerCamera.position;
        distance = Vector3.Distance(playerCamera.position, targetVector);

        if (distance >= maxDistance)
            return true;
        else
        {
            if ((playerCamera.position - target.position).y <= 0f)
                direction = playerCamera.up;
            else
                direction = -playerCamera.up; 

            reSetting.y += 0.1f;
            if (Physics.Raycast(reSetting, direction, out hit, maxRaycast, 1 << LayerMask.NameToLayer("Floor")))
            {
                if (hit.distance < Mathf.Abs((playerCamera.position - target.position).y))
                    return true;
            }
        }
        return false;
    }

    //감지된 Floor의 월드좌표y를 Player 좌표로 대입, 키에 따른 높이 상이함을 보정
    public float playerHeight;
    public float floorHeight;
    public Transform currentFloor;
    public void SetPlayerHeight()
    {
        Transform floor = currentFloor = MinimapManager.instance.FloorCheck();

        floorHeight = floor.position.y;

        Vector3 playerPos = transform.position;
        playerPos.y = floorHeight;
        transform.position = playerPos;

        playerHeight = transform.position.y;
    }
}
