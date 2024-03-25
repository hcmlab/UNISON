using UnityEngine;
using Photon.Pun;

namespace PUN
{
    public class PunMaster : PunClient
    {
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Transform.position, 0.5f);
        }
    }
}