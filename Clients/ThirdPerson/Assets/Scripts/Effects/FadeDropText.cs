using System.Collections;
using TMPro;
using UnityEngine;

namespace Effects
{
    public class FadeDropText : MonoBehaviour
    {
        private TMP_Text text;

        private void Awake()
        {
            text = GetComponentInChildren<TMP_Text>();

            StartCoroutine(TextFadeOutAndMoveDown());
        }

        public void SetTextColorPosition(string value, Color color, Vector3 position, bool behindOthers = true)
        {
            if (behindOthers)
            {
                transform.SetAsFirstSibling();
            }

            text.text = value;
            text.color = color;
            text.transform.position = position;
        }

        private IEnumerator TextFadeOutAndMoveDown()
        {
            // loop over 1 second backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime)
            {
                // set color with i as alpha 
                text.color = new Color(text.color.r, text.color.g, text.color.b, i);
                var position = text.transform.position;
                transform.position = new Vector3(position.x, position.y - i, position.z);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}