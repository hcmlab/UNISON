using Clients;
using PUN;
using Stations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI.HUD
{
    /// <summary>
    ///     A single element in the [PlayerListHUD]
    /// </summary>
    public class PlayerListTile : MonoBehaviour
    {
        public AudioSource audioSource;
        public TMP_Text PlayerText;
        public Image HelpBorderImage;
        public TMP_Text HelpText;
        public Image StationImage;
        private MasterManager masterManager;
        private bool needsHelp;
        private PunPlayer punPlayer;

        private void Update()
        {
            // TODO: Optimize with subscriber
            if (punPlayer != null && needsHelp != punPlayer.NeedsHelp)
            {
                needsHelp = punPlayer.NeedsHelp;
                ToggleHelp(needsHelp);
            }

            SetStationImage(punPlayer.CurrentStation);
        }

        public void Setup(WorldManager worldManager, PunPlayer player)
        {
            masterManager = worldManager.MasterManager;
            punPlayer = player;
            SetPlayerText(player.Name);
        }

        private void SetPlayerText(string text)
        {
            PlayerText.text = text;
        }

        private void SetStationImage(Station station)
        {
            if (station == null)
            {
                return;
            }

            var s = Resources.Load<Sprite>($"Sprites/StationIcons/{station.Type}");
            StationImage.sprite = s;
            StationImage.color = Color.black;
        }

        public void OnJumToPressed()
        {
            masterManager.FadeToStation(punPlayer.CurrentStation, punPlayer.Transform.position);
            punPlayer.NeedsHelp = false;
        }

        private void ToggleHelp(bool isActive)
        {
            if (isActive)
            {
                audioSource.Play();
            }

            HelpBorderImage.gameObject.SetActive(isActive);
            HelpText.gameObject.SetActive(isActive);
        }
    }
}