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
        protected override ZoneType ZoneType => ZoneType.SendMoney;
        protected override string ActionSpriteName => "SendMoney";

        private readonly List<string> playerNames = new List<string>();
        private readonly Dictionary<string, int> playerIDByPlayerNames = new Dictionary<string, int>();

        protected override void StartAction()
        {
            StartCoroutine(BackendConnection.GetPlayers(LoadPlayersAndShowDialog));
        }

        private void LoadPlayersAndShowDialog(ParameterizedResponse<List<Player>> response)
        {
            if (!HandlePossibleErrorResponse(response, OnActionFinished)) return;

            playerNames.Clear();
            playerIDByPlayerNames.Clear();

            foreach (Player simplePlayer in response.Data)
            {
                if (simplePlayer.ID == BackendConnection.PlayerID) continue;

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
            if (!TryGetSelected(playerNames, out string receiverName) ||
                !TryGetMoneyUnits(out double moneyUnits)) return;

            if (playerIDByPlayerNames.TryGetValue(receiverName, out int receiverID))
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