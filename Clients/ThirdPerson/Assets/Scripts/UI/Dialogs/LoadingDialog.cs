using UnityEngine;

namespace UI.Dialogs
{
    public class LoadingDialog : MonoBehaviour
    {
        [SerializeField] private RectTransform loadingImageRectTransform;
        [SerializeField] private float rotationSpeed = 200f;

        private void Update()
        {
            loadingImageRectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
}