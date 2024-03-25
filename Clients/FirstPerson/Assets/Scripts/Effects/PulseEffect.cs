using System.Collections;
using UnityEngine;

namespace Effects
{
    public class PulseEffect : MonoBehaviour
    {
        private const float InitialSpeed = 0.005f;

        // Grow parameters
        public float approachSpeed = 0.005f;
        public float growthBound = 1.1f;
        public float shrinkBound = 0.9f;
        private float currentRatio = 1;

        // Attach the coroutine
        private void Awake()
        {
            // Then start the routine
            StartCoroutine(Pulse());
        }

        private IEnumerator Pulse()
        {
            // Run this indefinitely
            while (true)
            {
                // Get bigger for a few seconds
                while (currentRatio != growthBound)
                {
                    // Determine the new ratio to use
                    currentRatio = Mathf.MoveTowards(currentRatio, growthBound, approachSpeed);
                    transform.localScale = Vector3.one * currentRatio;

                    yield return new WaitForFixedUpdate();
                }

                // Shrink for a few seconds
                while (currentRatio != shrinkBound)
                {
                    // Determine the new ratio to use
                    currentRatio = Mathf.MoveTowards(currentRatio, shrinkBound, approachSpeed);
                    transform.localScale = Vector3.one * currentRatio;
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        public void SetSpeedSlow() => approachSpeed = 0.001f;
        public void SetSpeedNormal() => approachSpeed = InitialSpeed;
    }
}