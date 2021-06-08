using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormPile : MonoBehaviour
{
    [SerializeField] Sprite[] frames;
    [SerializeField] SpriteRenderer spr;
    private bool animating;
    private int currentFrame;
    private int counter;

    public int wormCount;

    private void Start()
    {
        animating = true;
        currentFrame = 0;
    }

    public void FixedUpdate()
    {
        if (animating)
        {
            counter++;
            if (currentFrame < frames.Length - 1 && counter % 5 == 0)
            {
                currentFrame++;
                spr.sprite = frames[currentFrame];
            }
            else if (currentFrame == frames.Length)
            {
                animating = false;
            }
        }
    }

    /// <summary>
    /// Dismiss the worms that the pile consists of.
    /// </summary>
    public void Dismiss()
    {
        GameManager.Instance.CreateWorm(this.transform.position + new Vector3(0, 1.0f, 0), true, wormCount);
        Destroy();
    }

    /// <summary>
    /// Destroy the pile of worms. This consumes the worms but still plays the animation.
    /// </summary>
    public void Destroy()
    {
        GameManager.Instance.SpawnSplosion(this.transform.position + new Vector3(0, 1.0f, 0));
        Destroy(this.gameObject);
    }

    public void FlipX()
    {
        spr.flipX = true;
    }
}
