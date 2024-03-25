using Backend;
using PUN;
using Settings;
using UI.Dialogs;
using UI.HUD;
using UnityEngine;
using UnityEngine.Events;
using World;

namespace Stations.Zones
{
    public abstract class Interactable : MonoBehaviour
    {
        protected WorldManager WorldManager;
        protected Station Station;

        private ActionDialogHUD actionDialog;
        private Sprite actionSprite;

        protected virtual ZoneType ZoneType => default;
        protected virtual string ActionSpriteName => null;

        [SerializeField] private Color isInactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color isActiveColor = new Color(0.37f, 0.62f, 1f, 0.5f);
        [SerializeField] private Color isInteractableColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

        private Material material;

        private const int IsInactiveColorOffset = 0;
        private const int IsActiveColorOffset = 100;
        private const int IsInteractableColorOffset = 200;

        private int targetColorOffset = 100;
        private int currentColorOffset = 100;

        private const int ColorOffsetSpeed = 5;

        private bool canInteract;
        private bool actionStarted;

        private void Start()
        {
            WorldManager = GetComponentInParent<WorldManager>();
            Station = GetComponentInParent<Station>();

            actionSprite = Resources.Load<Sprite>($"Sprites/ActionSprites/{ActionSpriteName}");

            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && Station.Type != StationType.Hospital)
            {
                spriteRenderer.sprite = actionSprite;
            }

            material = GetComponentInChildren<Renderer>().material;

            // Set initial color.
            material.color = isActiveColor;
        }

        private void OnDisable()
        {
            actionDialog = null;
            actionStarted = false;
            canInteract = false;
        }

        private void Update()
        {
            if (!actionDialog && !WorldManager.IsMasterClient)
            {
                actionDialog = WorldManager.canvasHandler.HUDManager.GetComponentInChildren<ActionDialogHUD>();
                return;
            }

            // TODO: Optimize with subscriber
            (bool isActive, string reasonForInactive) = WorldManager.IsInteractableActive(Station.Type, ZoneType);
            if (canInteract)
            {
                targetColorOffset = isActive ? IsInteractableColorOffset : IsInactiveColorOffset;

                (float remainingCooldown, float cooldownInSeconds) =
                    InteractableConfiguration.CalculateRemainingCooldown(ZoneType);
                actionDialog.SetRemainingCooldown(remainingCooldown, cooldownInSeconds, isActive, reasonForInactive);

                if (isActive && remainingCooldown == 0 && Input.GetKeyDown(ControlSettings.ControlData.Interact))
                {
                    StartActionIfNotStarted();
                }
            }
            else
            {
                targetColorOffset = isActive ? IsActiveColorOffset : IsInactiveColorOffset;
            }
        }

        private void FixedUpdate()
        {
            if (currentColorOffset > targetColorOffset)
            {
                currentColorOffset -= ColorOffsetSpeed;
            }
            else if (currentColorOffset < targetColorOffset)
            {
                currentColorOffset += ColorOffsetSpeed;
            }
            else
            {
                return;
            }

            if (currentColorOffset >= IsActiveColorOffset)
            {
                // White to default: currentColorOffset is somewhere between 100 and 200.
                material.color = Color.Lerp(isActiveColor, isInteractableColor, (currentColorOffset - 100) / 100f);
            }
            else if (currentColorOffset < IsActiveColorOffset)
            {
                // Black to default: currentColorOffset is somewhere between 0 and 99.
                material.color = Color.Lerp(isInactiveColor, isActiveColor, currentColorOffset / 100f);
            }
        }

        protected void StartCooldown()
        {
            float currentTime = Time.time;
            InteractableConfiguration.LastInteractionTimestampByZones[ZoneType] = currentTime;
            InteractableConfiguration.GlobalLastInteractionTimestamp = currentTime;
        }

        private void StartActionIfNotStarted()
        {
            if (!actionStarted)
            {
                GameObject player = GameObject.Find("PlayerManager(Clone)");
                if (player != null)
                {
                    player.GetComponent<Clients.PlayerManager>().EnableCursor();
                }
                actionStarted = true;

                StartAction();
            }
        }

        protected abstract void StartAction();

        protected void OnActionFinished()
        {
            GameObject player = GameObject.Find("PlayerManager(Clone)");
            if (player != null)
            {
                player.GetComponent<Clients.PlayerManager>().DisableCursor();
            }
            actionStarted = false;
        }

        protected bool HandlePossibleErrorResponse(Response response, UnityAction onCloseCall = null)
        {
            switch (response.GetResponseType())
            {
                case ResponseType.Success:
                    return true;

                case ResponseType.ServerError:
                    ShowServerErrorMessage(LocalizationUtility.GetLocalizedString("backendError"), onCloseCall);
                    return false;

                case ResponseType.ClientError:
                    ShowNotAllowedMessage(response.GetLocalizedResponse(), onCloseCall);
                    return false;

                default:
                    throw new UnityException($"Unknown response type {response.GetResponseType()}!");
            }
        }

        protected void ReactToGenericBoolResponse(Response response, UnityAction successAction,
            UnityAction notAllowedAction)
        {
            if (response.TryGetResponseAsBool(out bool success))
            {
                if (success)
                {
                    successAction();
                }
                else
                {
                    notAllowedAction();
                }
            }
            else
            {
                throw new UnityException($"Response \"{response.GetRawResponse()}\" not a bool!");
            }
        }

        protected void ShowSuccessMessage(string message, UnityAction onCloseCall = null)
        {
            WorldManager.canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("successTitle"),
                message, LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Positive, onCloseCall);
        }

        protected void ShowNotAllowedMessage(string message, UnityAction onCloseCall = null)
        {
            WorldManager.canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("notAllowedTitle"),
                message, LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative, onCloseCall);
        }

        protected void ShowClientErrorMessage(string message, UnityAction onCloseCall = null)
        {
            WorldManager.canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("warningTitle"),
                message, LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative, onCloseCall);
        }

        protected void ShowServerErrorMessage(string message, UnityAction onCloseCall = null)
        {
            WorldManager.canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("criticalErrorTitle"),
                message + " " + LocalizationUtility.GetLocalizedString("contactAdmin"),
                LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative, onCloseCall);
        }

        private void OnTriggerEnter(Collider other)
        {
            PunClient client = other.gameObject.GetComponentInParent<PunClient>();
            if (client && client.IsMine)
            {
                actionStarted = false;
                canInteract = true;
                actionDialog.Show(actionSprite, ControlSettings.ControlData.Interact);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PunClient client = other.gameObject.GetComponentInParent<PunClient>();
            if (client && client.IsMine)
            {
                actionStarted = false;
                canInteract = false;
                actionDialog.Hide();
            }
        }

        public bool IsStarted()
        {
            return actionStarted;
        }
    }
}