using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyWorm : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] SpriteRenderer spriteRenderer; // assigned in inspector
    private bool flag = false;
    private ushort tick = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        tick++;
        if (tick % 80 == 0)
        {
            flag = !flag;
            if (flag)
                spriteRenderer.sprite = sprites[0];
            else
                spriteRenderer.sprite = sprites[1];
        }
    }
}
