using UnityEngine;

public class Offset : MonoBehaviour
{
    public Transform Transform;
    public Vector3 offset;
    
    void Update()
    {
        if (Transform == null)
        {
            return;
        }

        Transform t = transform;
        t.position = Transform.position + t.rotation * offset;
    }
}
