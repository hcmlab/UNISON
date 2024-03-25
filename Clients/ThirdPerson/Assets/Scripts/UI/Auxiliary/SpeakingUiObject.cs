using PUN;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Auxiliary
{
    public class SpeakingUiObject : MonoBehaviour, IStateSubscriber<VoiceState>
    {
        [SerializeField] private PunPlayer punPlayer;
        [SerializeField] private Image playerSpeaking;
        [SerializeField] private Image playerMute;
        [SerializeField] private TMP_Text playerNameText;

        private void Start()
        {
            playerNameText.text = punPlayer.Name;
            punPlayer.voiceClient.SubscribeToVoiceState(this, true);
        }

        private void Update()
        {
            gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
        }

        private void OnDestroy()
        {
            punPlayer.voiceClient.UnsubscribeFromVoiceState(this);
        }

        public void OnUpdated(VoiceState voiceState, VoiceState oldVoiceState)
        {
            playerSpeaking.enabled = voiceState == VoiceState.Speaking;
            playerMute.enabled = voiceState == VoiceState.Mute || voiceState == VoiceState.Unavailable;
        }
    }
}