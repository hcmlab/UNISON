using UnityEngine;

namespace Stations
{
    public class SpawnArea : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            var currentTransform = transform;
            Gizmos.DrawCube(currentTransform.position, currentTransform.localScale);
        }

        public Vector3 GetRandomPosition()
        {
            var currentTransform = transform;
            var origin = currentTransform.position;
            var range = currentTransform.localScale / 2.0f;
            var randomRange = new Vector3(
                Random.Range(-range.x, range.x),
                Random.Range(-range.y, range.y),
                Random.Range(-range.z, range.z)
            );
            return origin + randomRange;
        }
    }
}