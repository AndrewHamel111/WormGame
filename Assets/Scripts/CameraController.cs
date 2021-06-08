using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] PlayerController player;
    [Tooltip("A vector such that player.pos + defaultOffset = camera.pos.")]
    [SerializeField] Vector3 defaultOffset;
    [SerializeField] Vector3 cameraEulers;
    private Quaternion defaultRotation;

    public bool cameraInMotion; // may or may not be useful 
    public bool inCutscene; // when true, the camera is being controlled by some sort of cutscene and as such the values of defaultRotation, defaultOffset are not used.
    public readonly float DEFAULT_CAMERA_MOVE_TIME = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.Instance.player;
        if (player == null)
            Debug.LogError("PlayerController null in CameraController.");

        cameraInMotion = false;

        defaultRotation = Quaternion.identity;
        defaultRotation.eulerAngles = cameraEulers;
    }

    // Update is called once per frame
    void Update()
    {
        // update camera position to player location 
        if (!cameraInMotion && !inCutscene)
        {
            Vector3 pos = player.transform.position;
            pos.y = 2;
            this.transform.position = pos + defaultOffset;
            this.transform.rotation = defaultRotation; 
        }
    }

    public IEnumerator CameraMotion(CameraMove move)
    {
        float timeElapsed = 0;
        cameraInMotion = true;

        while ( timeElapsed < move.t)
        {
            float t = timeElapsed / move.t;
            // t = Mathf.Lerp
            this.transform.position = Vector3.Slerp(move.startPos, move.endPos, Mathf.Lerp(0, 1, t));
            this.transform.rotation = Quaternion.Lerp(move.startRot, move.endRot, t);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        this.transform.position = move.endPos;
        this.transform.rotation = move.endRot;
        cameraInMotion = false;
    }

    public CameraMove GetMove()
    {
        CameraMove move = new CameraMove();
        move.startPos = move.endPos = this.transform.position;
        move.startRot = move.endRot = this.transform.rotation;
        move.t = DEFAULT_CAMERA_MOVE_TIME;

        return move;
    }
}
