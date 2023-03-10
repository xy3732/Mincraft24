using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class Character : MonoBehaviour
{
    private Transform cam;

    public bool isGrounded;
    public bool isSprinting;
    [Space(20)]

    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float jumpForce = 5f;
    public float gravityValue = -25f;
    [Space(20)]

    public float playerWidth = 0.3f;
    [Space(20)]

    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8f;

    private float horizontal;
    private float vertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;
    [Space(20)]

    //UI
    public byte selectedBlockIndex = 0;

    // 에니메이션
    bool isWaiting = false;
    [Space(20)]
    private World world;
    public Animator animator;

    private void Start()
    {
        cam = GameObject.Find("MainCamera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        Cursor.lockState = CursorLockMode.Locked;
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
        placeCursorBlocks();
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


        if(highlightBlock.gameObject.activeSelf)
        {
            // 블럭 파괴
            if(Input.GetMouseButtonDown(0))
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
            }

            //블럭 설치
            if(Input.GetMouseButtonDown(1))
            {
                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, selectedBlockIndex);
            }
        }
    }

    //현재 마우스커서 위에 있는 블럭 확인하는 메소드.
    private void placeCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while(step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);

            if(world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

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
