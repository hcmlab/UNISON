using UnityEngine;

namespace PUN
{
    public class PunMaster : PunClient
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Transform.position, 0.5f);
        }

        protected override void StartClient()
        {
            CurrentWorldManager.RegisterMaster(this);
        }

        protected override void OnDestroyClient()
        {
            if (CurrentWorldManager)
            {
                CurrentWorldManager.UnregisterMaster(this);
            }
        }
    }
}