using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeDShrub : MonoBehaviour
{
    [Header("A list of all 4 shrub part references as children to the object with this component.")]
    [SerializeField] SpriteRenderer[] shrubParts;
    [SerializeField] bool isRock;
    [SerializeField] bool manualOverride;

    // Start is called before the first frame update
    void Start()
    {
        if (!manualOverride)
            isRock = GameManager.Instance.environmentManager.caveLevel;

        Sprite[] sprites;
        if (isRock)
            sprites = GameManager.Instance.halfRocks;
        else
            sprites = GameManager.Instance.halfShrubs;

        int i = Random.Range(0, sprites.Length);
        if (i % 2 != 0)
            i--;
        // thus i, i+1 are the respective half shrubs

        Debug.Log("i:" + i);
        shrubParts[0].sprite = sprites[i];
        shrubParts[1].sprite = sprites[i];
        shrubParts[2].sprite = sprites[i + 1];
        shrubParts[3].sprite = sprites[i + 1];
    }
}
