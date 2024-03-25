using System.Collections.Generic;
using System.Globalization;
using Backend;
using Backend.ResponseDataCapsules;
using PUN;
using UI.Dialogs;
using UnityEngine;

namespace Stations.Zones.Mall
{
    public class SendMoneyZone : SelectAndSpendZone<SelectAndSpendDialog>
    {
        private readonly Dictionary<string, int> playerIDByPlayerNames = new Dictionary<string, int>();

        private readonly List<string> playerNames = new List<string>();
        protected override ZoneType ZoneType => ZoneType.SendMoney;
        protected override string ActionSpriteName => "SendMoney";

        protected override void StartAction()
        {
            StartCoroutine(BackendConnection.GetPlayers(LoadPlayersAndShowDialog));
        }

        private void LoadPlayersAndShowDialog(ParameterizedResponse<List<Player>> response)
        {
            if (!HandlePossibleErrorResponse(response, OnActionFinished))
            {
                return;
            }

            playerNames.Clear();
            playerIDByPlayerNames.Clear();

            foreach (var simplePlayer in response.Data)
            {
                if (simplePlayer.ID == BackendConnection.PlayerID)
                {
                    continue;
                }

                playerNames.Add(simplePlayer.Name);
                playerIDByPlayerNames[simplePlayer.Name] = simplePlayer.ID;
            }

            CurrentDialog = WorldManager.canvasHandler.InitializeSelectAndSpendDialog(
                LocalizationUtility.GetLocalizedString("sendMoneyTitle"),
                LocalizationUtility.GetLocalizedString("sendMoneyMessage"),
                LocalizationUtility.GetLocalizedString("send"),
                LocalizationUtility.GetLocalizedString("cancel"),
                SendMoney,
                OnCloseDialog,
                playerNames
            );
        }

        private void SendMoney()
        {
            if (!TryGetSelected(playerNames, out var receiverName) ||
                !TryGetMoneyUnits(out var moneyUnits))
            {
                return;
            }

            if (playerIDByPlayerNames.TryGetValue(receiverName, out var receiverID))
            {
                CurrentDialog.ToggleButtons(false, CurrentDialog.yesButton);

                StartCooldown();

                StartCoroutine(BackendConnection.SendMoneyToUser(response =>
                {
                    CloseInfoMessage();

                    if (HandlePossibleErrorResponse(response, OnActionFinished))
                    {
                        ReactToGenericBoolResponse(response, () =>
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnMoneySent,
                                GameEventResult.Success, receiverName, moneyUnits);

                            ShowSuccessMessage(
                                LocalizationUtility.GetLocalizedString("sendMoneySuccessMessage",
                                    new[] { moneyUnits.ToString("N2", CultureInfo.InvariantCulture), receiverName }),
                                OnActionFinished);
                        }, () =>
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnMoneySent,
                                GameEventResult.NotAllowed, receiverName, moneyUnits);

                            ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("sendMoneyNotAllowedMessage"),
                                OnActionFinished);
                        });
                    }
                    else
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnMoneySent,
                            GameEventResult.Error, receiverName, moneyUnits);
                    }
                }, receiverID, moneyUnits));
            }
            else
            {
                throw new UnityException($"Unknown receiver \"{receiverName}\"!");
            }
        }
    }
}