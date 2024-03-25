using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Stations;
using System;
using PUN;
using TMPro;
using World;

namespace UI.HUD
{
    /// <summary>
    /// Indicator whether the master can hear the player
    /// </summary>
    public class MasterInRangeHUD : MonoBehaviourHUD
    {
        [SerializeField] private Image stationImage;
        [SerializeField] private TMP_Text stationTitle;
        [SerializeField] private Image masterInRangeImage;
        [SerializeField] private TMP_Text inRangeTitle;

        private Sprite spriteMasterInRange;
        private Sprite spriteMasterNotInRange;
        private Sprite spriteSpeaking;

        private readonly Dictionary<StationType, Sprite> sprites = new Dictionary<StationType, Sprite>();

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            if (WorldManager.IsMasterClient)
            {
                gameObject.SetActive(false);
                return;
            }

            spriteMasterInRange = Resources.Load<Sprite>("Sprites/ear-hearing");
            spriteMasterNotInRange = Resources.Load<Sprite>("Sprites/ear-hearing-off");
            spriteSpeaking = Resources.Load<Sprite>("Sprites/volume-high");
            foreach (StationType st in Enum.GetValues(typeof(StationType)))
            {
                Sprite sprite = Resources.Load<Sprite>($"Sprites/StationIcons/{st}");
                sprites.Add(st, sprite);
            }
        }

        private void Update()
        {
            if (CurrentWorldManager.Master == null)
            {
                return;
            }

            // TODO: Optimize with subscriber
            UpdateMasterInRangeImage();
            UpdateMasterStationImage();
        }

        private void UpdateMasterStationImage()
        {
            StationType st = CurrentWorldManager.Master.CurrentStationType;
            stationTitle.text = st.GetDisplayName();
            stationImage.sprite = sprites[st];
        }

        private void UpdateMasterInRangeImage()
        {
            if (!CurrentWorldManager.ClientManager.PunClient.voiceClient.MasterInRange)
            {
                masterInRangeImage.sprite = spriteMasterNotInRange;
                inRangeTitle.text = LocalizationUtility.GetLocalizedString("cannotHearYou");
            }
            else if (CurrentWorldManager.Master.voiceClient.VoiceState == VoiceState.Speaking)
            {
                masterInRangeImage.sprite = spriteSpeaking;
                inRangeTitle.text = LocalizationUtility.GetLocalizedString("speaking");
            }
            else
            {
                masterInRangeImage.sprite = spriteMasterInRange;
                inRangeTitle.text = LocalizationUtility.GetLocalizedString("canHearYou");
            }
        }
    }
}