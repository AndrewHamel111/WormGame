using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class WormPlayer : MonoBehaviour
{
    /*
    // player constants
    [Header("Player Constants")]
    [SerializeField] private float moveSpeed;

    // animation constants

    // animation vars
    private string currentState = "player_idle_right";
    private bool left = false;
    private bool up = false;

    // input vars
    public float xMove; // horizontal input axis
    public float zMove; // vertical input axis
    private bool[] actionBools = { false, false, false, false }; // jump, interact, sprint, cancel

    // state vars
    private bool grounded;
    public bool digging = false;
    public bool craftingAnim = false; // when true, crafting animation is occuring
    public bool pouringAnim = false; // when true, pouring is occuring 
    public bool crafting = false; // when true, actions will complete a crafting operation
    public bool bootsEquipped = false;
    public bool fistEquipped = false;
    public bool onSpikes = false;
    public bool punchAnim = false;
    //private bool inSearchRadius = false; // for wet patches (high yield)
    private WormholeController wormPatch;

    // game fields
    [Header("")]
    [SerializeField] private ushort wormCount = 1;
    public ushort WormCount { get => wormCount; }
    [SerializeField] private ushort maxWormCount = 15;

    // unity components
    [Header("Unity Components")]
    Rigidbody rb;
    CapsuleCollider coll;
    [SerializeField] Animator animator; // Managed in inspector;
    [SerializeField] SpriteRenderer spriteRenderer; // Managed in inspector;
    [SerializeField] Text[] wormCountText;
    [SerializeField] Color defaultColorText;
    [SerializeField] Color maxColorText;
    [SerializeField] Tilemap tilemap;

    private GameObject wormMeter;
    private Image wormMeterImage;

    // hazard vars
    Vector3 spikeOrigin;
    Vector3 spikeDestination;
    bool spikeAnim = false;
    private float spikeTime;
    [SerializeField] private float spikeDuration = 1.0f;
    [SerializeField] private float spikeAmplitude = 1.5f;

    // crafting vars
    WormPile wormPile;
    [SerializeField] float wormShootTime;
    [SerializeField] Vector3 wormPileOffset;
    public int bootsTimer;
    [SerializeField] private int bootsDuration;

    // Start is called before the first frame update
    void Start()
    {
        spikeOrigin = spikeDestination = Vector3.zero;

        rb = this.GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("Could not find Rigidbody in PlayerController script.");

        coll = this.GetComponent<CapsuleCollider>();
        if (coll == null)
            Debug.LogError("Could not find CapsuleCollider in PlayerController script.");

        wormMeter = GameObject.FindGameObjectWithTag("WormMeter");
        if (wormMeter == null) Debug.LogError("No WormMeter in scene!");
        wormMeterImage = wormMeter.GetComponent<Image>();
        if (wormMeterImage == null) Debug.LogError("No Image component on WormMeter tagged object!!");
    }

    // Update is called once per frame
    void Update()
    {
        if (spikeAnim || craftingAnim || pouringAnim || punchAnim) return;

        if (crafting) GameManager.Instance.craftingGUI.SetActive(true);
        else GameManager.Instance.craftingGUI.SetActive(false);

        if (!digging)
            PollInputs();

        if (actionBools[0] && !crafting && !onSpikes)
        {
            // attempt a worm pour
            actionBools[0] = false;
            SetCurrentState(PLAYER_SHOOT);
            pouringAnim = true;
            Invoke("SpawnWormPile", wormShootTime);
        }
        if (actionBools[1] && fistEquipped && !crafting && !onSpikes)
        {
            actionBools[1] = false;
            Invoke("PerformPunch", punchTime);
            punchAnim = true;
            SetCurrentState(PLAYER_PUNCH);
        }
        else if (actionBools[1] && wormPatch != null && !digging && !crafting && !onSpikes) // dig
        {
            SetCurrentState(PLAYER_DIG);
            digging = true;
            rb.velocity = Vector3.zero;
            Invoke("Search", digTime);
        }

        // update state variables
        if (xMove > 0)
            left = false;
        else if (xMove < 0)
            left = true;

        if (zMove > 0)
            up = true;
        else if (zMove < 0)
            up = false;

        // update animation states
        if (!crafting)
            spriteRenderer.flipX = left;
        if (Mathf.Abs(zMove) > 0) // player is moving up/down
        {
            if (zMove > 0)
                SetCurrentState(PLAYER_WALK_BACK);
            else
                SetCurrentState(PLAYER_WALK_RIGHT);
        }
        else if (Mathf.Abs(xMove) > 0) // player is moving left/right
        {
            if (xMove > 0)
                SetCurrentState(PLAYER_WALK_RIGHT);
            else
                SetCurrentState(PLAYER_WALK_RIGHT);
        }
        else // player is standing still
        {
            if (up)
                SetCurrentState(PLAYER_IDLE_BACK);
            else
                SetCurrentState(PLAYER_IDLE_RIGHT);
        }

        wormCountText[0].text = wormCount.ToString();
        wormCountText[1].text = wormCount.ToString();
        if (wormCount == maxWormCount)
            wormCountText[0].color = maxColorText;
        else
            wormCountText[0].color = defaultColorText;
    }
    private void FixedUpdate()
    {
        if (digging || craftingAnim) return;

        if (crafting)
        {
            if (zMove < 0 || actionBools[3]) // cancel
            {
                crafting = false;

                if (wormPile == null) return;
                wormPile.Dismiss();
                wormPile = null;
            }
            else if (xMove < 0) // craftBoots
            {
                if (bootsEquipped)
                {
                    // dismiss pile if you already have boots
                    wormPile.Dismiss();
                    wormPile = null;
                }
                else
                {
                    Invoke("CraftBoots", digTime);
                    SetCurrentState(PLAYER_DIG);
                    craftingAnim = true;
                }

                crafting = false;
            }
            else if (xMove > 0) // craftFist
            {
                if (fistEquipped)
                {
                    // dismiss pile if you already have fist
                    wormPile.Dismiss();
                    wormPile = null;
                }
                else
                {
                    Invoke("CraftFist", digTime);
                    SetCurrentState(PLAYER_DIG);
                    craftingAnim = true;
                }

                crafting = false;
            }

            return;
        }



        wormMeter.SetActive(bootsEquipped);
        if (bootsEquipped)
            wormMeterImage.fillAmount = (float)bootsTimer / (float)bootsDuration;

        if (onSpikes && bootsEquipped)
        {
            bootsTimer--;
            if (bootsTimer <= 0)
            {
                bootsTimer = 0;
                bootsEquipped = false;
                GameManager.Instance.SpawnSplosion(this.transform.position + new Vector3(0, -1.0f, 0));
            }
        }
        
        if (spikeAnim)
        {
            float t = (Time.time - spikeTime) / spikeDuration;

            Vector3 p = Vector3.Lerp(spikeOrigin, spikeDestination, t);
            p.y += 4 * t * (1 - t) * spikeAmplitude;
            this.transform.position = p;

            if(t > 1)
            {
                spikeAnim = false;
                spikeTime = 0.0f;
            }
        }
        else
        {
            MovePlayer();
            
        }
    }

    private void Search()
    {
        wormPatch.Search();
        Debug.Log("Searched");
        digging = false;
        //SetCurrentState(PLAYER_IDLE_RIGHT);
    }

    private void CraftBoots()
    {
        // if we have fist reimburse that first
        if (fistEquipped)
        {
            GameManager.Instance.SpawnSplosion(this.transform.position);
            GameManager.Instance.CreateWorm(this.transform.position + new Vector3(0, 2.0f, 0), false, 10);

            fistEquipped = false;
        }
        // craft boots
        bootsEquipped = true;
        bootsTimer = bootsDuration;
        wormPile.Destroy();
        wormPile = null;
        craftingAnim = false;
    }

    private void CraftFist()
    {
        // if we have boots reimburse those first
        if (bootsEquipped)
        {
            GameManager.Instance.SpawnSplosion(this.transform.position);
            int wormsLeftFromBoots = (int)((float)bootsTimer / (float)bootsDuration * 10.0f);
            GameManager.Instance.CreateWorm(this.transform.position + new Vector3(0, 2.0f, 0), false, wormsLeftFromBoots);

            bootsEquipped = false;
        }
        // craft fist
        fistEquipped = true;
        wormPile.Destroy();
        wormPile = null;
        craftingAnim = false;
    }

    private void PerformPunch()
    {
        punchAnim = false;
        SetCurrentState(STATE_NONE);
        // see if we hit a rock
        Vector3 direction = Vector3.zero;
        if (left) direction.x = -1;
        else direction.x = 1;
        if (up) direction.z = 1;
        else direction.z = -1;
        RaycastHit info;
        Ray ray = new Ray(this.transform.position, direction);
        Ray rayx = new Ray(this.transform.position, direction - new Vector3(0, 0, direction.z));
        Ray rayz = new Ray(this.transform.position, direction - new Vector3(direction.x, 0, 0));
        if (Physics.Raycast(rayx, out info, 2.5f)) // first check x direction
        {
            if (info.collider.gameObject.tag == "BreakableObject")
            {
                GameObject go = info.collider.gameObject;
                GameManager.Instance.SpawnSplosion(go.transform.position + new Vector3(0, 1.0f, 0));
                Destroy(go);
                fistEquipped = false;
            }
        }
        if (Physics.Raycast(rayz, out info, 2.5f)) // then z direction
        {
            if (info.collider.gameObject.tag == "BreakableObject")
            {
                GameObject go = info.collider.gameObject;
                GameManager.Instance.SpawnSplosion(go.transform.position + new Vector3(0, 1.0f, 0));
                Destroy(go);
                fistEquipped = false;
            }
        }
        else if (Physics.Raycast(ray, out info, 2.5f)) // lastly the diagonal of the directions
        {
            if (info.collider.gameObject.tag == "BreakableObject")
            {
                GameObject go = info.collider.gameObject;
                GameManager.Instance.SpawnSplosion(go.transform.position + new Vector3(0, 1.0f, 0));
                Destroy(go);
                fistEquipped = false;
            }
        }
    }

    private void EnterCrafting()
    {
        crafting = true;
    }

    private void PollInputs()
    {
        // movement with WASD or arrow keys
        xMove = Input.GetAxisRaw("Horizontal");
        zMove = Input.GetAxisRaw("Vertical");

        // TODO replace these with Input.GetButton
        actionBools[0] = Input.GetButton("Use2"); 
        actionBools[1] = Input.GetButton("Use1");
        actionBools[2] = Input.GetKey(KeyCode.LeftShift);
        actionBools[3] = Input.GetButton("Cancel");
    }

    private void SpawnWormPile()
    {
        SetCurrentState(PLAYER_IDLE_RIGHT);
        pouringAnim = false;
        if (wormCount > 10)
        {
            crafting = true;

            Vector3 v = this.transform.position;
            if (left) v -= wormPileOffset;
            else v += wormPileOffset;
            v.y -= 2.0f;

            wormPile = GameManager.Instance.SpawnWormPile(v);
            if (left)
                wormPile.FlipX();
            //wormCount -= (ushort)GameManager.wormPileCost;
            wormCount -= 10;
        }
    }

    private void MovePlayer()
    {
        Vector3 v = rb.velocity;
        float speed = moveSpeed;
        if (actionBools[2])
            speed *= sprintBonus;

        // do it
        if (Mathf.Abs(zMove) > 0 && Mathf.Abs(xMove) > 0)
        {
            v.z = zMove * speed * 0.65f;
            v.x = xMove * speed * 0.65f;
        }
        else
        {
            v.z = zMove * speed;
            v.x = xMove * speed;
        }

        rb.velocity = v;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Hazard")
        {
            if (!bootsEquipped)
                InitializeSpike(collision.gameObject);
            else
                onSpikes = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Ground")
            grounded = true;
        else if (collision.collider.gameObject.tag == "Hazard")
        {
            if (bootsEquipped)
                onSpikes = true;
            else
                InitializeSpike(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Hazard")
            onSpikes = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "WormPatch")
        {
            //inSearchRadius = true;
            wormPatch = other.gameObject.GetComponent<WormholeController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "WormPatch")
        {
            //inSearchRadius = false;
            wormPatch = null;
        }
    }

    private void InitializeSpike(GameObject go)
    { 
        SpikeData sp = go.GetComponent<SpikeData>();
        if (sp == null) Debug.LogError("Hazard has no SpikeData!");
        else
        {
            spikeDestination = sp.SpikeDestination.position;
            spikeOrigin = this.transform.position;
            spikeAnim = true;
            spikeTime = Time.time;
            SetCurrentState(PLAYER_PAIN);
        }
    }

    public bool AddWorm()
    {
        if (wormCount < maxWormCount)
        {
            wormCount++;
            return true;
        }

        return false;
    }

    public bool RemoveWorms(ushort n)
    {
        if (wormCount <= n)
            return false;

        wormCount -= n;
        return true;
    }

    void SetCurrentState(string statename)
    {
        if (digging) return;

        if (statename == PLAYER_WALK_RIGHT && up)
            statename = PLAYER_WALK_BACK;

        if (bootsEquipped && (statename == PLAYER_IDLE_RIGHT || statename == PLAYER_IDLE_BACK ||
            statename == PLAYER_WALK_RIGHT || statename == PLAYER_WALK_BACK) )
        {
            statename += "_boots";
        }

        string str;

        if (wormCount == 1) str = "_1";
        else if (wormCount <= 1 * maxWormCount / 3) str = "_2";
        else if (wormCount <= 2 * maxWormCount / 3) str = "_3";
        else if (wormCount < maxWormCount) str = "_4";
        else str = "_5";

        if (statename + str == currentState) return;

        animator.Play(statename + str);

        currentState = statename + str;
    }
    */
}
