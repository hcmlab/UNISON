using Photon.Realtime;
using PUN;
using TMPro;
using UI.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Auxiliary
{
    public class PlayerItem : MonoBehaviour, IStateSubscriber<VoiceState>
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private Image muteImage;
        [SerializeField] private Image speakingImage;
        [SerializeField] private Image masterImage;
        private Player player;
        private PunManager punManager;
        private VoiceClient voiceClient;

        private void Start()
        {
            punManager = GetComponentInParent<MainMenu>().punManager;
        }

        private void Update()
        {
            // TODO: Optimize with subscriber
            if (voiceClient == null && player != null && punManager.TryGetVoiceClient(player, out voiceClient))
            {
                voiceClient.SubscribeToVoiceState(this, true);
            }
        }

        private void OnDestroy()
        {
            if (voiceClient != null)
            {
                voiceClient.UnsubscribeFromVoiceState(this);
            }
        }

        public void OnUpdated(VoiceState voiceState, VoiceState oldVoiceState)
        {
            speakingImage.gameObject.SetActive(voiceState == VoiceState.Speaking);
            muteImage.gameObject.SetActive(voiceState == VoiceState.Mute || voiceState == VoiceState.Unavailable);
        }

        public void Setup(Player newPlayer)
        {
            player = newPlayer;
            playerName.text = player.NickName;
            if (player.IsLocal)
            {
                playerName.fontStyle = FontStyles.Bold;
            }

            if (masterImage && masterImage.gameObject)
            {
                masterImage.gameObject.SetActive(player.IsMasterClient);
            }
        }
    }
}