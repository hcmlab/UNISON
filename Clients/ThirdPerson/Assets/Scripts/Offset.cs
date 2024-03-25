using UnityEngine;

public class Offset : MonoBehaviour
{
    public Transform Transform;
    public Vector3 offset;

    private void Update()
    {
        if (Transform == null)
        {
            return;
        }

        var t = transform;
        t.position = Transform.position + t.rotation * offset;
    }
}