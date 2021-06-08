using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO resolve some of the constants to fields in inspector for better tweaking of the visuals
// TODO replace halfWidth in CloudData with width and move all pivots to left center across all sprites

// handles the foreground/background parallaxes
public class EnvironmentManager : MonoBehaviour
{
    // constants
    [SerializeField] int cloudBuffer; // min distance between two clouds
    [SerializeField] int cloudDiff; // Max distance between two clouds
    [SerializeField] float cloudSpeed; // Speed clouds move at (Velocity.x value)

    [SerializeField] Camera mainCamera; // Managed in inspector; Reference to the Main Camera.

    [Header("Clouds")]
    [SerializeField] Sprite[] cloudSprites; // Managed in inspector; a list
    [SerializeField] GameObject cloudObject; // Managed in inspector; prefab for cloud objects
    CloudData lastCloud; // A reference to the script attached to the most recently created cloud. // may be deprecated since currentClouds's last element should match it
    List<CloudData> currentClouds; // A list of all clouds in the scene currently.
    [SerializeField] Sprite[] streakSprites; // Managed in inspector; a list

    [Header("Shrubs & Hedges")]
    [SerializeField] Sprite[] shrubSprites; // Managed in inspector; a list
    [SerializeField] Sprite[] hedgeSprites; // Managed in inspector; a list
    [SerializeField] Sprite[] rockSprites; // Managed in inspector; a list
    [SerializeField] Sprite[] stalSprites; // Managed in inspector; a list
    [SerializeField] GameObject shrubObject; // Managed in inspector; prefab for shrub objects
    CloudData lastShrub; // Same as lastCloud but for Shrubs.
    List<CloudData> allShrubs; // A list of all shrubs in the scene currently.

    [Header("Environment Variables")]
    [Tooltip("Leftmost point for shrubs in world coordinates.")]
    [SerializeField] double leftEdge;
    [Tooltip("Rightmost point for shrubs in world coordinates.")]
    [SerializeField] double rightEdge;
    [Tooltip("When true, GenerateShrub is replaced with GenerateRock and clouds are disabled.")]
    [SerializeField] public bool caveLevel;

    [Header("Shrub Values")]
    [Tooltip("When true, rightEdge is ignored and the game will always generate ShrubCount many shrub instead.")]
    [SerializeField] bool fixedShrubCount;
    [Tooltip("Number of shrubs generated when Fixed Shrub Count is true.")]
    [SerializeField] int shrubCount;
    [Tooltip("Space between shrubs.")]
    [SerializeField] double shrubOffset;
    [SerializeField] float xScaleVariance;
    [SerializeField] float yScaleVariance;
    [SerializeField] float yPosVariance;
    [SerializeField] float zPosVariance;
    [SerializeField] float yPos;
    [SerializeField] float zPos;
    [SerializeField] float cloudY;
    [SerializeField] float cloudZ;
    [SerializeField] float hedgeZOffset;

    [Header("Cave Parameters")]
    [SerializeField] float stalagmiteChance = 1.0f;
    [SerializeField] float stalagtiteChance = 1.0f;
    [SerializeField] Vector3 stalagtiteOffset;

    // Runtime Variables
    private bool hedgeNext = false; //  when true, the next shrub will be a hedge instead

    void Start()
    {
        float x = 0;
        Vector3 pos;
        allShrubs = new List<CloudData>();

        if (!caveLevel)
        {
            currentClouds = new List<CloudData>();
            this.transform.position = Vector3.zero;

            // create a full screen's worth of clouds
            pos = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
            pos.y = cloudY;
            pos.z = cloudZ;

            while (mainCamera.WorldToScreenPoint(pos).x < Screen.width + 2 * cloudBuffer)
            {
                GenerateCloud();
                pos = new Vector3(lastCloud.transform.position.x + lastCloud.halfWidth + cloudBuffer, cloudY, cloudZ);
            }
        }

        // spawn all the shrubs for the level
        x = (float)leftEdge;
        pos = Vector3.zero;
        pos.y = 1.0f;
        pos.z = zPos;

        int i = 0;
        while(fixedShrubCount ? i < shrubCount : pos.x < rightEdge)
        {
            GenerateShrub();
            pos = new Vector3(lastShrub.transform.position.x + lastShrub.halfWidth + (float)shrubOffset, 1, zPos);
            i++;
        }

        // start a repeating invoke to check clipping on clouds
        if (!caveLevel)
            InvokeRepeating("CheckClouds", 1.0f, 1.0f);
    }

