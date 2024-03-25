using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public GameObject g;

    private void Update()
    {
        g.transform.Rotate(0, 0, 45 * Time.deltaTime);
    }
}