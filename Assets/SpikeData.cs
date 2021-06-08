using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeData : MonoBehaviour
{
    [SerializeField] private Transform spikeDestination; // location player will be sent to after hitting the spike
    public Transform SpikeDestination { get => spikeDestination; set => spikeDestination = value; }

    [Tooltip("When true, spikeDestination is specified by a position instead of a transform. Good for testing or more static designs.")]
    [SerializeField] public bool useVectorInstead;

    [SerializeField] public Vector3 spikeDestinationVector;
}