    void GenerateCloud()
    {
        // determine location
        float x;
        if (lastCloud == null)
            x = mainCamera.ScreenToWorldPoint(new Vector3(0 - Screen.width/2,0, 15)).x;
        else
            x = lastCloud.transform.position.x + lastCloud.halfWidth + Random.Range(cloudBuffer, cloudDiff);

        // determine cloud type
        ushort i = (ushort)Random.Range(0, cloudSprites.Length);
        Sprite spr = cloudSprites[i];

        // determine cloud position
        Vector3 pos = Vector3.zero;
        float hw = spr.bounds.size.x / 2;
        x += hw; // offset location by cloud width
        pos.x = x;
        pos.y = cloudY + Random.Range(4.25f, 6.25f);
        pos.z = cloudZ;

        // instantiate the cloud and add some references to the cloud data
        GameObject go = Instantiate(cloudObject, pos, Quaternion.Euler(15, 0, 0), mainCamera.transform);
        CloudData cloud = go.GetComponent<CloudData>();
        if (cloud == null)
            Debug.LogError("CloudData absent from Cloud prefab!");
        else
        {
            cloud.index = i;
            cloud.halfWidth = hw;
        }

        // assign sprite to renderer
        SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("SpriteRenderer absent from Cloud prefab!");
        else
            spriteRenderer.sprite = spr;

        // give the cloud a velocity
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("Rigidbody absent from Cloud prefab!");
        else
            rb.velocity = new Vector3(0 - cloudSpeed, 0, 0);

        currentClouds.Add(cloud);
        lastCloud = cloud;
    }

    void GenerateShrub()
    {
        float x;
        if (lastShrub == null)
            x = (float)leftEdge;
        else
        {
            if (caveLevel)
                x = lastShrub.transform.position.x + 2 * lastShrub.halfWidth + 0.5f * (float)shrubOffset;
            else
                x = lastShrub.transform.position.x + 2 * lastShrub.halfWidth + (float)shrubOffset;
        }

        // determine shrub type
        ushort i = 0; // initialize to 0 since they'll always have at least one.. right?
        Sprite spr = shrubSprites[0]; // initialize to a default value to quell warnings 
        if (hedgeNext)
        {
            if (caveLevel)
            {
                i = (ushort)Random.Range(0, stalSprites.Length);
                spr = stalSprites[i];
            }
            else
            {
                i = (ushort)Random.Range(0, hedgeSprites.Length);
                spr = hedgeSprites[i];
            }
        }
        else
        {
            if (caveLevel)
            {
                i = (ushort)Random.Range(0, rockSprites.Length);
                spr = rockSprites[i];
            }
            else
            {
                i = (ushort)Random.Range(0, shrubSprites.Length);
                spr = shrubSprites[i];
            }
        }

        // determine shrub position
        Vector3 pos = Vector3.zero;
        float hw = spr.bounds.size.x / 2;
        pos.x = x;
        pos.y = yPos + Random.Range(0 - yPosVariance, yPosVariance);
        pos.z = zPos + Random.Range(0 - zPosVariance, zPosVariance);
        if (hedgeNext)
            pos.z += hedgeZOffset;

        // instantiate the cloud and add some references to the cloud data
        GameObject go = Instantiate(shrubObject, pos, Quaternion.Euler(15, 0, 0), this.transform);
        float xScale = Random.Range(1.0f - xScaleVariance, 1.0f);
        float yScale = Random.Range(1.0f - yScaleVariance, 1.0f);
        if (hedgeNext)
            xScale = yScale = 1.0f;
        go.transform.localScale = new Vector3(xScale, yScale, 1);
        CloudData shrub = go.GetComponent<CloudData>();
        if (shrub == null)
            Debug.LogError("CloudData absent from Shrub prefab!");
        else
        {
            shrub.index = i;
            shrub.halfWidth = hw * xScale;
        }

        // assign sprite to renderer
        SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("SpriteRenderer absent from Shrub prefab!");
        else
        {
            spriteRenderer.sprite = spr;
            if(caveLevel)
            {
                spriteRenderer.sortingOrder = (hedgeNext) ? 0 : (1 + (allShrubs.Count % 2));
            }
            else
                spriteRenderer.sortingOrder = (!hedgeNext) ? 2 : 1;
        }

        allShrubs.Add(shrub);
        lastShrub = shrub;

        if (caveLevel && hedgeNext && Random.value < stalagtiteChance)
        {
            pos.y = pos.z = 0;
            pos += stalagtiteOffset;
            GameObject _go = Instantiate(shrubObject, pos, Quaternion.Euler(15, 0, 0), this.transform);

            i = (ushort)Random.Range(0, stalSprites.Length);
            spr = stalSprites[i];
            spriteRenderer = _go.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = spr;
            spriteRenderer.flipY = true;
        }

        if (caveLevel)
            hedgeNext = (allShrubs.Count % 3) == 0 && Random.value < stalagmiteChance;
        else
            hedgeNext = !hedgeNext;
    }

    void CheckClouds()
    {
        Vector3 pos = currentClouds[0].transform.position;
        pos.x += currentClouds[0].halfWidth;
        if (mainCamera.WorldToScreenPoint(pos).x < 0)
        {
            CloudData cloud = currentClouds[0];
            currentClouds.RemoveAt(0);
            Destroy(cloud.gameObject);
            GenerateCloud();
        }
    }
}
