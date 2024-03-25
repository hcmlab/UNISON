using PUN;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI.HUD
{
    /// <summary>
    /// Shows the client, if he is speaking, muted or idle. And indicates, if the client is speaking globally
    /// </summary>
    public class VoiceStateHUD : MonoBehaviourHUD, IStateSubscriber<VoiceState>, IStateSubscriber<VoiceRegion>
    {
        private VoiceClient voiceClient;

        [SerializeField] private Image soundStateImageLocal;
        [SerializeField] private Image soundStateImageGlobally;

        [SerializeField] private GameObject objectSpeakingGlobally;
        [SerializeField] private GameObject objectSpeakingLocal;

        private Sprite spriteNotSpeaking;
        private Sprite spriteMuted;
        private Sprite spriteSpeaking;

        private void Awake()
        {
            spriteMuted = Resources.Load<Sprite>("Sprites/volume-off");
            spriteSpeaking = Resources.Load<Sprite>("Sprites/volume-high");
            spriteNotSpeaking = Resources.Load<Sprite>("Sprites/volume-medium");
        }

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            voiceClient = CurrentWorldManager.ClientManager.PunClient.voiceClient;
            voiceClient.SubscribeToVoiceState(this, true);
            voiceClient.SubscribeToSpeakingRegion(this, true);
        }

        public override void OnBeingDisabled()
        {
            voiceClient.UnsubscribeFromVoiceState(this);
            voiceClient.UnsubscribeFromSpeakingRegion(this);
        }

        public void OnUpdated(VoiceState voiceState, VoiceState oldVoiceState)
        {
            SetVoiceStateImages(voiceState switch
            {
                VoiceState.Speaking => spriteSpeaking,
                VoiceState.Idle => spriteNotSpeaking,
                _ => spriteMuted
            });
        }

        private void SetVoiceStateImages(Sprite s)
        {
            if (soundStateImageLocal)
            {
                soundStateImageLocal.sprite = s;
            }

            if (soundStateImageGlobally)
            {
                soundStateImageGlobally.sprite = s;
            }
        }

        public void OnUpdated(VoiceRegion speakingRegion, VoiceRegion oldSpeakingRegion)
        {
            if (objectSpeakingGlobally)
            {
                objectSpeakingGlobally.SetActive(speakingRegion == VoiceRegion.Global);
            }

            if (objectSpeakingLocal)
            {
                objectSpeakingLocal.SetActive(speakingRegion == VoiceRegion.Local);
            }
        }
    }
}