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

    // ���ϸ��̼�
    bool isWaiting = false;

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

        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        Inputs();
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        //���ϸ��̼�
        animator.SetBool("isGrounded", isGrounded);

        // �߷°� ����
        if (verticalMomentum > gravityValue)
            verticalMomentum += Time.fixedDeltaTime * gravityValue;

        // ���� ����Ʈ�� ������ �޸��� �ӵ���, �ƴϸ� �Ϲ� �ӵ��� ����.
        if (isSprinting) velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * runSpeed;
        else velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        if (vertical > 0) animator.SetFloat("speed", 1);
        else animator.SetFloat("speed", 0);

        // �߷°�
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back)) velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left)) velocity.x = 0;

        if (velocity.y < 0) velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0) velocity.y = checkUpSpeed(velocity.y);
    }

    private void Inputs()
    {

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
     

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

    // ���� �پҴ��� Ȯ���ϴ� �޼ҵ�
    private float checkDownSpeed(float downSpeed)
    {

        if (
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)
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

    // ��� ���� �ִ��� Ȯ���ϴ� �޼ҵ�
    private float checkUpSpeed(float upSpeed)
    {

        if (
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)
           )
        {

            return 0;

        }
        else
        {

            return upSpeed;

        }

    }

    public bool front
    {

        get
        {
            if (
                world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z + playerWidth) ||
                world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth)
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
                world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z - playerWidth) ||
                world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)
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
                world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z)
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
                world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z)
                )
                return true;
            else
                return false;
        }

    }

    IEnumerator RestWaiting()
    {
        yield return new WaitForSeconds(0.1f);
        animator.ResetTrigger("jump");
        isWaiting = false;
    }
}
