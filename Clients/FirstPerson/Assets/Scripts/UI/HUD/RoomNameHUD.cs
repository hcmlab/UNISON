using TMPro;
using UnityEngine;
using World;

namespace UI.HUD
{
    /// <summary>
    /// Shows the name of the current pun room
    /// </summary>
    public class RoomNameHUD : MonoBehaviourHUD
    {
        [SerializeField] private TMP_Text roomName;

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            roomName.text = WorldManager.RoomName;
        }
    }
}