using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormSplosion : MonoBehaviour
{
    [SerializeField] Animator animator;
    float t;
    [SerializeField] float duration;

    // Start is called before the first frame update
    void Start()
    {
        t = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - t > duration)
            Destroy(this.gameObject);
    }
}
