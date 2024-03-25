using System;
using Backend;
using Clients;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Settings;
using Stations;
using UI.Dialogs;
using UnityEngine;
using World;

namespace PUN
{
    public class PunPlayer : PunClient
    {
        public GameObject avatarObject;

        [SerializeField] private SkinnedMeshRenderer maleSkinnedMeshRenderer;
        [SerializeField] private SkinnedMeshRenderer femaleSkinnedMeshRenderer;
        public Animator animator;

        public PlayerSoundManager soundManager;

        // parameters
        [SerializeField] private Quaternion hospitalizedRotation = Quaternion.Euler(-90, -180, -90);
        [SerializeField] private float stepSoundInterval = 0.25f;
        private int forwardAnimation;
        private int hospitalizedAnim;
        [NonSerialized] public bool NeedsHelp;

        // state
        private float nextStepQueue;
        public CharacterController CharacterController { get; private set; }

        protected override void AwakeClient()
        {
            CharacterController = controllerObject.GetComponent<CharacterController>();
        }

        protected override void StartClient()
        {
            CurrentWorldManager.RegisterPlayer(this);

            forwardAnimation = Animator.StringToHash("ForwardMovement");
            hospitalizedAnim = Animator.StringToHash("Hospitalized");
        }

        protected override void UpdateClient()
        {
            HandleAnimation();
            HandleSound();
        }

        protected override void OnDestroyClient()
        {
            if (CurrentWorldManager)
            {
                CurrentWorldManager.UnregisterPlayer(this);
            }
        }

        private void HandleAnimation()
        {
            if (!IsMine)
            {
                return;
            }

            animator.SetBool(forwardAnimation, IsMoving);
            animator.SetBool(hospitalizedAnim, IsHospitalized);
        }

        private void HandleSound()
        {
            if (IsMoving)
            {
                var time = Time.time;
                if (nextStepQueue < time)
                {
                    soundManager.Play("Step");
                    nextStepQueue = time + stepSoundInterval;
                }
            }
        }

        public void AdjustRotation()
        {
            Transform.localRotation = IsHospitalized ? hospitalizedRotation : Quaternion.identity;
        }

        protected override void UpdateCustomPropertiesClient(Hashtable properties, bool initial = false)
        {
            if (properties.TryGetValue("customization", out var obj))
            {
                var customization = AvatarCustomization.Deserialize((object[])obj);
                SetupCustomization(customization);
            }
        }

        private void SetupCustomization(AvatarCustomization customization)
        {
            if (maleSkinnedMeshRenderer == null && femaleSkinnedMeshRenderer == null)
            {
                return;
            }

            if (customization == null)
            {
                return;
            }

            // Male: 0: trouser, 1: shirt, 2: shoe, 3: skin, 6: mask, 7: hair
            // Female: 0: trouser, 1: shirt, 2: shoe, 3: skin, 4: hair, 7: mask

            Material[] materials;
            if (customization.Body is AvatarBody.Female)
            {
                femaleSkinnedMeshRenderer.enabled = true;
                maleSkinnedMeshRenderer.enabled = false;
                materials = femaleSkinnedMeshRenderer.materials;

                materials[4].color = customization.HairColor.GetColor();
                materials[7].color = customization.MaskColor.GetColor();
            }
            else
            {
                femaleSkinnedMeshRenderer.enabled = false;
                maleSkinnedMeshRenderer.enabled = true;
                materials = maleSkinnedMeshRenderer.materials;

                materials[7].color = customization.HairColor.GetColor();
                materials[6].color = customization.MaskColor.GetColor();
            }

            materials[0].color = customization.TrouserColor.GetColor();
            materials[1].color = customization.ShirtColor.GetColor();
            materials[2].color = customization.ShoeColor.GetColor();
            materials[3].color = customization.SkinColor.GetColor();

            if (customization.Body is AvatarBody.Female)
            {
                femaleSkinnedMeshRenderer.materials = materials;
            }
            else
            {
                maleSkinnedMeshRenderer.materials = materials;
            }
        }

        [PunRPC]
        [UsedImplicitly]
        public void OnCalledForHelp()
        {
            NeedsHelp = true;
        }

        [PunRPC]
        [UsedImplicitly]
        public void OnNewPetitionEffect(int petitionID)
        {
            Debug.Log("OnNewPetitionEffect: " + petitionID);
            StartCoroutine(BackendConnection.GetPetition(response =>
            {
                var petitionText = DialogUtility.CreatePetitionText(response.Data);
                CurrentWorldManager.canvasHandler.InitializeOkDialog(
                    LocalizationUtility.GetLocalizedString("newPetitionEffectTitle"),
                    LocalizationUtility.GetLocalizedString("newPetitionEffectMessage") + "\n\n" + petitionText,
                    LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Positive, null
                );
            }, petitionID, true));
        }

