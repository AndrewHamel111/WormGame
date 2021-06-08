using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormholeController : MonoBehaviour
{
    // unity components
    [Header("Sprites")]
    [SerializeField] Sprite[] dryPatches;
    [SerializeField] Sprite[] wetPatches;
    [Header("References")]
    [SerializeField] SpriteRenderer topLayer;
    [SerializeField] SpriteRenderer bottomLayer;
    [SerializeField] GameObject wormObject;
    [SerializeField] GameObject wormSplosion;
    [SerializeField] Transform wormRoot;
    [SerializeField] Vector3 wormStartOffset;

    // configuration
    [Header("Configuration")]
    [SerializeField] ushort maxWormCount; // max number of worms this patch will provide
    private ushort wormsSpawned = 0; // max number of worms this patch will provide
    [SerializeField] int wormsPerSearch; // number of worms that spawn after each search

    // runtime vars
    private int spriteIndex;
    private bool isWet = true;
    public ushort searchCount = 0; // number of times this pit has been searched

    void Start()
    {
        /*
        if (dryPatches.Length != wetPatches.Length)
            Debug.LogWarning("Dry Patch list and Wet Patch list have different lengths!");

        */
        spriteIndex = Random.Range(0, dryPatches.Length);
        bottomLayer.sprite = dryPatches[spriteIndex];
        topLayer.sprite = wetPatches[spriteIndex % dryPatches.Length];
    }

    void Update()
    {
        
    }

    public void Search()
    {
        if (!isWet) return;

        if (!GameManager.Instance.wormManager.wormSpawnsEnabled) return;

        ushort wormsToSpawn = (ushort)Mathf.Min(wormsPerSearch, maxWormCount - wormsSpawned);
        wormsSpawned += wormsToSpawn;

        // update opacity of wet layer
        float percent = 1.0f - (float)wormsSpawned / (float)maxWormCount;
        topLayer.color = new Color(1.0f, 1.0f, 1.0f, percent);

        // create the new worm

        GameManager.Instance.CreateWorm(this.transform.position + wormStartOffset, true, wormsToSpawn);
        GameManager.Instance.SpawnSplosion(this.transform.position + wormStartOffset);
        /*
        GameObject go;
        for(int i = 0; i < wormsToSpawn; i++)
        {
            go = Instantiate(wormObject, this.transform.position + wormStartOffset, Quaternion.identity, wormRoot);
            WormController wrm = go.GetComponent<WormController>();
            if (wrm == null)
                Debug.LogError("No WormController on Worm Object prefab!");
            else
            {
                // launch the worm
                wrm.StartLaunch();
                Instantiate(wormSplosion, this.transform.position + new Vector3(0, 1.5f, -2.0f), Quaternion.Euler(15, 0, 0));
            }
        }
        */

        if (wormsSpawned == maxWormCount)
        {
            isWet = false;
            Destroy(topLayer.gameObject);
        }
    }
}
