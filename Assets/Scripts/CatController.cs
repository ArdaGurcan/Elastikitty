using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class CatController : MonoBehaviour
{

    public Rigidbody2D head;
    public Rigidbody2D butt;

    private SpriteShapeController sprite;
    private Spline spline;

    public Animator headAnimator;
    public Animator buttAnimator;

    [SerializeField]
    float speed = 5f;
    [SerializeField]
    float jumpForce = 10f;
    [SerializeField]

    private float buttMove;
    private float headMove;

    private bool buttJump;
    private bool headJump;

    public void OnButtMove(InputAction.CallbackContext context)
    {
        buttMove = context.ReadValue<float>();
    }

    public void OnHeadMove(InputAction.CallbackContext context)
    {
        headMove = context.ReadValue<float>();
    }

    public void OnButtJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Butt Jump");
            buttJump = true;
        }
    }

    public void OnHeadJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Head Jump");
            headJump = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        buttMove = 0;
        headMove = 0;
        head = GameObject.Find("Head").GetComponent<Rigidbody2D>();
        butt = GameObject.Find("Butt").GetComponent<Rigidbody2D>();
        sprite = GameObject.Find("Body").GetComponent<SpriteShapeController>();
        spline = sprite.spline;
    }

    void FixedUpdate()
    {
        // Move the cat
        if (Math.Abs(buttMove) > 0.001)
        {
            butt.velocity = new Vector2(buttMove * speed, butt.velocity.y);
        } else {
            // butt.velocity = new Vector2(butt.velocity.x, butt.velocity.y);
        }

        if (Math.Abs(headMove) > 0.001)
        {
            head.velocity = new Vector2(headMove * speed, head.velocity.y);
        } else {
            // butt.velocity = new Vector2(butt.velocity.x, butt.velocity.y);
        }

        // Jump the cat
        if (headJump)
        {
            headJump = false;
            head.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        }

        if (buttJump)
        {
            buttJump = false;
            butt.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        }

        headAnimator.SetFloat("Speed", Mathf.Abs(headMove) + Mathf.Abs(buttMove));
        buttAnimator.SetFloat("Speed", Mathf.Abs(buttMove) + Mathf.Abs(headMove));
    }

    void Update()
    {
        Vector2 butt2head = (head.position - butt.position).normalized;

        spline.SetPosition(0, butt.position + butt2head.Perpendicular1() * 0.5f);
        spline.SetPosition(1, head.position + butt2head.Perpendicular1() * 0.5f);
        spline.SetPosition(2, head.position + butt2head.Perpendicular2() * 0.5f);
        spline.SetPosition(3, butt.position + butt2head.Perpendicular2() * 0.5f);

    }
}
