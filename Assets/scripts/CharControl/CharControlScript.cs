using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(CapsuleCollider))]
[RequireComponent(typeof(CharInputController))]
public class CharControlScript : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rbody;
    private CharInputController cinput;
    private int groundContactCount = 0;

    float _inputForward = 0f;
    float _inputTurn = 0f;

    public bool IsGrounded
    {
        get
        {
            return groundContactCount > 0;
        }
    }

    public float jumpableGroundNormalMaxAngle = 45f;
    public bool closeToJumpableGround;
    public float animationSpeed = 1f;
    public float rootMovementSpeed = 1f;
    public float rootTurnSpeed = 1f;



    void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.Log("Animator could not be found");

        rbody = GetComponent<Rigidbody>();
        if (rbody == null)
            Debug.Log("Rigid body could not be found");

        cinput = GetComponent<CharInputController>();
        if (cinput == null)
            Debug.Log("CharInputController could not be found");

        anim.applyRootMotion = true;
    }


    void Start()
    {

    }


    void Update()
    {
        if (cinput.enabled)
        {
            _inputForward = cinput.Forward;
            _inputTurn = cinput.Turn;
        }
    }

    void FixedUpdate()
    {
        bool isGrounded = IsGrounded || CharacterCommon.CheckGroundNear(this.transform.position, jumpableGroundNormalMaxAngle, 0.85f, 0f, out closeToJumpableGround);


        anim.SetFloat("velx", _inputTurn);
        anim.SetFloat("vely", _inputForward);
        anim.SetBool("isFalling", !isGrounded);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == "ground")
        {
            ++groundContactCount;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.gameObject.tag == "ground")
        {
            --groundContactCount;
        }
    }

    void OnAnimatorMove()
    {

        Vector3 newRootPosition;
        Quaternion newRootRotation;

        bool isGrounded = IsGrounded || CharacterCommon.CheckGroundNear(this.transform.position, jumpableGroundNormalMaxAngle, 0.85f, 0f, out closeToJumpableGround);

        if (isGrounded)
        {      
            newRootPosition = anim.rootPosition;
        }
        else
        {
            newRootPosition = new Vector3(anim.rootPosition.x, this.transform.position.y, anim.rootPosition.z);
        }

        newRootRotation = anim.rootRotation;

        Vector3 scaledPosition = Vector3.LerpUnclamped(this.transform.position, newRootPosition, rootMovementSpeed);
        Quaternion scaledRotation = Quaternion.LerpUnclamped(this.transform.rotation, newRootRotation, rootTurnSpeed);

        rbody.MovePosition(scaledPosition);
        rbody.MoveRotation(scaledRotation);


    }
}