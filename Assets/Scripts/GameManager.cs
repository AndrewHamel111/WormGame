using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    // Constants
    public static int wormPileCost = 10;

    [Header("Prefabs")]
    [SerializeField] public GameObject wormObject;
    [SerializeField] public GameObject wormSplosion;
    [SerializeField] public GameObject wormPile;
    [SerializeField] public GameObject spikeObject;
    [SerializeField] public GameObject shrubObject;
    [SerializeField] public GameObject groundDecoration;

    [Header("Sprites")]
    [SerializeField] public Sprite[] forestDecorations;
    [SerializeField] public Sprite[] forestWormholes;
    [SerializeField] public Sprite[] caveDecorations;
    [SerializeField] public Sprite[] caveWormholes;
    [SerializeField] public Sprite[] halfShrubs;
    [SerializeField] public Sprite[] halfRocks;

    [Header("References")]
    [SerializeField] GameObject textBoxRoot;
    [SerializeField] Text textBoxText;
    [SerializeField] public GameObject craftingGUI;

    // LEVEL FIELDS
    public bool levelFinishing = false;
    public float timeElapsed = 0;
    public float fadeDuration = 3.0f;
    public int wormCount = 1;

    // RUNTIME COMPONENTS
    public Camera mainCamera;
    public CameraController mainCameraController;
    private Transform wormRoot;
    public EnvironmentManager environmentManager;
    public WormManager wormManager;
    public PlayerController player;
    public GameObject fadeout;
    public Image fadeoutImg;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Awake used to enforce singleton pattern. this means the game must start from the main menu, but will then always contain an instance of ScoreManager
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;

            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // fetch main camera
        GameObject go = GameObject.FindGameObjectWithTag("MainCamera");
        if (go == null)
            Debug.LogError("No object tagged with MainCamera found in the scene!");
        else
        {
            Camera c = go.GetComponent<Camera>();
            if (c == null)
                Debug.LogError("Camera component not found on game object tagged with MainCamera!");
            else
                mainCamera = c;

            CameraController cc = go.GetComponent<CameraController>();
            if (cc == null)
                Debug.LogError("CameraController component not found on game object tagged with MainCamera!");
            else
                mainCameraController = cc;
        }

        // fetch worm root (root object for all worms in hierarchy)
        wormRoot = GameObject.FindGameObjectWithTag("WormRoot").transform;
        if (wormRoot == null)
            Debug.LogError("No transform tagged with WormRoot found in scene!");

        // fetch environment manager
        go = GameObject.FindGameObjectWithTag("EnvironmentManager");
        if (go == null)
            Debug.LogError("No object tagged with EnvironmentManager found in the scene!");
        else
        {
            environmentManager = go.GetComponent<EnvironmentManager>();
        }

        // fetch worm manager
        wormManager = wormRoot.GetComponent<WormManager>();
        if (wormManager == null)
            Debug.LogError("No WormManager found on WormRoot!");

        // fetch player
        go = GameObject.FindGameObjectWithTag("Player");
        player = go.GetComponent<PlayerController>();
        if (player == null)
            Debug.LogError("Null player");
        player.WormCount = (ushort)wormCount;

        // fetch textbox root
        textBoxRoot = GameObject.Find("TextBoxRoot");
        if (textBoxRoot == null)
        {
            Debug.LogError("TextBoxRoot is not found in the scene!");
        }

        fadeout = GameObject.FindGameObjectWithTag("FadeoutPanel");
        fadeoutImg = fadeout.GetComponent<Image>();
        if (fadeoutImg == null)
            Debug.LogError("Fadeout Panel missing image component somehow");

        // fetch craftingGUI
        craftingGUI = GameObject.Find("CraftingGUI");
        if (craftingGUI == null)
            Debug.LogError("No crafting GUI found in scene");
        else
            craftingGUI.SetActive(false);

        // temporary fix for the jam. The issue I was having was that the first time I would use CreateTextBox
        // in every scene it wouldn't actually get the texts from the DialogueTrigger. I don't have time to fix
        // it during the jam so I'm happy with the fact that this works and hopefully nothing I add between now
        // and the deadline will cause this fix to stop working.
        List<string> temp = new List<string>() { " " };
        TextBox tb = CreateTextBox(temp, new Vector2(-Screen.width, -Screen.height), false);
        tb.ResetBox();
    }

    // Update is called once per frame
    void Update()
    {
        if (levelFinishing)
        {
            mainCameraController.inCutscene = true;
            timeElapsed += Time.deltaTime;
            fadeoutImg.color = new Color(0.0f, 0.0f, 0.0f, (float)timeElapsed / (float)fadeDuration);
        }
    }

    public void StartLevelChange()
    {
        Invoke("NextLevel", fadeDuration);
    }

    private void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        levelFinishing = false;
    }

    public void SpawnSplosion(Vector3 position)
    {
        Instantiate(wormSplosion, position, Quaternion.Euler(15, 0, 0));
    }

    public void CreateWorm(Vector3 pos, bool launchWorm)
    {
        GameObject go;
        go = Instantiate(wormObject, pos, Quaternion.identity, wormRoot);
        WormController wrm = go.GetComponent<WormController>();
        if (wrm == null)
            Debug.LogError("No WormController on Worm Object prefab!");
        else
        {
            if (launchWorm)
                wrm.StartLaunch();
        }

        wormManager.wormCount++;
    }

    public void CreateWorm(Vector3 pos, bool launchWorm, int wormCount)
    {
        for (int i = 0; i < wormCount; i++)
            CreateWorm(pos, launchWorm);
    }

    public TextBox CreateTextBox(List<string> text, Vector2 screenSpacePos, bool freezePlayer = true)
    {
        TextBox tb = textBoxRoot.GetComponent<TextBox>();
        tb.ResetBox();

        textBoxRoot.transform.position = screenSpacePos;
        //for(int i = 0; i < text.Length; i++)
        //    tb.AddText(text[i]);
        tb.texts = text;
        Debug.Log("texts size " + text.Count + ":" + tb.texts.Count);
        textBoxRoot.SetActive(true);
        return tb;
    }

    public TextBox CreateTextBox(List<string> text, Vector3 worldSpacePos, bool freezePlayer = true)
    {
        // find screen space pos
        Vector2 screenSpacePos = new Vector2(worldSpacePos.x, worldSpacePos.y);
        screenSpacePos = mainCamera.WorldToScreenPoint(worldSpacePos);

        // call other method declaration
        return CreateTextBox(text, screenSpacePos, freezePlayer);
    }

    public WormPile SpawnWormPile(Vector3 pos)
    {
        GameObject go = Instantiate(wormPile, pos, Quaternion.identity);
        WormPile wp = go.GetComponent<WormPile>();
        if (wp == null)
            Debug.LogError("WormPile script not found on WormPile prefab!");
        else
        {
            wp.wormCount = GameManager.wormPileCost;
            return wp;
        }

        return null;
    }

}
