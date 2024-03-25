using UnityEngine;
using UnityEngine.UI;

namespace UI.Auxiliary
{
    public class LinearProgressBar : MonoBehaviour
    {
        private Image linearProgressbarImage;

        public void SetProgress(int i)
        {
            linearProgressbarImage.fillAmount = i;
        }
    }
}