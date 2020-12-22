using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterPlatformer : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public float moveSpeed = 10f;
    public Vector2 direction;
    private bool facingRight = true;
    public float iceMultiplier = 15f; 

    [Header("Vertical Movement")]
    public float jumpSpeed = 15f;
    public float jumpDelay = 0.25f;
    private float jumpTimer;

    [Header("Components")]
    public Rigidbody2D rb;
    // public Animator animator;
    public LayerMask groundLayer;
    public LayerMask iceLayer;
    // public GameObject characterHolder;

    [Header("Physics")]
    public float maxSpeed = 7f;
    public float uphillIceReduction = 6f;
    public float linearDrag = 4f;
    public float gravity = 1;
    public float fallMultiplyer = 5f;

    [Header("Collision")]
    public bool onGround = false; // going to use for raycasting (look up concept)
    public bool onIce = false;
    public float groundLength = 0.52f;
    public Vector3 colliderOffset;
    public Collision2D collision2D;

    // Update is called once per frame
    void Update()
    {
        // bool wasOnGround = onGround;
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer)
            || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);

        onIce = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, iceLayer)
            || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, iceLayer);

        /* 
         * if(!wasOnGround && onGround)
         * {
         *      StartCorountine(JumpSqueeze(1.25f, 0.8f, 0.05f));
         * }
         */

        if (Input.GetButtonDown("Jump"))// && onGround)
        {
            //Jump();
            jumpTimer = Time.time + jumpDelay;
        }

        direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    // OnCollisionEnter2D is called when this collider2D/rigidbody2D has begun touching another rigidbody2D/collider2D (2D physics only)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.name == "Ice")
        {
            Debug.Log("Setting collision: " + collision.transform.name);
            collision2D = collision;
        }
    }

    // Good to place code in here dealing with physics
    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled
    private void FixedUpdate()
    {
        moveCharacter(direction.x);

        if((jumpTimer > Time.time) && onGround || (jumpTimer > Time.time) && onIce)
        {
            Jump();
        }

        modifyPhysics();
    }

    void moveCharacter(float horizontal)
    {
        rb.AddForce(Vector2.right * horizontal * moveSpeed);

        // animator.SetFloat("horizontal", Mathf.Abs(rb.velocity.x));

        if((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
        {
            Flip();
        }
        if(Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }    
    }    

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        jumpTimer = 0;
        //StartCoroutine(JumpSqueeze(0.5f, 1.2f, 0.1f));
    }

    void modifyPhysics()
    {
        bool changingDirections = (direction.x > 0 && rb.velocity.x < 0) || (direction.x < 0 && rb.velocity.x > 0);

        if (onGround || onIce)
        {
            if (onGround)
            {
                if (Mathf.Abs(direction.x) < 0.4f || changingDirections)
                {
                    rb.drag = linearDrag;
                }
                else
                {
                    rb.drag = 0f;
                }

                rb.gravityScale = 0;
            }
            else // Ice Physics
            {
                
                rb.gravityScale = 0;

                if(collision2D != null)
                {
                    Debug.Log("NOT NULL" + collision2D);

                    if (collision2D.transform.rotation.z < 0)
                    {

                        if(direction.x < 0)
                        {
                            rb.drag = 5f;
                        }
                        else
                        {
                            rb.drag = 0f;
                            rb.velocity = new Vector2(10f, rb.velocity.y);
                        }

                        Debug.Log("rotation is less than 0: " + collision2D.transform.rotation.z);
                    }
                    else
                    {

                        if (direction.x > 0)
                        {
                            rb.drag = 5f;
                        }
                        else
                        {
                            rb.drag = 0f;
                            rb.velocity = new Vector2(-10f, rb.velocity.y);
                        }

                        Debug.Log("rotation is greater than 0: " + collision2D.transform.rotation.z);
                    }
                }
            }
        }
        else
        {
            rb.gravityScale = gravity;
            rb.drag = linearDrag * 0.15f;
            
            if(rb.velocity.y < 0)
            {
                rb.gravityScale = gravity * fallMultiplyer;
            }
            else if(rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                rb.gravityScale = gravity * (fallMultiplyer / 2);
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }

    /* 
     IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float t = 0f;
        while(t <= 1.0)
        {
            t+= Time.deltaTime / seconds;
            characterHolder.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while(t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            characterHolder.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            yield return null;
        }
    }
     */


    // Implement this OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);
    }


}
