using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Transform cam;
    // Start is called before the first frame update
    void Awake()
    {
        // find main camera
        cam = Camera.main.transform;

        // disable by default until visible
        enabled = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.forward = cam.transform.forward;
    }

    // copying transform.forward is relatively expensive and slows things down
    // for large amounts of entities, so we only want to do it while the mesh
    // is actually visible
    void OnBecameVisible() { enabled = true; }
    void OnBecameInvisible() { enabled = false; }
}
