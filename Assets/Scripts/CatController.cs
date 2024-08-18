using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class CatController : MonoBehaviour
{

    [SerializeField]
    GameObject rollingCat;
    public Rigidbody2D head;
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
    public List<GameObject> stomachContents;
    List<PolygonCollider2D> stomachColliders;

    private float buttMove;
    private float headMove;

    private bool buttJump;
    private bool headJump;

    private float buttJumpTimer=0f;
    private float headJumpTimer=0f;

    List<Vector2> splinePoints = new List<Vector2>();

    AudioManager audioManager;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    // These functions are called by unity's new input action system when the user presses the buttons
    public void OnButtMove(InputAction.CallbackContext context)
    {
        // Gets the value of the input as float since it can be decimal in the case of a joystick
        // This value is either -1 or 1 depending on direction pressed on keyboard
        // Reads into input variable to immediately receive input and then uses it when drawing the next frame
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
            buttJumpTimer = 0.2f;
        }
    }

    public void OnHeadJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            headJump = true;
            headJumpTimer = 0.2f;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        // Initialize the lists
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
        // This gets the 4 corners from the sprite shape controller in the editor so we can use them in the script
        // without hardcoding them
        spline = sprite.spline;
        for (int i = 0; i < 4; i++)
        {
            splinePoints.Add(spline.GetPosition(i));
        }
    }

    void OnDrawGizmos()
    {
        if (head == null || butt == null)
        {
            return;
        }
        int flipFactor = butt.position.x < head.position.x ? 1 : -1;
        if (headJump)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawSphere(head.transform.position + -head.transform.up * 0.5f*flipFactor, 0.1f);
        if (buttJump)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawSphere(butt.transform.position + -butt.transform.up * 0.5f*flipFactor, 0.1f);
    }

    // FixedUpdate is called once per physics frame
    // This is where we should do all physics related calculations
    // The time between each FixedUpdate is constant
    void FixedUpdate()
    {


        // If we have some input
        if (Mathf.Abs(buttMove) > 0.001)
        {
            // Move the butt with speed
            butt.velocity = new Vector2(buttMove * speed, butt.velocity.y);
        }
        else
        {
            // Otherwise apply just gravity
            // Probalby redundant
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
        
        int flipFactor = butt.position.x < head.position.x ? 1 : -1;
        // Jump the cat
        if (headJump)
        {
            if (Physics2D.OverlapCircle(head.position + -(Vector2)head.transform.up * 0.5f * flipFactor, 0.1f, groundLayer) != null)
            {
                headJump = false;
                headAnimator.SetTrigger("Jumping");
                head.velocity = new Vector2(head.velocity.x, 0);
                head.AddForce((Vector2)butt.transform.up * jumpForce * flipFactor, ForceMode2D.Impulse);
            } else {
                headJumpTimer -= Time.fixedDeltaTime;
                if (headJumpTimer <= 0)
                {
                    headJump = false;
                }
            }
        }

        if (buttJump)
        {
            if (Physics2D.OverlapCircle(butt.position + -(Vector2)butt.transform.up * 0.5f * flipFactor, 0.1f, groundLayer) != null) 
            {
                buttJump = false;
                buttAnimator.SetTrigger("Jumping");
                butt.velocity = new Vector2(butt.velocity.x, 0);
                butt.AddForce((Vector2)butt.transform.up * jumpForce * flipFactor, ForceMode2D.Impulse);
            } else {
                buttJumpTimer -= Time.fixedDeltaTime;
                if (buttJumpTimer <= 0)
                {
                    buttJump = false;
                }
            }
        }

        headAnimator.SetFloat("Speed", Mathf.Abs(headMove));
        buttAnimator.SetFloat("Speed", Mathf.Abs(buttMove));

        // determine the horizontal size of the belly to set the distance of the spring joint
        // butt ---- belly ---- head
        //     [   bellyWidth  ]
        float bellyWidth;
        // if nothing in stomach, belly width just enough to separate butt and head + some padding
        if (stomachContents.Count == 0)
        {
            bellyWidth = 1.05f;
            // transform.GetChild(1).GetComponents<SpringJoint2D>()[0].distance = bellyWidth / 2f;
            // transform.GetChild(1).GetComponents<SpringJoint2D>()[1].distance = bellyWidth / 2f;
        }
        else // if there is food in stomach, belly width is the sum of the food widths + some padding + butt and head width ( from their centers )
        {
            bellyWidth = 1.05f;
            if (SceneManager.GetActiveScene().name == "03-ball")
            bellyWidth = 0.55f;
            foreach (GameObject food in stomachContents)
            {
                List<Vector2> points = food.GetComponent<PolygonCollider2D>().points.ToList();
                float maxX = points.Max(p => p.x);
                float minX = points.Min(p => p.x);
                bellyWidth += Mathf.Abs(maxX - minX);
            }
            // find middle - leftmost point of stomach
            Vector2 middle = Vector2.zero;
            Vector2 leftmost = stomachContents[0].GetComponent<PolygonCollider2D>().points.ToList().OrderBy(p => p.x).First();
            // transform.GetChild(1).GetComponents<SpringJoint2D>()[0].distance = Vector3.Magnitude(middle - leftmost) + 1.05f/2f + 0.05f;
            // find rightmost - middle point of stomach
            Vector2 rightmost = stomachContents[0].GetComponent<PolygonCollider2D>().points.ToList().OrderByDescending(p => p.x).First();
            // transform.GetChild(1).GetComponents<SpringJoint2D>()[1].distance = Vector3.Magnitude(middle - rightmost) + 1.05f/2f + 0.05f;;
        }
        bool trigger = Vector3.SqrMagnitude(head.position - butt.position) <= (bellyWidth - 0.5f) * (bellyWidth - 0.5f);
        head.gameObject.GetComponent<PolygonCollider2D>().isTrigger = trigger;
        // transform.GetChild(1).gameObject.SetActive(!trigger && stomachContents.Count > 0);
        

        butt.gameObject.GetComponent<SpringJoint2D>().distance = bellyWidth;

        
        for (int i = 0; i < stomachContents.Count; i++)
        {
        
            if (stomachContents[i].name == "anvil")
            {
                head.mass = 20;
                butt.mass = 20;
                jumpForce = 50;
            }
            else if (stomachContents[i].name == "balloon")
            {

                head.velocity = new Vector2(head.velocity.x, 1);
                butt.velocity = new Vector2(head.velocity.x, 1);
                head.drag = 0.6f;
                butt.drag = 0.6f;
            }
            else if (stomachContents[i].name == "ball")
            {
                if (head.GetComponent<CircleCollider2D>().bounciness < 1f)
                {
                    PhysicsMaterial2D bouncy = new PhysicsMaterial2D();
                    bouncy.bounciness = 1.4f;
                    bouncy.friction = head.GetComponent<CircleCollider2D>().sharedMaterial.friction;
                    head.GetComponent<CircleCollider2D>().sharedMaterial = bouncy;
                    butt.GetComponent<CircleCollider2D>().sharedMaterial = bouncy;
                    head.GetComponent<PolygonCollider2D>().sharedMaterial = bouncy;
                }

            } else if (stomachContents[i].name == "watermelon")
            {
                for (int j = 0; j < 3; j++)
                {
                    transform.GetChild(j).gameObject.SetActive(false);
                }
                rollingCat.transform.position = (head.transform.position + butt.transform.position )/ 2;
                rollingCat.SetActive(true);
                gameObject.SetActive(false);
            }
        }


        bool headGrounded = Physics2D.OverlapCircle(head.position, 0.55f, groundLayer) != null;
        bool buttGrounded = Physics2D.OverlapCircle(butt.position, 0.55f, groundLayer) != null;
        // Debug.Log("Head grounded: " + headGrounded);
        // Debug.Log("Butt grounded: " + buttGrounded);
        // if butt is grounded and head isn't, rotate the head to the same angle as the butt
        // if (buttGrounded && !headGrounded)
        // {
        //     head.MoveRotation(butt.rotation);
        // } // if head is grounded and butt isn't, rotate the butt to the same angle as the head
        // else if (headGrounded && !buttGrounded)
        // {
        //     butt.MoveRotation(head.rotation);
        // } // if both are grounded, rotate both to the average angle
        // else if (headGrounded && buttGrounded)
        // {
        //     float averageRotation = (head.rotation + butt.rotation) / 2;
        //     head.MoveRotation(averageRotation);
        //     butt.MoveRotation(averageRotation);
        // }
        

    }

    // Update is called once per frame
    // This is where we should do mostly rendering related calculations
    // The time between each Update is not constant, it is Time.deltaTime
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        // Get the direction of the cat
        Vector2 butt2head = (head.position - butt.position).normalized;

        // Get the direction of the cat
        // Flip is used to calculate the belly points correctly when the cat is flipped
        int flipFactor = butt.position.x < head.position.x ? 1 : -1;

        // Total width of the food in the stomach used for calculating the belly width which is used for positing food in belly
        float totalFoodWidth = 0;

        List<List<Vector2>> bellyPoints = new List<List<Vector2>>();
        // Get the points of the food in the stomach
        // sort them, extract width, and add them to the belly points
        foreach (GameObject food in stomachContents)
        {
            // Get the bounding points of the food from polygon collider
            bellyPoints.Add(new List<Vector2>());
            List<Vector2> foodPoints = food.GetComponent<PolygonCollider2D>().points.ToList();
            // Sort points by x for easy drawing
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

        Spline collisionSpline = new Spline();
        List<Vector2> collisionPoints = new List<Vector2>();

        // Set position of butt points
        spline.SetPosition(0, buttOrigin + flipFactor * butt2head.Perpendicular2() * splinePoints[0].y + butt2head * splinePoints[0].x);
        spline.SetPosition(1, buttOrigin + flipFactor * butt2head.Perpendicular2() * splinePoints[1].y + butt2head * splinePoints[1].x);

        float minBellyWidth = Mathf.Min(Mathf.Abs(splinePoints[0].y - splinePoints[1].y), Mathf.Abs(splinePoints[2].y - splinePoints[3].y));

        int pointIndex = 2;
        float startPoint = -totalFoodWidth / 2f;

        // Procedurally generate back shape
        foreach (List<Vector2> foodPoints in bellyPoints)
        {
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
                if (foodPoints[i].y > 0)
                {
                    Vector2 p = ((startPoint - min) * Vector2.right) + flipFactor * Vector2.up * foodPoints[i].y + Vector2.right * foodPoints[i].x;
                    collisionPoints.Add(p);
                }
            }
            startPoint += foodWidth;
        }

        float endPoint = totalFoodWidth / 2f;

        int headPointIndex = pointIndex;
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
                if (foodPoints[i].y <= 0)
                {
                    Vector2 p = ((endPoint - max) * Vector2.right) + flipFactor * Vector2.up * foodPoints[i].y + Vector2.right * foodPoints[i].x;
                    collisionPoints.Add(p);
                }
            }
            endPoint -= foodWidth;
        }

        if (collisionPoints.Count > 0)
        {
            PolygonCollider2D collider = head.gameObject.GetComponent<PolygonCollider2D>();
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < collisionPoints.Count; i++)
            {
                points.Add(collisionPoints[i]);
            }
            Vector2 average = new Vector2(points.Average(p => p.x), points.Average(p => p.y));
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Vector2(points[i].x - average.x, points[i].y - average.y);
            }
            collider.points = points.ToArray();
            collider.offset = Vector2.left * (head.position - butt.position).magnitude / 2f;
        }


        // Update the rotation of the cat
        butt.rotation = Mathf.Atan2(butt2head.y, butt2head.x) * Mathf.Rad2Deg;
        head.rotation = Mathf.Atan2(butt2head.y, butt2head.x) * Mathf.Rad2Deg;

        // Flip the cat if needed
        if (butt.position.x > head.position.x)
        {
            butt.gameObject.GetComponent<CircleCollider2D>().offset = new Vector2(0.08468014f, 0.089414f);
            head.gameObject.GetComponent<CircleCollider2D>().offset = new Vector2(-0.08468014f, 0.089414f);
            butt.transform.GetComponent<SpriteRenderer>().flipY = true;
            head.transform.GetComponent<SpriteRenderer>().flipY = true;
        }
        else
        {
            butt.gameObject.GetComponent<CircleCollider2D>().offset = new Vector2(0.08468014f, -0.089414f);
            head.gameObject.GetComponent<CircleCollider2D>().offset = new Vector2(-0.08468014f, -0.089414f);
            butt.transform.GetComponent<SpriteRenderer>().flipY = false;
            head.transform.GetComponent<SpriteRenderer>().flipY = false;
        }
        // butt.transform.GetChild(0).rotation = butt.transform.rotation;
        // head.transform.GetChild(0).rotation = butt.transform.rotation;
    }
}
