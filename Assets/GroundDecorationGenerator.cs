using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDecorationGenerator : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] public int count;

    [Header("Linear Settings")]
    [SerializeField] public bool generateLine;
    [SerializeField] public float distance; // distance between objects generated
    [Tooltip("The direction in which the line generates. This vector should be normalized, since it is directly scaled by distance.")]
    [SerializeField] public Vector3 direction; // direction the objects are generated in
    /*
     * FEATURE CREEP QUARANTINE
    [SerializeField] public bool useVariance; // when true, variance will be used in object generation
    [SerializeField] public float variance; // variance in distance between objects generated
    */

    [Header("Circle/Random Settings")]
    [SerializeField] public float radius; // Radius of circle in world units
    [Tooltip("When true, the decorations don't form a neat circle and instead appear randomly a square with it's half extends defined by the radius.")]
    [SerializeField] bool randomlyDistributeDecorations;

    [Header("Sprite Settings")]
    [SerializeField] public bool randomlyFlip;
    [SerializeField] Sprite[] sprites;
    [Tooltip("When true, the sprites in the circle are randomly picked from the sprites list. Otherwise, they're pulled in order as a cycle.")]
    [SerializeField] bool randomizeSprites;
    [Tooltip("When true, sprites don't need to be specified and instead will be collected from the game manager based on the current level.")]
    [SerializeField] bool useSpritesFromGameManager;

    // Start is called before the first frame update
    void Start()
    {
        if (useSpritesFromGameManager)
        {
            if (GameManager.Instance.environmentManager.caveLevel)
                sprites = GameManager.Instance.caveDecorations;
            else
                sprites = GameManager.Instance.forestDecorations;
        }
        if (generateLine) // line style
        {
            Vector3 pos = this.transform.position;
            GameObject go;
            for(int i = 0; i < count; i++)
            {
                go = Instantiate(GameManager.Instance.groundDecoration, pos, Quaternion.identity, this.transform);
                GroundDecoration gd = go.GetComponent<GroundDecoration>();
                gd.chooseRandomDecoration = false;
                gd.FetchSpriteReference();
                gd.SetFlipX(randomlyFlip ? (Random.value < 0.5f ? true : false) : false);

                if (randomizeSprites)
                    gd.SetSprite(sprites[Random.Range(0, sprites.Length)]);
                else
                    gd.SetSprite(sprites[i % sprites.Length]);

                // generate next position
                pos += direction * distance;
            }
        }
        else if (randomlyDistributeDecorations) // square style
        {
            float _x = this.transform.position.x, _z = this.transform.position.z;
            float x, z = 0;
            Vector3 pos;
            GameObject go;
            for(int i = 0; i < count; i++)
            {
                pos = new Vector3(Random.Range(_x - radius, _x + radius), this.transform.position.y, Random.Range(_z - radius, _z + radius));

                go = Instantiate(GameManager.Instance.groundDecoration, pos, Quaternion.identity, this.transform);
                GroundDecoration gd = go.GetComponent<GroundDecoration>();
                gd.chooseRandomDecoration = false;
                gd.FetchSpriteReference();
                gd.SetFlipX(randomlyFlip ? (Random.value < 0.5f ? true : false) : false);

                if (randomizeSprites)
                    gd.SetSprite(sprites[Random.Range(0, sprites.Length)]);
                else
                    gd.SetSprite(sprites[i % sprites.Length]);
            }
        }
        else // circle style
        {
            float x, z = 0;
            float theta = 0;
            Vector3 pos;
            GameObject go;
            for (int i = 0; i < count; i++)
            {
                theta = (float)i / (float)count * 2 * Mathf.PI + Mathf.PI / 2;
                x = radius * Mathf.Cos(theta);
                z = radius * Mathf.Sin(theta);
                pos = this.transform.position + new Vector3(x, 0, z);

                go = Instantiate(GameManager.Instance.groundDecoration, pos, Quaternion.identity, this.transform);
                GroundDecoration gd = go.GetComponent<GroundDecoration>();
                gd.chooseRandomDecoration = false;
                gd.FetchSpriteReference();
                gd.SetFlipX(randomlyFlip ? (Random.value < 0.5f ? true : false) : false);

                if (randomizeSprites)
                    gd.SetSprite(sprites[Random.Range(0, sprites.Length)]);
                else
                    gd.SetSprite(sprites[i % sprites.Length]);
            }
        }

        Destroy(this);
    }
}
