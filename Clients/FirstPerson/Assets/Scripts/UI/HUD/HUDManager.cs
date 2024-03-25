using UnityEngine;
using World;

namespace UI.HUD
{
    /// <summary>
    /// Holds the different prefabs for client, master and player hud elements
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [SerializeField] private MonoBehaviourHUD[] clientPrefabs;
        [SerializeField] private MonoBehaviourHUD[] masterPrefabs;
        [SerializeField] private MonoBehaviourHUD[] playerPrefabs;

        public void Setup(WorldManager worldManager, bool isMaster)
        {
            InstantiatePrefabs(clientPrefabs, worldManager);
            InstantiatePrefabs(isMaster ? masterPrefabs : playerPrefabs, worldManager);
        }

        private void InstantiatePrefabs(MonoBehaviourHUD[] prefabs, WorldManager worldManager)
        {
            foreach (MonoBehaviourHUD prefab in prefabs)
            {
                MonoBehaviourHUD hudObject = Instantiate(prefab, transform);
                hudObject.OnBeingEnabled(worldManager);
            }
        }

        public void Clear()
        {
            foreach (MonoBehaviourHUD hudObject in transform.GetComponentsInChildren<MonoBehaviourHUD>())
            {
                hudObject.OnBeingDisabled();
                Destroy(hudObject.gameObject);
            }
        }
    }
}