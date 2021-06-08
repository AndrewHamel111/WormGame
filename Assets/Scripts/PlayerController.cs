using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerState
{
	NONE,
    DIGGING, POURING, CRAFTING,
    PUNCHING, CRAFTING_MENU
}

public enum PlayerEquipment
{
    NONE, FIST, BOOTS
}

public class PlayerController : MonoBehaviour
{
    // player constants
    [Header("Player Constants")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintBonus;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float digTime;
    [SerializeField] private float punchTime;

    // animation constants
    const string PLAYER_IDLE_RIGHT = "player_idle_right";
    const string PLAYER_IDLE_BACK = "player_idle_back";

    const string PLAYER_WALK_RIGHT = "player_walk_right";
    const string PLAYER_WALK_BACK = "player_walk_back";

    const string PLAYER_DIG = "player_dig";
    const string PLAYER_PAIN = "player_pain";
    const string PLAYER_SHOOT = "player_shoot";
    const string PLAYER_PUNCH = "player_punch";
    const string STATE_NONE = "";

    // animation vars
    private string currentAnimationState = "player_idle_right";
    private bool left = false;
    private bool up = false;

    // input vars
    public float xMove; // horizontal input axis
    public float zMove; // vertical input axis
    private bool[] actionBools = { false, false, false, false }; // jump, interact, sprint, cancel

    // state vars
    private bool grounded;
    private bool working;
    private PlayerState playerState;
    private PlayerEquipment equipment;

    public bool digging = false;
    public bool craftingAnim = false; // when true, crafting animation is occuring
    public bool pouring = false; // when true, pouring is occuring 
    public bool crafting = false; // when true, actions will complete a crafting operation
    public bool bootsEquipped = false;
    public bool fistEquipped = false;
    public bool levelFinished = false;
    public bool onSpikes = false;
    public bool punchAnim = false;
    //private bool inSearchRadius = false; // for wet patches (high yield)
    private WormholeController wormPatch;

    // game fields
    [Header("")]
    [SerializeField] private ushort wormCount = 1;
    public ushort WormCount { get => wormCount; set => wormCount = value; }
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
    //[SerializeField] Tilemap tilemap;

    private GameObject wormMeter;
    private GameObject wormMeterParent;
    private Image wormMeterImage;

    // cutscene vars
    public TextBox currentTextBox;
    private DialogueTrigger currentCutscene;
    public bool inCutscene; // when true all player input is paused except buttons to advance the cutscene.
    public Vector3 playerTargetPosition; // when true all player input is paused except buttons to advance the cutscene.

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
        wormMeterParent = wormMeter.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (spikeAnim || craftingAnim || pouring || punchAnim || levelFinished) return;

        if (crafting) GameManager.Instance.craftingGUI.SetActive(true);
        else GameManager.Instance.craftingGUI.SetActive(false);

        if (!digging)
            PollInputs();

        if(inCutscene)
        {
            if (Input.GetButtonDown("Use1") || Input.GetButtonDown("Use2") || Input.GetButtonDown("Cancel"))
            {
                if(GameManager.Instance.mainCameraController.cameraInMotion)
                {
                    // ignore user spammin buttons like a child
                }
                else
                {
                    // input should advance textbox.
                    if (currentTextBox == null)
                        Debug.Log("Camera no longer in motion but currentTextBox is null!");
                    else
                    {
                        if(currentTextBox.NextSlide())
                        {
                            currentTextBox.ResetBox();
                            currentTextBox.gameObject.SetActive(false);

                            if(currentCutscene.NextSlide())
                            {
                                // CUTSCENE OVER
                                EndOfCutscene(currentCutscene.id);
                                Destroy(currentCutscene.gameObject);
                                currentCutscene = null;
                                inCutscene = false;
                                GameManager.Instance.mainCameraController.inCutscene = false;
                            }
                        }

                    }
                }
            }


            // check for end of cutscene

            return;
        }

        /*
        if (currentTextBox != null && currentTextBox.freezePlayer)
        {


            return;
        }
        */

        // DEBUG CODE
        /*
        if (Input.GetKeyDown(KeyCode.O))
        {
            //string[] texts = { "Hello cummies lord..", "I wanna cum!!!! don't you?!?!?!?!", "696969696969696969696969696969696969696969696969696969696969696969696969" };
            //currentTextBox = GameManager.Instance.CreateTextBox(texts, this.transform.position + new Vector3(5.0f, 4.0f, 0.0f));
        }

        if (Input.GetKeyDown(KeyCode.I))
            if (currentTextBox.NextSlide())
            {
                currentTextBox.ResetBox();
                currentTextBox.gameObject.SetActive(false);
            }
        */
        if (Input.GetKeyDown(KeyCode.P))
            GameManager.Instance.CreateWorm(this.transform.position + new Vector3(0, 4.0f, 0), true, 50);

        if (actionBools[0] && !onSpikes)
        {
            // attempt a worm pour
            actionBools[0] = false;
            SetPlayerState(PlayerState.POURING);
        }
        if (actionBools[1] && fistEquipped && !onSpikes)
        {
            actionBools[1] = false;
            Invoke("PerformPunch", punchTime);
            punchAnim = true;
            SetCurrentState(PLAYER_PUNCH);
        }
        else if (actionBools[1] && wormPatch != null && !onSpikes) // dig
        {
            actionBools[1] = false;
            SetPlayerState(PlayerState.DIGGING);
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

    /*
    private void OnDrawGizmos()
    {
            Vector3 direction = Vector3.zero;
            if (left) direction.x = -1;
            else direction.x = 1;
            if (up) direction.z = 1;
            else direction.z = -1;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction);
    }
    */

    private void FixedUpdate()
    {
        if (digging || craftingAnim || inCutscene) return;

        if (levelFinished)
        {
            rb.velocity = new Vector3(1.0f, 0, 0);
            left = false;
            SetCurrentState(PLAYER_WALK_RIGHT);
        }

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
        wormMeterParent.SetActive(bootsEquipped);
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
        SetCurrentState(STATE_NONE);
        pouring = false;
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
        else if (other.gameObject.tag == "CutsceneTrigger")
        {
            inCutscene = true;
            rb.velocity = Vector3.zero;

            GameManager.Instance.mainCameraController.inCutscene = true;
            DialogueTrigger dt = other.gameObject.GetComponent<DialogueTrigger>();
            currentCutscene = dt;

            SetCurrentState(PLAYER_IDLE_RIGHT);

            // move player to target 
            playerTargetPosition = currentCutscene.transform.Find("PlayerTarget").position + new Vector3(0, 2.0f, 0) ;

            Invoke("CreateTextBoxFromDT", dt.cameraMoveTime);
            CameraMove move = GameManager.Instance.mainCameraController.GetMove();
            move.endPos = dt.cameraDest + playerTargetPosition;
            move.t = dt.cameraMoveTime;
            StartCoroutine(GameManager.Instance.mainCameraController.CameraMotion(move));
            SetCurrentState(STATE_NONE);
            StartCoroutine(MovePlayerTo(playerTargetPosition, this.transform.position, move.t));
        }
        else if (other.gameObject.tag == "Finish")
        {
            levelFinished = true;
            ReimburseBoots(false);
            ReimburseFist(false);

            left = up = inCutscene = false;
            // TODO falsify the rest of the flags here
            GameManager.Instance.levelFinishing = true;
            GameManager.Instance.wormCount = wormCount;
            GameManager.Instance.StartLevelChange();
        }
    }

    private void ReimburseBoots(bool SpawnWorms = true)
    {
        if (!bootsEquipped) return;

        ushort worms = (ushort)((float)bootsTimer / (float)bootsDuration * 10.0f);
        if (SpawnWorms)
            GameManager.Instance.CreateWorm(this.transform.position, false, worms);
        else
            wormCount += worms;

        bootsEquipped = false;
    }
    private void ReimburseFist(bool SpawnWorms = true)
    {
        if (!fistEquipped) return;

        ushort worms = 10;
        if (SpawnWorms)
            GameManager.Instance.CreateWorm(this.transform.position, false, worms);
        else
            wormCount += worms;
        
        fistEquipped = false;
    }

    IEnumerator MovePlayerTo(Vector3 destination, Vector3 origin, float time)
    {
        float timeElapsed = 0;

        while (timeElapsed < time)
        {
            timeElapsed += Time.deltaTime;

            float t = timeElapsed / time;
            this.transform.position = Vector3.Lerp(origin, destination, t);

            if(origin.x > destination.x)
                left = true;
            if (origin.z < destination.z)
                up = true;
            SetCurrentState(PLAYER_WALK_RIGHT);

            yield return null;
        }

        this.transform.position = transform.position;
        SetCurrentState(PLAYER_IDLE_RIGHT);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "WormPatch")
        {
            //inSearchRadius = false;
            wormPatch = null;
        }
    }

    private void CreateTextBoxFromDT()
    {
        /*
        if (currentCutscene.textBoxPositionIsScreenSpace)
        {
            Vector2 v = new Vector2(currentCutscene.textBoxPosition.x, currentCutscene.textBoxPosition.y);
            currentTextBox = GameManager.Instance.CreateTextBox(currentCutscene.text, v);
        }
        else
        {
        */
        // text box is always 2/3 across the screen. that's just how it is in game jams people
        Vector2 v = new Vector2(2 * Screen.width / 3, Screen.height/2);
            currentTextBox = GameManager.Instance.CreateTextBox(currentCutscene.text, v);
        Debug.Log("currentText: " + currentTextBox.texts.Count);
        /*}*/
    }

    private void InitializeSpike(GameObject go)
    { 
        SpikeData sp = go.GetComponent<SpikeData>();
        if (sp == null) Debug.LogError("Hazard has no SpikeData!");
        else
        {
            if (sp.useVectorInstead)
                spikeDestination = sp.spikeDestinationVector;
            else
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

    private void EndOfCutscene(int id)
    {
        if (id == 0)
        {
            // do  nothing :)
            return;
        }
        if (id == 1)
        {
            // spawn a single worm
            Vector3 v = this.transform.position + new Vector3(2.0f, 0, 0);
            GameManager.Instance.CreateWorm(v, true);
            GameManager.Instance.SpawnSplosion(v);
            Destroy(currentCutscene.transform.Find("DummyWorm").gameObject);
            //Destroy(GameObject.Find("DummyWorm"));
        }
    }

    public bool SetPlayerState(PlayerState state)
    {
        if (working) return false;

        playerState = state;
        working = true;

        switch(state)
        {
            case PlayerState.NONE:
                // should not be allowed lol
                break;
            case PlayerState.DIGGING:
                SetCurrentState(PLAYER_DIG);
                rb.velocity = Vector3.zero;
                Invoke("Search", digTime);
                break;
            case PlayerState.POURING:
                SetCurrentState(PLAYER_SHOOT);
                Invoke("SpawnWormPile", wormShootTime);
                break;
            case PlayerState.CRAFTING:
                break;
            case PlayerState.PUNCHING:
                break;
            case PlayerState.CRAFTING_MENU:
                break;
        }

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

        if (statename + str == currentAnimationState) return;

        animator.Play(statename + str);

        currentAnimationState = statename + str;
    }
}
