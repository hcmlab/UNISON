using UnityEngine;

namespace Stations
{
    public class SpawnArea : MonoBehaviour
    {
        public Vector3 GetRandomPosition()
        {
            Transform currentTransform = transform;
            Vector3 origin = currentTransform.position;
            Vector3 range = currentTransform.localScale / 2.0f;
            Vector3 randomRange = new Vector3(
                Random.Range(-range.x, range.x),
                Random.Range(-range.y, range.y),
                Random.Range(-range.z, range.z)
            );
            return origin + randomRange;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Transform currentTransform = transform;
            Gizmos.DrawCube(currentTransform.position, currentTransform.localScale);
        }
    }
}