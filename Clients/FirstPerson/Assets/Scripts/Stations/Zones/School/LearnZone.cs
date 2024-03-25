using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Backend;
using PUN;
using UI.Dialogs;

namespace Stations.Zones.School
{
    public class LearnZone : InteractableWithDialog<YesNoDialog>
    {
        protected override ZoneType ZoneType => ZoneType.Learn;
        protected override string ActionSpriteName => "Learn";

        private static readonly List<Scale> Scales = new List<Scale>
        {
            Scale.CostSchool, Scale.SchoolCostFree
        };

        protected override void StartAction()
        {
            StartCoroutine(TryToOpenLearnDialog());
        }

        private IEnumerator TryToOpenLearnDialog()
        {
            bool isAllowedToLearn = false;

            yield return BackendConnection.GetCurrentPlayer(playerResponse =>
            {
                if (HandlePossibleErrorResponse(playerResponse, OnActionFinished))
                {
                    if (playerResponse.Data.HasLearned)
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnLearned,
                            GameEventResult.NotAllowed);
                        ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("alreadyLearnedWarning"),
                            OnActionFinished);
                    }
                    else if (playerResponse.Data.HasWorked)
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnLearned,
                            GameEventResult.NotAllowed);
                        ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("alreadyWorkedWarning"),
                            OnActionFinished);
                    }
                    else
                    {
                        isAllowedToLearn = true;
                    }
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnLearned, GameEventResult.Error);
                }
            }, true);

            if (isAllowedToLearn)
            {
                yield return BackendConnection.GetScales(scalesResponse =>
                {
                    if (HandlePossibleErrorResponse(scalesResponse, OnActionFinished))
                    {
                        OpenLearnDialog(scalesResponse.Data[Scale.CostSchool],
                            scalesResponse.Data[Scale.SchoolCostFree] != 0);
                    }
                    else
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnLearned,
                            GameEventResult.Error);
                    }
                }, Scales, true);
            }
        }

        private void OpenLearnDialog(int schoolCostMoneyUnits, bool isSchoolCostFree)
        {
            CurrentDialog = WorldManager.canvasHandler.InitializeYesNoDialog(
                LocalizationUtility.GetLocalizedString("learnTitle"),
                GenerateLearnDialogMessage(schoolCostMoneyUnits, isSchoolCostFree),
                LocalizationUtility.GetLocalizedString("yes"),
                LocalizationUtility.GetLocalizedString("no"),
                Learn,
                OnCloseDialog
            );
        }

        private static string GenerateLearnDialogMessage(double schoolCostMoneyUnits, bool isSchoolCostFree)
        {
            return LocalizationUtility.GetLocalizedString(isSchoolCostFree ? "learnSchoolFreeMessage" : "learnMessage",
                new[] { schoolCostMoneyUnits.ToString("N2", CultureInfo.InvariantCulture) });
        }

        private void Learn()
        {
            CurrentDialog.ToggleButtons(false, CurrentDialog.yesButton);

            StartCoroutine(BackendConnection.Learn(response =>
            {
                CloseInfoMessage();

                if (HandlePossibleErrorResponse(response, OnActionFinished))
                {
                    ReactToGenericBoolResponse(response, () =>
                        {
                            StartCooldown();

                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnLearned,
                                GameEventResult.Success);
                            ShowSuccessMessage(LocalizationUtility.GetLocalizedString("learnSuccessMessage"),
                                OnActionFinished);
                        },
                        () =>
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnLearned,
                                GameEventResult.NotAllowed);
                            ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("learnNotAllowedMessage"),
                                OnActionFinished);
                        }
                    );
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnLearned, GameEventResult.Error);
                }
            }, true));
        }
    }
}