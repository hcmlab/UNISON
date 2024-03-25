using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UI.Dialogs
{
    public class GraphsDialog : OkDialog
    {
        [SerializeField] private TMP_Dropdown graphsDropdown;
        [SerializeField] private RawImage graphImage;

        private Dictionary<string, string> availableGraphs;
        private List<string> optionsAsList;

        public void Setup(string newTitle, string newMessage, string yesButtonText, UnityAction confirmAction,
            Dictionary<string, string> options)
        {
            base.Setup(newTitle, newMessage, yesButtonText, confirmAction);

            availableGraphs = options ?? new Dictionary<string, string>();
            optionsAsList = availableGraphs.Keys.ToList();

            graphsDropdown.ClearOptions();
            graphsDropdown.AddOptions(optionsAsList);

            graphsDropdown.onValueChanged.AddListener(UpdateGraph);
            UpdateGraph(0);
        }

        private void UpdateGraph(int index)
        {
            ToggleSelect(false);

            var selectedGraph = optionsAsList[index];
            var url = availableGraphs[selectedGraph];
            Debug.Log($"Updating to {selectedGraph}: {url}");
            StartCoroutine(DownloadGraphAndUpdateImage(url));
        }

        private IEnumerator DownloadGraphAndUpdateImage(string url)
        {
            var unityWebRequest = UnityWebRequestTexture.GetTexture(url);
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Received error response: \"{unityWebRequest.error}\"");
            }
            else
            {
                graphImage.texture = ((DownloadHandlerTexture)unityWebRequest.downloadHandler).texture;
            }

            ToggleSelect(true);
        }

        private void ToggleSelect(bool value)
        {
            graphsDropdown.interactable = value;
        }
    }
}