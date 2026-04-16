using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rb;


    public int facingDirection = 1; // 1 for right, -1 for left
    
    public Animator anim;

    void Start()
    {
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (horizontalInput > 0)
        {
            facingDirection = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalInput < 0)
        {
            facingDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }


        anim.SetFloat("horizontal", Mathf.Abs(horizontalInput));
        anim.SetFloat("vertical", Mathf.Abs(verticalInput));



        rb.linearVelocity = new Vector2(horizontalInput, verticalInput).normalized * moveSpeed;



    }


}
