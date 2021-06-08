using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDecoration : MonoBehaviour
{
    [Header("Decoration Options")]
    [Tooltip("The sprite for this decoration.")]
    [SerializeField] public Sprite sprite;
    [Tooltip("When true, a random decoration is chosen according to the level type.")]
    [SerializeField] public bool chooseRandomDecoration;
    [Tooltip("Value is passed through to SpriteRenderer.flipX.")]
    [SerializeField] public bool flip;
    [Tooltip("When true, this sprite has a 50% chance to be flipped.")]
    [SerializeField] public bool randomFlip;

    private SpriteRenderer spr;

    private void Start()
    {
        Sprite _spr;

        // fetch decoration from game manager
        if (chooseRandomDecoration)
        {
            if (GameManager.Instance.environmentManager.caveLevel)
            {
                _spr = GameManager.Instance.caveDecorations[Random.Range(0, GameManager.Instance.caveDecorations.Length)];
            }
            else
            {
                _spr = GameManager.Instance.forestDecorations[Random.Range(0, GameManager.Instance.forestDecorations.Length)];
            }
        }
        else
            _spr = sprite;
        
        spr = GetComponentInChildren<SpriteRenderer>();

        SetFlipX(randomFlip ? (Random.value < 0.5f ? true : false) : flip);
        SetSprite(_spr);
    }

    public void FetchSpriteReference()
    {
        spr = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetFlipX(bool flip)
    {
        spr.flipX = flip;
    }

    public void SetSprite(Sprite sprite)
    {
        this.sprite = sprite;
        spr.sprite = sprite;
    }
}
