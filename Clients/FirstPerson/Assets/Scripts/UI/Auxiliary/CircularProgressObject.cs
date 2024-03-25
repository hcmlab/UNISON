using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Auxiliary
{
    public class CircularProgressObject : MonoBehaviour
    {
        public TMP_Text TextWidget;
        public Image ImageWidget;
        public bool IsTextBased = true;
        public Image ImageProgress;

        private void Awake()
        {
            // Set this value in the gameobject hierarchy / Unity editor
            if (IsTextBased)
            {
                ImageWidget.gameObject.SetActive(false);
            }
            else
            {
                TextWidget.gameObject.SetActive(false);
            }
        }

        public void SetRemainingSecondsText(float seconds)
        {
            if (seconds == 0f)
            {
                gameObject.SetActive(false);
                //TextWidget.gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                //TextWidget.gameObject.SetActive(true);
                TextWidget.text = seconds.ToString("N0", CultureInfo.InvariantCulture);
            }
        }

        public void SetPercentage(float progress) => ImageProgress.fillAmount = progress;

        public void SetImageSource(Sprite sprite, Color color)
        {
            ImageWidget.sprite = sprite;
            ImageWidget.color = color;
        }
    }
}