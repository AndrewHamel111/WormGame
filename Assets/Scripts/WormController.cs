using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormController : MonoBehaviour
{
    // constants
    [Tooltip("Minimum time that a worm will be still after spawning.")]
    [SerializeField] static float spawnDelay = 1.0f;
    [SerializeField] float chance = 0.5f;
    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] int wormVariant = 2;
    [SerializeField] float rainbowChance = 0.15f;

    // runtime vars
    private bool moving;
    private bool left;
    private bool falling;
    private float moveDiff;
    public bool collected;
    //private bool launching = false;
    //private bool ignorePlayer = false;
    private string currentState = "worm_idle_2";

    // Unity Components
    Rigidbody rb;
    [SerializeField] BoxCollider coll;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    // fancy animation stuff
    private Vector3 startPoint; // the point the worm started from
    //private Vector3 apexPoint; // the first point the worm will lerp to
    private Transform destinationTransform; // the transform the worm is heading for 
    [SerializeField] Vector3 playerHeight;
    private float time_t;
    [SerializeField] static float animationPhase1Time;
    [SerializeField] static float animationPhase2Time;
    private bool seekingPlayer;
    [SerializeField] float Phase2Pause;
    [SerializeField] float wormBounceX;
    [SerializeField] float wormBounceY;
    [SerializeField] float wormBounceZ;

    const string FALLING = "worm_falling";
    const string MOVING = "worm_moving";
    const string IDLE = "worm_idle";

    // Start is called before the first frame update
    void Start()
    {
        moving = left = false;
        moveDiff = 1.0f;
        collected = false;

        if (Random.value < rainbowChance)
            wormVariant = (Random.value < 0.5f) ? 4 : 5;
        else
            wormVariant = Random.Range(0, 4);

        rb = this.GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("No Rigidbody on WormController!");
/*
        coll = this.GetComponent<BoxCollider>();
        if (coll == null)
            Debug.LogError("No BoxCollider on WormController!");
*/
        if (animator == null)
            Debug.LogError("No Animator reference on WormController!");


        // fancy anim
        time_t = 0.0f;
        animationPhase1Time = 0.15f;
        animationPhase2Time = 0.2f;
        //animationPhase1Time = animationPhase2Time = 0.5f;
        seekingPlayer = false;
    }

    private void Awake()
    {
        Invoke("MoveWorm", spawnDelay);
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.flipX = !left;

        if (!collected)
        {
            if (Mathf.Abs(rb.velocity.y) > 0.01f)
                falling = true;

            if (falling)
            {
                SetCurrentState(FALLING);
            }
            else if (moving)
                SetCurrentState(MOVING);
            else
                SetCurrentState(IDLE);

            rb.velocity = new Vector3(
                moving ? (left ? 0 - moveSpeed : moveSpeed) : 0,
                rb.velocity.y, rb.velocity.z);
        }
        else
        {
            SetCurrentState(FALLING);
            float t = 0;
            if (!seekingPlayer) // phase 1
            {
                t = (Time.time - time_t) / animationPhase1Time;
                Vector3 midpoint = startPoint + (destinationTransform.position - startPoint) / 2;
                midpoint.y = destinationTransform.position.y + playerHeight.y + 1.0f;

                this.transform.position = Vector3.Lerp(startPoint, midpoint, t);
                Vector3 v = this.transform.position - midpoint;
                if (Mathf.Abs(v.x) < Mathf.Epsilon && Mathf.Abs(v.y) < Mathf.Epsilon)
                {
                    seekingPlayer = true;
                    time_t = Time.time;
                }
            }
            else // phase 2
            {
                t = (Time.time - time_t) / animationPhase2Time;
                Vector3 midpoint = startPoint + (destinationTransform.position - startPoint) / 2;
                midpoint.y = destinationTransform.position.y + playerHeight.y + 1.0f;

                this.transform.position = Vector3.Lerp(midpoint, destinationTransform.position + playerHeight, t);
                this.transform.localScale = new Vector3(1 - t * 0.8f, 1 - t, 1);
                Vector3 v = this.transform.position - (destinationTransform.position + playerHeight);
                if (t > 1)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (this.transform.position.y < -50.0f)
            Destroy(this.gameObject);
    }

    void MoveWorm()
    {
        if (collected) return;

        if (Random.value < chance)
        {
            moving = !moving;
            if (moving)
                left = Random.value < 0.5f;
        }

        moveDiff = 2 * Random.value + 0.5f; // 0.5f <= moveDiff <= 2.5f
        Invoke("MoveWorm", moveDiff);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collected) return;

        if (collision.collider.gameObject.tag == "Player")
        {
            GameObject pGo = collision.collider.gameObject;
            PlayerController p = pGo.GetComponent<PlayerController>();
            if (p == null)
                Debug.LogError("No PlayerController on object tagged as Player!");

            if (p.AddWorm())
            {
                destinationTransform = pGo.GetComponent<Transform>();
                Vector3 pos = destinationTransform.position;
                //apexPoint = pos + playerHeight;
                startPoint = transform.position;

                time_t = Time.time;
                collected = true;

                moving = false;
                //falling = true;

                rb.useGravity = false;
                coll.enabled = false;

                GameManager.Instance.wormManager.wormCount--;
            }
            else
            {
                /*
                Vector3 v = Vector3.zero;
                v.x = (Random.value < 0.5f) ? wormBounceX : 0 - wormBounceX;
                v.y = wormBounceY;

                this.transform.position = pGo.transform.position + new Vector3(0, 0, -1.5f);
                this.rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0);
                rb.AddForce(v, ForceMode.Impulse);
                */
            }
        }
        else if (collision.collider.gameObject.tag == "Ground")
            falling = false;
    }

    void StartPhase2()
    {
        seekingPlayer = true;
        time_t = Time.time;
    }

    public void StartLaunch()
    {
        Invoke("Launch", 0.05f);
        //Launch();
    }

    public void Launch()
    {
        // determine a vector for the force
        Vector3 v = Vector3.zero;
        //v.x = (Random.value < 0.5f) ? wormBounceX : 0 - wormBounceX;
        v.x = Random.Range(-1.0f, 1.0f) * wormBounceX;
        v.y = wormBounceY;
        v.z = 0 - Random.value * wormBounceZ;

        // apply the force
        rb.AddForce(v, ForceMode.Impulse);
    }

    private void ReEnableCollider()
    {
        //Debug.Log("Collider enabled");
        //coll.enabled = true;
    }

    void SetCurrentState(string statename)
    {
        if (statename + "_" + wormVariant == currentState) return;

        animator.Play(statename + "_" + wormVariant);

        currentState = statename + "_" + wormVariant;
    }
}
