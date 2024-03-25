using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Transform camera = GameObject.Find("MainCamera").transform;
        this.transform.LookAt(new Vector3(camera.position.x,this.transform.position.y,camera.position.z));
        this.transform.Rotate(Vector3.up, 180f);
    }
}
