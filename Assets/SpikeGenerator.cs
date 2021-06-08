﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeGenerator : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("How many spikes should be generated in X direction (including this spike). Negative will generate in negative direction.")]
    [SerializeField] int xSpikes;
    [Tooltip("How many spikes should be generated in Z direction (including this spike). Negative will generate in negative direction.")]
    [SerializeField] int zSpikes;
    [Tooltip("The transform that will be used for all spikes generated by this Generator.")]
    [SerializeField] Transform spikeDestination;
    [Tooltip("When true, the spike generator will use a vector relative to the generator in place of a transform.")]
    [SerializeField] bool useVectorInstead;
    [Tooltip("When true, the spike generator will use a vector relative to the generator in place of a transform.")]
    [SerializeField] Vector3 vector;

    [Header("Unity Components")]
    [Tooltip("Spike prefab.")]
    [SerializeField] GameObject spikeObject;

    // Start is called before the first frame update
    void Start()
    {
        // determine directions
        int xDir = (xSpikes < 0) ? -1 : 1;
        int zDir = (zSpikes < 0) ? -1 : 1;

        // abs the values (for the loops)
        xSpikes = Mathf.Abs(xSpikes);
        zSpikes = Mathf.Abs(zSpikes);

        for(int x = 0; x < xSpikes; x++)
        {
            for(int z = 0; z < zSpikes; z++)
            {
                if (x == z && x == 0) continue; // this spike will act as the 0,0 spike

                Vector3 pos = this.transform.position;
                pos.x += x * xDir * this.transform.localScale.x;
                pos.z += z * zDir * this.transform.localScale.z;

                GameObject go = Instantiate(spikeObject, pos, Quaternion.identity, this.transform);
                SpikeData spd = go.GetComponent<SpikeData>();
                if (spd == null)
                    Debug.LogError("No SpikeData on Spike prefab!!");
                else
                {
                    if (useVectorInstead)
                    {
                        spd.spikeDestinationVector = this.transform.position + vector;
                        spd.useVectorInstead = true;
                    }
                    else
                    {
                        spd.SpikeDestination = spikeDestination;
                        spd.useVectorInstead = false;
                    }
                }
            }
        }

        // Destroy this script (not needed after)
        Destroy(this);
    }
}