        [PunRPC]
        [UsedImplicitly]
        public void OnNewRegisterPetitionVote(int petitionID)
        {
            Debug.Log("OnNewRegisterPetitionVote: " + petitionID);
            if (!IsApplicableToCurrentPlayer())
            {
                return;
            }

            StartCoroutine(BackendConnection.GetPetition(response =>
            {
                if (response.IsSuccess())
                {
                    CurrentWorldManager.canvasHandler.InitializeRegisterPetitionDialog(
                        DialogUtility.CreatePetitionText(response.Data),
                        registerPetition =>
                        {
                            if (BackendConnection.PlayerID.HasValue)
                            {
                                CurrentWorldManager.PlayerManager.SendRegisterPetitionVote(registerPetition,
                                    BackendConnection.PlayerID.Value);
                            }
                            else
                            {
                                throw new UnityException("Player ID missing!");
                            }
                        },
                        Time.time + CurrentWorldManager.RegisterPetitionDialogDuration);
                }
                else
                {
                    Debug.LogError($"Could not get petition #{petitionID}: \"{response.GetRawResponse()}\"");
                }
            }, petitionID));
        }

        [PunRPC]
        [UsedImplicitly]
        public void OnRegisterPetitionVote(bool registerPetition, int playerID)
        {
            Debug.Log("OnRegisterPetitionVote: " + (registerPetition ? "Yes" : "No") + ", " + playerID);
            if (!IsApplicableToCurrentPlayer())
            {
                return;
            }

            if (CurrentWorldManager.PlayerManager.RegisterPetitionVoteByPlayerIDs != null)
            {
                CurrentWorldManager.PlayerManager.RegisterPetitionVoteByPlayerIDs[playerID] = registerPetition;
            }
        }

        [PunRPC]
        [UsedImplicitly]
        public void OnRegisterPetitionVoteResult(int positiveVotes, int negativeVotes)
        {
            Debug.Log("OnRegisterPetitionVoteResult: " + positiveVotes + ", " + negativeVotes);
            if (!IsApplicableToCurrentPlayer())
            {
                return;
            }

            if (CurrentWorldManager.PlayerManager.PetitionZone != null)
            {
                var message = positiveVotes > negativeVotes
                    ? LocalizationUtility.GetLocalizedString("yourPositivePetitionVoteResultMessage")
                    : LocalizationUtility.GetLocalizedString("yourNegativePetitionVoteResultMessage");

                CurrentWorldManager.canvasHandler.InitializeOkDialog(
                    LocalizationUtility.GetLocalizedString("registerPetitionVoteResultTitle"), message,
                    LocalizationUtility.GetLocalizedString("ok"),
                    positiveVotes > negativeVotes ? FeedbackType.Positive : FeedbackType.Negative, () =>
                    {
                        if (positiveVotes > negativeVotes)
                        {
                            CurrentWorldManager.PlayerManager.PetitionZone.OpenPetition();
                        }
                        else
                        {
                            CurrentWorldManager.PlayerManager.PetitionZone.CancelOpeningPetition();
                        }

                        CurrentWorldManager.PlayerManager.PetitionZone = null;
                    }
                );
            }
            else
            {
                var message = positiveVotes > negativeVotes
                    ? LocalizationUtility.GetLocalizedString("positivePetitionVoteResultMessage")
                    : LocalizationUtility.GetLocalizedString("negativePetitionVoteResultMessage");

                CurrentWorldManager.canvasHandler.InitializeOkDialog(
                    LocalizationUtility.GetLocalizedString("registerPetitionVoteResultTitle"), message,
                    LocalizationUtility.GetLocalizedString("ok"),
                    positiveVotes > negativeVotes ? FeedbackType.Positive : FeedbackType.Negative, null
                );
            }
        }

        private bool IsApplicableToCurrentPlayer()
        {
            if (WorldManager.IsMasterClient)
            {
                Debug.Log("Skipping because master client.");
                return false;
            }

            Debug.Log("CurrentStationType: " + CurrentWorldManager.ClientManager.PunClient.CurrentStation.Type);
            if (CurrentWorldManager.ClientManager.PunClient.CurrentStation.Type != StationType.TownHall)
            {
                Debug.Log("Skipping because not in town hall.");
                return false;
            }

            return true;
        }
    }
}