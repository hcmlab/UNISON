using System.Collections.Generic;
using PUN;
using UnityEngine.UI;
using World;

namespace UI.HUD
{
    /// <summary>
    /// A list of currently active players in the MasterHUD. With this, the master can see the current station of the player and can directly jump to the player.
    /// </summary>
    public class PlayerListHUD : ScrollingHUD, ICollectionSubscriber<PunPlayer>
    {
        public PlayerListTile playerListTilePrefab;
        private Dictionary<PunPlayer, PlayerListTile> instantiatedPlayerListTiles = new Dictionary<PunPlayer, PlayerListTile>();

        public VerticalLayoutGroup VerticalLayoutGroup;

        private void Awake()
        {
            instantiatedPlayerListTiles = new Dictionary<PunPlayer, PlayerListTile>();
        }

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            CurrentWorldManager.SubscribeToPlayers(this, true);
        }

        public override void OnBeingDisabled()
        {
            CurrentWorldManager.SubscribeToPlayers(this);
        }

        public void OnAdded(PunPlayer player)
        {
            if (instantiatedPlayerListTiles.TryGetValue(player, out _)) return;
            if (!VerticalLayoutGroup) return;

            PlayerListTile tile = Instantiate(playerListTilePrefab, VerticalLayoutGroup.transform);
            tile.Setup(CurrentWorldManager, player);
            instantiatedPlayerListTiles[player] = tile;

        }

        public void OnRemoved(PunPlayer player)
        {
            if (!instantiatedPlayerListTiles.TryGetValue(player, out PlayerListTile tile)) return;
            if (tile)
            {
                Destroy(tile.gameObject);
            }
            instantiatedPlayerListTiles.Remove(player);
        }
    }
}
