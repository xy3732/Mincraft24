using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;
    [Space(20)]

    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float jumpForce = 5f;
    public float gravityValue = -25f;
    [Space(20)]
    public float playerWidth = 0.3f;

    private float horizontal;
    private float vertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    // 에니메이션
    bool isWaiting = false;
    [Space(20)]
    private World world;
    public Animator animator;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    private void FixedUpdate()
    {
        CalculateVelocity();
        if (jumpRequest) Jump();

        // velocity값 플레이어 오브젝트에게 값 전달.
        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        // 키보드 인풋
        Inputs();
    }

    // 플레이어 물리 계산기
    // 콜라이더 없이 충돌 구현 완료.
    private void CalculateVelocity()
    {
        //에니메이션
        animator.SetBool("isGrounded", isGrounded);

        // 중력값 조정
        if (verticalMomentum > gravityValue)
            verticalMomentum += Time.fixedDeltaTime * gravityValue;

        // 왼쪽 쉬프트를 누르면 달리는 속도로, 아니면 일반 속도로 간다.
        if (isSprinting) velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * runSpeed;
        else velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        // velocity 값이 0보다 크면 뛰는 에니메이션으로 설정, 멈춰있으면 Idle로 다시 설정.
        if (vertical > 0) animator.SetFloat("speed", 1);
        else animator.SetFloat("speed", 0);

        // 중력값
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        // 옆에 블럭이 있는지 확인하는 IF문들.
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back)) velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left)) velocity.x = 0;

        // 아레에 블럭이 있는지 확인하는 if, 머리 위에 블럭이 있는지 확인하는 else if문.
        if (velocity.y < 0) velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0) velocity.y = checkUpSpeed(velocity.y);
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void Inputs()
    {

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
     

        // 왼쪽 쉬프트 누르고 있을시 달리는 속도로, 아니면 걷는 속도로 정하기.
        if (Input.GetKeyDown(KeyCode.LeftShift)) isSprinting = true;
        if (Input.GetKeyUp(KeyCode.LeftShift)) isSprinting = false;

        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && isWaiting == false)
        {
            animator.SetTrigger("jump");

            jumpRequest = true;
            isWaiting = true;

            StopAllCoroutines();
            StartCoroutine(RestWaiting());
        }

    }

    // 땅에 다았는지 확인하는 메소드
    private float checkDownSpeed(float downSpeed)
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
           )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    // 블력 옆에 있는지 확인하는 메소드
    private float checkUpSpeed(float upSpeed)
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
           ) return 0;
        else return upSpeed;
    }

    //font back right left - 블럭이 있는지 확인하는 문구.
    #region
    public bool front
    {

        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
                return true;
            else
                return false;
        }

    }


    public bool back
    {

        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
                return true;
            else
                return false;
        }

    }
    public bool left
    {

        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }
    public bool right
    {

        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }

    #endregion 

    // 에니메이션 초기화용 이너뮬레이터
    IEnumerator RestWaiting()
    {
        yield return new WaitForSeconds(0.1f);
        animator.ResetTrigger("jump");
        isWaiting = false;
    }
}
