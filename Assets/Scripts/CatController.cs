using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class CatController : MonoBehaviour
{

    Rigidbody2D head;
    Rigidbody2D butt;

    private SpriteShapeController sprite;
    private Spline spline;

    Animator headAnimator;
    Animator buttAnimator;

    [SerializeField]
    float speed = 5f;
    [SerializeField]
    float jumpForce = 10f;
    [SerializeField]
    float mass = 1f;

    [SerializeField]
    LayerMask groundLayer;

    [SerializeField]
    List<GameObject> stomachContents;
    List<PolygonCollider2D> stomachColliders;

    private float buttMove;
    private float headMove;

    private bool buttJump;
    private bool headJump;

    List<Vector2> splinePoints = new List<Vector2>();

    // These functions are called by unity's new input action system when the user presses the buttons
    public void OnButtMove(InputAction.CallbackContext context)
    {
        // Gets the value of the input as float since it can be decimal in the case of a joystick
        buttMove = context.ReadValue<float>();
    }

    public void OnHeadMove(InputAction.CallbackContext context)
    {
        headMove = context.ReadValue<float>();
    }

    public void OnButtJump(InputAction.CallbackContext context)
    {
        // If the button is pressed
        if (context.performed)
        {
            buttJump = true;
        }
    }

    public void OnHeadJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            headJump = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (stomachContents == null)
        {
            stomachContents = new List<GameObject>();
        }

        if (stomachColliders == null)
        {
            stomachColliders = new List<PolygonCollider2D>();
        }

        buttMove = 0;
        headMove = 0;
        // Automatically find components from head and butt
        head = GameObject.Find("Head").GetComponent<Rigidbody2D>();
        butt = GameObject.Find("Butt").GetComponent<Rigidbody2D>();

        headAnimator = GameObject.Find("Head").GetComponent<Animator>();
        buttAnimator = GameObject.Find("Butt").GetComponent<Animator>();

        // Get belly sprite shap controller for dynamically generating belly later
        sprite = GameObject.Find("Body").GetComponent<SpriteShapeController>();

        // Save spline points for dynamically generating belly later
        spline = sprite.spline;
        for (int i = 0; i < 4; i++)
        {
            splinePoints.Add(spline.GetPosition(i));
            Debug.Log(spline.GetPosition(i));
        }
    }

    void FixedUpdate()
    {
        // If we have some input
        if (Mathf.Abs(buttMove) > 0.001)
        {
            // Move the butt with speed and apply gravity
            butt.velocity = new Vector2(buttMove * speed, butt.velocity.y);
        }
        else
        {
            // Otherwise apply just gravity
            butt.velocity = new Vector2(butt.velocity.x, butt.velocity.y);
        }

        if (Mathf.Abs(headMove) > 0.001)
        {
            head.velocity = new Vector2(headMove * speed, head.velocity.y);
        }
        else
        {
            head.velocity = new Vector2(head.velocity.x, head.velocity.y);
        }

        // Jump the cat
        if (headJump && Physics2D.OverlapCircle(head.position + Vector2.down * 0.5f, 0.05f, groundLayer) != null)
        {
            headJump = false;
            headAnimator.SetTrigger("Jumping");
            head.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (buttJump && Physics2D.OverlapCircle(butt.position + Vector2.down * 0.5f, 0.05f, groundLayer) != null)
        {
            buttJump = false;
            buttAnimator.SetTrigger("Jumping");
            butt.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        headAnimator.SetFloat("Speed", Mathf.Abs(headMove)/* + Mathf.Abs(buttMove) */);
        buttAnimator.SetFloat("Speed", Mathf.Abs(buttMove)/* + Mathf.Abs(headMove) */);

        float bellyWidth = 0;
        if (stomachContents.Count == 0)
        {
            bellyWidth = 1.05f;
        }
        else
        {
            bellyWidth = 0.55f;
            foreach (GameObject food in stomachContents)
            {
                bellyWidth += food.GetComponent<SpriteRenderer>().bounds.size.x;
            }
        }
        butt.gameObject.GetComponent<SpringJoint2D>().distance = bellyWidth;

        // if (stomachContents.Count != stomachColliders.Count)
        // {
        //     foreach (GameObject food in stomachContents)
        //     {
        //         PolygonCollider2D foodCollider = food.GetComponent<PolygonCollider2D>();
        //         PolygonCollider2D stomachCollider = gameObject.AddComponent<PolygonCollider2D>();
        //         stomachCollider.points = foodCollider.points;
        //         // stomachCollider.offset = 
        //         stomachColliders.Add(stomachCollider);
        //     }
        // }
    }

    // Update is called once per frame
    void Update()
    {
        // Get the direction of the cat
        Vector2 butt2head = (head.position - butt.position).normalized;

        // Update the spline
        // Flip is used to calculate the belly points correctly when the cat is flipped
        int flipFactor = butt.position.x < head.position.x ? 1 : -1;

        float totalFoodWidth = 0;
        // Procedurally generate body

        List<List<Vector2>> bellyPoints = new List<List<Vector2>>();
        foreach (GameObject food in stomachContents)
        {
            // Get the bounding points of the food from polygon collider
            bellyPoints.Add(new List<Vector2>());
            List<Vector2> foodPoints = food.GetComponent<PolygonCollider2D>().points.ToList();
            // Sort points by x
            foodPoints.Sort((a, b) => a.x.CompareTo(b.x));
            totalFoodWidth += Mathf.Abs(foodPoints.Last().x - foodPoints.First().x);
            // Add the points to the belly points
            foreach (Vector2 point in foodPoints)
            {
                bellyPoints.Last().Add(point);
            }
        }

        Vector2 buttOrigin = butt.position + butt2head * 0.5f;
        Vector2 headOrigin = head.position - butt2head * 0.5f;


        // Set position of butt points
        spline.SetPosition(0, buttOrigin + flipFactor * butt2head.Perpendicular2() * splinePoints[0].y + butt2head * splinePoints[0].x);
        spline.SetPosition(1, buttOrigin + flipFactor * butt2head.Perpendicular2() * splinePoints[1].y + butt2head * splinePoints[1].x);

        float minBellyWidth = Mathf.Min(Mathf.Abs(splinePoints[0].y - splinePoints[1].y), Mathf.Abs(splinePoints[2].y - splinePoints[3].y));

        int pointIndex = 2;
        float startPoint = -totalFoodWidth / 2f;
        // Procedurally generate back shape
        foreach (List<Vector2> foodPoints in bellyPoints)
        {
            // normalize x and y axis so that average is 0
            float averageX = foodPoints.Average(p => p.x);
            float averageY = foodPoints.Average(p => p.y);
            for (int i = 0; i < foodPoints.Count; i++)
            {
                foodPoints[i] = new Vector2(foodPoints[i].x - averageX, foodPoints[i].y - averageY);
            }

            float foodWidth = foodPoints.Max(p => p.x) - foodPoints.Min(p => p.x);
            float min = foodPoints.Min(p => p.x);

            for (int i = 0; i < foodPoints.Count; i++)
            {
                if (foodPoints[i].y > minBellyWidth / 2f)
                {
                    Vector2 p = ((buttOrigin + headOrigin) / 2f + (startPoint - min) * butt2head) + flipFactor * butt2head.Perpendicular2() * foodPoints[i].y + butt2head * foodPoints[i].x;

                    if (spline.GetPointCount() - 2 == pointIndex)
                    {
                        spline.InsertPointAt(pointIndex, p);
                    }
                    else
                    {
                        spline.SetPosition(pointIndex, p);
                    }
                    pointIndex++;
                }
            }
            startPoint += foodWidth;
        }

        float endPoint = totalFoodWidth / 2f;
        // Set position of head points
        spline.SetPosition(pointIndex++, headOrigin + flipFactor * butt2head.Perpendicular2() * splinePoints[2].y + butt2head * splinePoints[2].x);
        spline.SetPosition(pointIndex++, headOrigin + flipFactor * butt2head.Perpendicular2() * splinePoints[3].y + butt2head * splinePoints[3].x);
        // Procedurally generate underbelly shape
        bellyPoints.Reverse();
        foreach (List<Vector2> foodPoints in bellyPoints)
        {
            foodPoints.Reverse();
            // normalize x and y axis so that average is 0
            float averageX = foodPoints.Average(p => p.x);
            float averageY = foodPoints.Average(p => p.y);
            for (int i = 0; i < foodPoints.Count; i++)
            {
                foodPoints[i] = new Vector2(foodPoints[i].x - averageX, foodPoints[i].y - averageY);
            }
            
            float foodWidth = foodPoints.Max(p => p.x) - foodPoints.Min(p => p.x);
            float max = foodPoints.Max(p => p.x);

            for (int i = 0; i < foodPoints.Count; i++)
            {
                if (foodPoints[i].y < -minBellyWidth / 2f)
                {
                    Vector2 p = ((buttOrigin + headOrigin) / 2f + (endPoint - max) * butt2head) + flipFactor * butt2head.Perpendicular2() * foodPoints[i].y + butt2head * foodPoints[i].x;

                    if (spline.GetPointCount() == pointIndex)
                    {
                        spline.InsertPointAt(pointIndex, p);
                    }
                    else
                    {
                        spline.SetPosition(pointIndex, p);
                    }
                    pointIndex++;
                }
            }
            endPoint -= foodWidth;
        }


        // Update the rotation of the cat
        butt.rotation = Mathf.Atan2(butt2head.y, butt2head.x) * Mathf.Rad2Deg;
        head.rotation = Mathf.Atan2(butt2head.y, butt2head.x) * Mathf.Rad2Deg;

        // Flip the cat if needed
        if (butt.position.x > head.position.x)
        {
            butt.gameObject.GetComponent<CircleCollider2D>().offset = new Vector2(0.08468014f, 0.089414f);
            head.gameObject.GetComponent<CircleCollider2D>().offset = new Vector2(-0.08468014f, 0.089414f);
            butt.gameObject.GetComponent<SpriteRenderer>().flipY = true;
            head.gameObject.GetComponent<SpriteRenderer>().flipY = true;
        }
        else
        {
            butt.gameObject.GetComponent<CircleCollider2D>().offset = new Vector2(0.08468014f, -0.089414f);
            head.gameObject.GetComponent<CircleCollider2D>().offset = new Vector2(-0.08468014f, -0.089414f);
            butt.gameObject.GetComponent<SpriteRenderer>().flipY = false;
            head.gameObject.GetComponent<SpriteRenderer>().flipY = false;
        }
    }
}
