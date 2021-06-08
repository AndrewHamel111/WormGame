using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormManager : MonoBehaviour
{
    public int wormCount; // worms in the scene currently
    public int wormCap; // max number of worms allowed in the scene. This is a soft max and as such there are circumstances under which more worms will spawn. For example, equipping boots over a fist will spawn more but a wormhole will not.
    public bool wormSpawnsEnabled;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        wormSpawnsEnabled = wormCount < wormCap;
    }
}
