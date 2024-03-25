using System;
using PUN;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI.HUD
{
    /// <summary>
    ///     Shows the feed of actains that are done by the players only in MasterHUD
    /// </summary>
    public class GameFeedHUD : ScrollingHUD, ICollectionSubscriber<GameEvent>
    {
        public GameObject FeedObjectPrefab;
        public VerticalLayoutGroup FeedObjectContainer;

        [SerializeField] private Color successColor;
        [SerializeField] private Color notAllowedColor;
        [SerializeField] private Color errorColor;

        [SerializeField] private ScrollRect scrollRect;

        public GameObject ScrollDownButtonContainer;

        private bool shouldScrollToBottom = true;

        private void Update()
        {
            HandleScrollInterrupt();
            ToggleScrollDownButtonContainer();
            if (shouldScrollToBottom)
            {
                ScrollToBottom();
            }
        }

        private void OnDestroy()
        {
            CurrentWorldManager.UnsubscribeFromEvents(this);
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        public void OnAdded(GameEvent gameEvent)
        {
            var text = Instantiate(FeedObjectPrefab, FeedObjectContainer.transform).GetComponent<TMP_Text>();
            text.text = gameEvent.GetFeedMessage();
            switch (gameEvent.GameEventResult)
            {
                case GameEventResult.Success:
                    text.color = successColor;
                    break;
                case GameEventResult.NotAllowed:
                    text.color = notAllowedColor;
                    break;
                case GameEventResult.Error:
                    text.color = errorColor;
                    break;
            }
        }

        public void OnRemoved(GameEvent gameEvent)
        {
        }

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            CurrentWorldManager.SubscribeToEvents(this);
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        private void HandleScrollInterrupt()
        {
            if (Input.mouseScrollDelta.y > 0 && PointerOnView)
            {
                shouldScrollToBottom = false;
            }
        }

        private void ToggleScrollDownButtonContainer()
        {
            ScrollDownButtonContainer.SetActive(!shouldScrollToBottom);
        }

        private void ScrollToBottom()
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        public void OnScrollDownButtonPressed()
        {
            shouldScrollToBottom = true;
        }

        public void OnHandleClicked()
        {
            shouldScrollToBottom = false;
        }

        private void OnScrollValueChanged(Vector2 newVal)
        {
            var newScrollPos = Math.Round(newVal.y, 2);
            if (!shouldScrollToBottom && newScrollPos == 0f)
            {
                shouldScrollToBottom = true;
            }
        }
    }
}