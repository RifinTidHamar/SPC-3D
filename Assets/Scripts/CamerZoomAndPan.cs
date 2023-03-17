using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerZoomAndPan : MonoBehaviour
{
    public Camera cam;

    // Update is called once per frame
    void Update()
    {
        Debug.Log(cam.transform.rotation);
        if (cam.orthographicSize >= 0.1f && cam.orthographicSize <= 100)
            cam.orthographicSize -= Input.mouseScrollDelta.y * 0.5f;
        else if (cam.orthographicSize <= 0.1f)
            cam.orthographicSize = 0.1f;
        else if (cam.orthographicSize >= 100)
            cam.orthographicSize = 100;

        if (Input.GetMouseButton(2))
        {
            Quaternion camRot = cam.transform.rotation;
            Cursor.lockState = CursorLockMode.Locked;
            cam.transform.Translate( new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")), Space.Self);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
        //-2.392152 - (-2.3836)
    }
}
