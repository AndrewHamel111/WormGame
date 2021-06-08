using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkingTing : MonoBehaviour
{
    [SerializeField] bool useImage;
    [SerializeField] Image img;
    bool flag = false;
    uint counter = 0;

    private void FixedUpdate()
    {
        counter++;
        if (counter % 60 == 0)
        {
            flag = !flag;
            if (useImage)
                img.enabled = flag;
        }
    }
}
