using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]
[RequireComponent (typeof(Animator))]

public class CharacterAnimBasedMovement : MonoBehaviour
{
    public float rotationSpeed = 4f;
    public float rotationThreshold = 0.3f;
    [Range(0, 180f)]
    public float degreesToTurn = 160f;

    [Header("Animation Parameters")]
    public string motionParam = "motion";
    public string mirrorIdleParam = "mirrorIdle";
    public string turn180Param = "turn180";
    public string jumpParam = "jump";
    public string idleParam = "idle_bool";
    public string idletypeParam = "idle_type";
    public string stairParam = "stairs";

    [Header("Animation Smoothing")]
    [Range(0, 1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;


    public float Speed;
    private Vector3 desiredMoveDirection;
    private CharacterController characterController;
    private Animator animator;
    private bool mirrorIdle;
    private bool turn180;
    public bool speedlimiter;

    private float LastInputTime;
    private float IdleTimeSet=5f;
    public float RandomIdleType;

    // Start is called before the first frame update
    void Start()
    {
        characterController=GetComponent<CharacterController>();
        animator=GetComponent<Animator>();

        LastInputTime = Time.time;
        RandomIdle();
        
    }
    private void Update()
    {
        //Reset Idle Timer if any key is pressed at any time
        if (Input.anyKey)
        {
            RandomIdle();
            LastInputTime = Time.time;
            animator.SetBool(idleParam,false);
            
        }
        if(Time.time - LastInputTime > IdleTimeSet)
        {
            animator.SetFloat(idletypeParam, RandomIdleType);
            animator.SetBool(idleParam,true);

        }
    }
    private void RandomIdle()
    {
        RandomIdleType = UnityEngine.Random.Range(0, 3);
    }

    public void moveCharacter(float hInput, float vInput, Camera cam, bool jump, bool dash, bool walk)
    {
        if (walk)
        {
            speedlimiter = !speedlimiter;
        }

        if (speedlimiter==true)
        {
            hInput = hInput / 2;
            vInput = vInput / 2;
            Speed = new Vector2(hInput, vInput).sqrMagnitude;
        }
        else
        {
            Speed = new Vector2(hInput, vInput).normalized.sqrMagnitude;
        }
        

        //Calculate Input Magnitude
       
        //Sprint makes player get to max speed
        if (Speed >= Speed - rotationThreshold && dash)
        {
            Speed = 1.5f;
        }
        //Moves player
        if (Speed > rotationThreshold)
        {
            animator.SetFloat(motionParam, Speed, StartAnimTime, Time.deltaTime);
            Vector3 forward = cam.transform.forward;
            Vector3 right=cam.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            //Rotates player depending on camera position
            desiredMoveDirection = forward*vInput + right * hInput;

            if (Vector3.Angle(transform.forward, desiredMoveDirection) >= degreesToTurn)
            {
                turn180 = true;
            }
            else
            {
                turn180 = false;
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                Quaternion.LookRotation(desiredMoveDirection),
                                                rotationSpeed * Time.deltaTime);
            }

            //180 turning
            animator.SetBool(turn180Param, turn180);
            //Move the character
            animator.SetFloat(motionParam, Speed, StartAnimTime, Time.deltaTime);
        }
        else if(Speed < rotationThreshold)
        {
            animator.SetBool(mirrorIdleParam, mirrorIdle);
            animator.SetFloat(motionParam, Speed, StopAnimTime, Time.deltaTime);
        }
        //Jump motion
            animator.SetBool(jumpParam, jump);

    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (Speed < rotationThreshold) return;

        float distanceToLeftFoot=Vector3.Distance(transform.position,animator.GetIKPosition(AvatarIKGoal.LeftFoot)); 
        float distanceToRightFoot=Vector3.Distance(transform.position,animator.GetIKPosition(AvatarIKGoal.RightFoot));

        //Right foot in front
        if (distanceToRightFoot > distanceToLeftFoot)
        {
            mirrorIdle = true;  
        }
        //Right foor behind
        else
        {
            mirrorIdle = false;
        }
    }
    private void OnTriggerStay(Collider collider)
    {
        if (collider.tag == "stairs")
        {
            Speed = Speed * 2;
            animator.SetBool(stairParam, true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "stairs")
        {
            animator.SetBool(stairParam, false);
        }
    }
}
