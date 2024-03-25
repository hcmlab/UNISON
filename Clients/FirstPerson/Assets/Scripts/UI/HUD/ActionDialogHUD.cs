using TMPro;
using UI.Auxiliary;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI.HUD
{
    /// <summary>
    /// Shows which action the user can do on this [Interactable] and which key he must use
    /// </summary>
    public class ActionDialogHUD : MonoBehaviourHUD
    {
        [SerializeField] private CanvasGroup canvasGroup;
        public GameObject actionObject;

        public TMP_Text keyText;
        public TMP_Text reasonForInactiveText;

        public Image inactiveImage;

        public Image WholeBgImage;
        public Image KeyBgImage;
        public Image ActionBgImage;

        public Image ActionImageDoor;
        public Image ActionImage;

        public CircularProgressObject CircularProgressObject;

        public Color ColorActiveDark;
        public Color ColorInactiveDark;
        public Color ColorActiveLight;
        public Color ColorInactiveLight;

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);
            
            inactiveImage.gameObject.SetActive(false);
            canvasGroup.alpha = 0;
        }

        public void Show(Sprite sprite, KeyCode code)
        {
            if (sprite.name == "door_open")
            {
                ActionImageDoor.gameObject.SetActive(true);
                ActionImage.gameObject.SetActive(false);
                ActionImageDoor.sprite = sprite;
            }
            else
            {
                ActionImage.gameObject.SetActive(true);
                ActionImageDoor.gameObject.SetActive(false);
                ActionImage.sprite = sprite;
            }

            keyText.text = code.ToString();

            canvasGroup.alpha = 1;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0;
        }

        public void SetRemainingCooldown(float coolDown, float max, bool isActive, string reasonForInactive)
        {
            CircularProgressObject.SetPercentage(coolDown / max);
            float rounded = Mathf.Ceil(coolDown);
            CircularProgressObject.SetRemainingSecondsText(rounded);
            actionObject.SetActive(rounded <= 0f);
            inactiveImage.gameObject.SetActive(!isActive);

            if (isActive)
            {
                WholeBgImage.color = ColorActiveLight;
                KeyBgImage.color = ColorActiveDark;
                ActionBgImage.color = ColorActiveDark;
                inactiveImage.color = ColorActiveLight;
                keyText.color = ColorActiveLight;
                ActionImage.color = ColorActiveLight;
                ActionImageDoor.color = ColorActiveLight;
                reasonForInactiveText.text = "";
            }
            else
            {
                WholeBgImage.color = ColorInactiveLight;
                KeyBgImage.color = ColorInactiveDark;
                ActionBgImage.color = ColorInactiveDark;
                inactiveImage.color = ColorInactiveLight;
                keyText.color = ColorInactiveLight;
                ActionImage.color = ColorInactiveLight;
                ActionImageDoor.color = ColorInactiveLight;
                reasonForInactiveText.text = reasonForInactive;
            }
        }
    }
}