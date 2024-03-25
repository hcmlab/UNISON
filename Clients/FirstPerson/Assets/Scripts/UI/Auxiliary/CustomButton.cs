using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Auxiliary
{
    public enum ButtonState
    {
        Active,
        Inactive,
        Loading,
        Hidden
    }

    public class CustomButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text text;
        [SerializeField] private GameObject loadingImage;
        [SerializeField] private RectTransform loadingImageRectTransform;
        [SerializeField] private float rotationSpeed = 200f;
        [SerializeField] private ButtonState buttonState = ButtonState.Active;

        public Button.ButtonClickedEvent OnClick => button.onClick;

        public bool Interactable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

        public string Text
        {
            get => text.text;
            set => text.text = value;
        }

        public void UpdateButtonState(ButtonState newButtonState)
        {
            buttonState = newButtonState;

            switch (buttonState)
            {
                case ButtonState.Active:
                    button.gameObject.SetActive(true);
                    if (text && text.gameObject)
                    {
                        text.gameObject.SetActive(true);
                    }

                    button.enabled = true;

                    if (loadingImage)
                    {
                        loadingImage.SetActive(false);
                    }

                    break;

                case ButtonState.Inactive:
                    button.gameObject.SetActive(true);
                    if (text && text.gameObject)
                    {
                        text.gameObject.SetActive(true);
                    }

                    button.enabled = false;

                    if (loadingImage)
                    {
                        loadingImage.SetActive(false);
                    }

                    break;

                case ButtonState.Loading:
                    button.gameObject.SetActive(true);
                    if (text && text.gameObject)
                    {
                        text.gameObject.SetActive(true);
                    }

                    button.enabled = false;

                    if (loadingImage)
                    {
                        loadingImage.SetActive(true);
                        loadingImageRectTransform.Rotate(0f, 0f, 0f);
                    }

                    break;

                case ButtonState.Hidden:
                    button.gameObject.SetActive(false);
                    if (text && text.gameObject)
                    {
                        text.gameObject.SetActive(false);
                    }

                    button.enabled = false;

                    if (loadingImage)
                    {
                        loadingImage.SetActive(false);
                    }

                    break;

                default:
                    throw new UnityException($"Unknown button state {buttonState}!");
            }
        }

        private void Start()
        {
            // Update button with initial button state.
            UpdateButtonState(buttonState);
        }

        private void Update()
        {
            if (buttonState == ButtonState.Loading)
            {
                loadingImageRectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
