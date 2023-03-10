using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject musicPlayer;
    AudioManager audioMan;

    [Header("Sounds")]
    public AudioSource source;
    public AudioClip clip;
    public float volume;
    public float walkingPitch;
    public float runningPitch;


    [Header("Animation")]
    [SerializeField] Animator MyAnimator;

    [Header("Camera Stats")]
    [SerializeField] private Camera MyCamera;

    [Header("Player Stats")]
    [SerializeField] private CharacterController cc;
    [SerializeField] private float PlayerWalkSpeed = 2f;
    [SerializeField] private float PlayerRunSpeed = 4f;

    [Header("Camera Smoothing variables")]
    [SerializeField] private float desiredRotation = 0f;
    [SerializeField] private float RotationSpeed = 10f;

    [Header ("Gravity and Ground Check")]
    [SerializeField] private float YSpeed = 0f;
    [SerializeField] private float gravity = -9.81f;

    

    private void Start()
    {
        audioMan =GameObject.Find("GameManager").GetComponent<AudioManager>();
        //Stops cursor from moving around while playing the game
        Cursor.lockState = CursorLockMode.Locked;
        
    }
    private void Update()
    {
        if (!MyAnimator.GetCurrentAnimatorStateInfo(0).IsName("getHit"))
        {
            PlayerAttack();
            Move();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            source.enabled = true;
        }else source.enabled = false;

       if(other.gameObject.name == musicPlayer.name)
       {
           //Debug.Log(other.name);
           audioMan.StartMusic();
           other.gameObject.SetActive(false);
       }
    }

    private void Move()
    {
        //The idle animation will be true by default
        MyAnimator.SetBool("idle", true);
        MyAnimator.SetBool("walk", false);
        MyAnimator.SetBool("dodge", false);
        //Setting up Gravity
        if (cc.isGrounded)
        {
            YSpeed = 0;
        }
        else
        {
            YSpeed += gravity * Time.deltaTime;
        }
        Vector3 GravityPull = Vector3.up * YSpeed;

        //Getting input from player to move character in specific direction
        Vector3 MoveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        //Play will move in the direction of camera's POV
        Vector3 rotatedMovement = Quaternion.Euler(0, MyCamera.transform.rotation.eulerAngles.y, 0) * MoveDirection;

        cc.Move((GravityPull + rotatedMovement)* PlayerWalkSpeed* Time.deltaTime);


        if (rotatedMovement.magnitude > 0)
        {
            //player rotation with respect to camera's rotation
            desiredRotation = Mathf.Atan2(rotatedMovement.x, rotatedMovement.z) * Mathf.Rad2Deg;
            MyAnimator.SetBool("walk", true);
            
        }
        if (rotatedMovement.magnitude > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            //if the player is already moving(walking), and the player presses shift, the running animation will be triggered
            MyAnimator.SetBool("run", true);

            //when the running animation is triggered, the player will have a different movement speed
            cc.Move((GravityPull+rotatedMovement) * PlayerRunSpeed * Time.deltaTime);
        }
        else MyAnimator.SetBool("run", false);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerDodge(GravityPull, rotatedMovement);
        }
        //Rotating the character towards input axis from player
        //Also smoothing the characters rotation using Lerp
        Quaternion CurrentRotation = transform.rotation;
        Quaternion TargetRotation = Quaternion.Euler(0, desiredRotation, 0);
        transform.rotation = Quaternion.Lerp(CurrentRotation, TargetRotation, RotationSpeed *Time.deltaTime);

    }
    public bool PlayerAttack()
    {
        if (!MyAnimator.GetCurrentAnimatorStateInfo(1).IsName("attack") && !MyAnimator.GetCurrentAnimatorStateInfo(1).IsName("attack_2"))
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Pressing the left mouse button will trigger the attack animation
                MyAnimator.SetLayerWeight(MyAnimator.GetLayerIndex("Attack layer"), 1);

                int randomAttack = Random.Range(1, 3);

                if (randomAttack == 1)
                {
                    MyAnimator.SetTrigger("attack");
                    MyAnimator.ResetTrigger("attack_2");
                }
                else
                {
                    MyAnimator.ResetTrigger("attack");
                    MyAnimator.SetTrigger("attack_2");
                }
                return true;
            }

        }
        
        return false;
    }

    
    void PlayerDodge(Vector3 gp, Vector3 rM)
    {
        cc.Move((gp + rM) * PlayerRunSpeed * Time.deltaTime);
        MyAnimator.SetBool("dodge", true);
        
    }
}
