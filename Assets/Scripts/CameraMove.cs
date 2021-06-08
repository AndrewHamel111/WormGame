using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[CreateAssetMenu(fileName = "CutsceneData", menuName = "Cutscenes/Camera Move")]
public class CameraMove : ScriptableObject
{
    public Vector3 startPos; // Start position of Camera
    public Quaternion startRot; // Start rotation of Camera
    public Vector3 endPos; // End position of Camera
    public Quaternion endRot; // End rotation of Camera
    public float t; // Time the move should take in seconds.
}
*/

public struct CameraMove
{
    public Vector3 startPos; // Start position of Camera
    public Quaternion startRot; // Start rotation of Camera
    public Vector3 endPos; // End position of Camera
    public Quaternion endRot; // End rotation of Camera
    public float t; // Time the move should take in seconds.

    /*
    public CameraMove()
    {
        startPos = endPos = Vector3.zero;
        startRot = endRot = Quaternion.identity;
        t = 1.0f;
    }
    */
}
