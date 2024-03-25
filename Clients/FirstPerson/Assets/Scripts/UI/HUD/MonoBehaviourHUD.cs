using UnityEngine;
using World;

namespace UI.HUD
{
    /// <summary>
    /// All HUD scripts extend this class, because all need the [WorldManager]
    /// </summary>
    public abstract class MonoBehaviourHUD : MonoBehaviour
    {
        protected WorldManager CurrentWorldManager;
        
        public virtual void OnBeingEnabled(WorldManager worldManager)
        {
            CurrentWorldManager = worldManager;
        }

        public virtual void OnBeingDisabled()
        {
        }
    }
}