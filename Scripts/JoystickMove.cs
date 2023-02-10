using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

public class JoystickMove : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform hand;
    [SerializeField] SteamVR_Input_Sources input_Sources = SteamVR_Input_Sources.RightHand;
    public bool freezeY = true;
    public bool freezeMove = false;
    public float speed;
    Vector3 moveDirection;
    public float standardY;

    /*void FixedUpdate()
    {
        if (!freezeMove)
        {
            if (SteamVR_Actions.default_OJoystickPosition.GetAxis(input_Sources) != Vector2.zero)
            {
                Vector2 direction = SteamVR_Actions.default_OJoystickPosition.GetAxis(input_Sources);
                moveDirection = GetMoveForward(hand.forward, hand.right, direction);
                Vector3 vector3 = player.position + moveDirection * speed * Time.fixedDeltaTime;
                if (freezeY)
                    vector3.y = standardY;

                player.position = vector3;
            }
            else standardY = player.position.y;



            if (SteamVR_Actions.default_Teleport.GetState(input_Sources))
            {
                if (SteamVR_Actions.default_VJoystickPosition.GetAxis(input_Sources) != Vector2.zero)
                {
                    Vector2 direction = SteamVR_Actions.default_VJoystickPosition.GetAxis(input_Sources);
                    moveDirection = GetMoveForward(hand.forward, hand.right, direction);
                    Vector3 vector3 = player.position + moveDirection * speed * Time.fixedDeltaTime;
                    if (freezeY)
                        vector3.y = standardY;
                    player.position = vector3;
                }
            }
            else if (SteamVR_Actions.default_VJoystickPosition.GetAxis(input_Sources) == Vector2.zero)
            {
                standardY = player.position.y;
            }

        }
    }*/

    private void Start()
    {
        if (freezeY)
            standardY = player.position.y;
    }

    //2022-12-27 YH 수정 : 미끄러지는 현상이 계속 발생.
    void Update()
    {
        if (!freezeMove)
        {
            #region Oculus 환경
            if (SteamVR_Actions.default_TeleportFront.GetState(input_Sources) || SteamVR_Actions.default_TeleportBack.GetState(input_Sources) 
                || SteamVR_Actions.default_TeleportLeft.GetState(input_Sources) || SteamVR_Actions.default_TeleportRight.GetState(input_Sources))
            {
                standardY = player.position.y;

                if (SteamVR_Actions.default_OJoystickPosition.GetAxis(input_Sources) != Vector2.zero)
                {
                    Vector2 direction = SteamVR_Actions.default_OJoystickPosition.GetAxis(input_Sources);
                    moveDirection = GetMoveForward(hand.forward, hand.right, direction);
                    Vector3 vector3 = player.position + moveDirection * speed * Time.deltaTime;
                    player.position = vector3;
                }

                return;

            }
            #endregion

            #region Vive 환경
            if (SteamVR_Actions.default_Teleport.GetState(input_Sources))
            {
                standardY = player.position.y;

                if (SteamVR_Actions.default_VJoystickPosition.GetAxis(input_Sources) != Vector2.zero)
                {
                    Vector2 direction = SteamVR_Actions.default_VJoystickPosition.GetAxis(input_Sources);
                    moveDirection = GetMoveForward(hand.forward, hand.right, direction);
                    Vector3 vector3 = player.position + moveDirection * speed * Time.fixedDeltaTime;
                    player.position = vector3;
                }

                return;
            }
            #endregion

        }
    }

    Vector3 GetMoveForward(Vector3 forward, Vector3 right, Vector2 direction)
    {
        Vector3 moveForward = Vector3.zero;

        //forward = Vector3.ProjectOnPlane(forward, Vector3.up);

        moveForward = (forward * direction.y) + (right * direction.x);
        moveForward.Normalize();
        if (freezeY)
            moveForward.y = 0f;

        return moveForward;
    }
}
