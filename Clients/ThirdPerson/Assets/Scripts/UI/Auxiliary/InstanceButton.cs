using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Auxiliary
{
    public class InstanceButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text instanceText;
        [SerializeField] private TMP_Text playerCountText;
        [SerializeField] private CustomButton button;

        private int maxPlayerCount;

        private int PlayerCount
        {
            set => playerCountText.text = value + "/" + maxPlayerCount;
        }

        public void SetupInstanceButton(string text, int playerCount, int lobbySize, UnityAction listener)
        {
            maxPlayerCount = lobbySize;
            instanceText.text = text;
            PlayerCount = playerCount;
            button.OnClick.AddListener(listener);
        }

        public void SetupInstanceButton(string instanceName, UnityAction listener)
        {
            instanceText.text = instanceName;
            button.OnClick.AddListener(listener);
        }
    }
}