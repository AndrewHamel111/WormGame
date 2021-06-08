using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] public List<string> text;
    [SerializeField] public Vector3 cameraDest;
    [SerializeField] bool useWorldPos; // When true, cameraDest is a world position. Otherwise, it is relative to the player dest transform.
    [SerializeField] public float cameraMoveTime;
    [SerializeField] public Vector3 textBoxPosition;
    [SerializeField] public bool textBoxPositionIsScreenSpace; // when true, the Vector3's x,y coordinates should be interpreted as a screenspace position.
    //[SerializeField] public Vector3[] cameraPosRelToPlayer; // this vector + player.tranform.position = camera destination
    //[Tooltip("moveTime at i is how fast camera will move from position (i) to (i + 1)")]
    //[SerializeField] public float[] cameraMoveTime;
    [SerializeField] public int id; // unique identifier that when nonzero will cause the end of the cutscene to do special stuff

    //public int index;
    

    /// <summary>
    /// Advances the cutscene to the next position.
    /// </summary>
    /// <returns>True when the cutscene has finished.</returns>
    public bool NextSlide()
    {
        return true;
    }
}
