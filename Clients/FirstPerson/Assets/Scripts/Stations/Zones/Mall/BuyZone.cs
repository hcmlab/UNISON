using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Backend;
using PUN;
using UI.Dialogs;
using UnityEngine;
using UnityEngine.Events;

namespace Stations.Zones.Mall
{
    public class BuyZone : InteractableWithDialog<ToggleDialog>
    {
        protected override ZoneType ZoneType => ZoneType.Buy;
        protected override string ActionSpriteName => "Buy";

        private static List<Product> _availableProducts;
        private static List<string> _availableProductTexts;

        private const Product DefaultProduct = Product.HealthPoints;

        private void Awake()
        {
            _availableProducts = Enum.GetValues(typeof(Product)).Cast<Product>().ToList();
            _availableProductTexts = _availableProducts.Select(x => x.GetDisplayName()).ToList();
        }

        protected override void StartAction()
        {
            CurrentDialog = WorldManager.canvasHandler.InitializeToggleDialog(
                LocalizationUtility.GetLocalizedString("buyTitle"),
                LocalizationUtility.GetLocalizedString("buyMessage"),
                LocalizationUtility.GetLocalizedString("buy"),
                LocalizationUtility.GetLocalizedString("cancel"),
                BuyProduct,
                OnCloseDialog,
                GetLabelsAndOnValueChangedListeners(),
                DefaultProduct.GetDisplayName()
            );

            UpdateProductPrice(DefaultProduct);
        }

        private List<(string, UnityAction<bool>)> GetLabelsAndOnValueChangedListeners()
        {
            List<(string, UnityAction<bool>)>
                labelsAndOnValueChangedListeners = new List<(string, UnityAction<bool>)>();
            for (int i = 0; i < _availableProductTexts.Count; ++i)
            {
                Product product = _availableProducts[i];
                string productText = _availableProductTexts[i];

                labelsAndOnValueChangedListeners.Add((productText, isOn =>
                {
                    if (isOn)
                    {
                        UpdateProductPrice(product);
                    }
                }));
            }

            return labelsAndOnValueChangedListeners;
        }

        private void UpdateProductPrice(Product product)
        {
            switch (product)
            {
                case Product.Disinfectant:
                    StartCoroutine(UpdatePriceText(Scale.CostDisinfectant));
                    break;

                case Product.HealthPoints:
                    StartCoroutine(UpdatePriceText(Scale.CostHp));
                    break;

                case Product.HealthCheck:
                    StartCoroutine(UpdatePriceText(Scale.CostHealthCheck));
                    break;

                default:
                    throw new UnityException($"Unknown product {product}!");
            }
        }

        private IEnumerator UpdatePriceText(Scale scale)
        {
            yield return StartCoroutine(BackendConnection.GetScale(response =>
            {
                if (!HandlePossibleErrorResponse(response, OnActionFinished)) return;

                if (response.TryGetResponseAsDouble(out double moneyUnits))
                {
                    string moneyUnitsAsString = moneyUnits.ToString("N2", CultureInfo.InvariantCulture);

                    if (CurrentDialog)
                    {
                        switch (scale)
                        {
                            case Scale.CostDisinfectant:
                                CurrentDialog.UpdateDialogMessage(
                                    LocalizationUtility.GetLocalizedString("buyDisinfectantMessage",
                                        new[] { moneyUnitsAsString }));
                                break;

                            case Scale.CostHp:
                                CurrentDialog.UpdateDialogMessage(
                                    LocalizationUtility.GetLocalizedString("buyHealthPointsMessage",
                                        new[] { moneyUnitsAsString }));
                                break;

                            case Scale.CostHealthCheck:
                                CurrentDialog.UpdateDialogMessage(
                                    LocalizationUtility.GetLocalizedString("buyHealthCheckMessage",
                                        new[] { moneyUnitsAsString }));
                                break;

                            default:
                                throw new UnityException($"Unknown scale {scale}!");
                        }
                    }
                }
            }, scale));
        }

        private void BuyProduct()
        {
            CurrentDialog.ToggleButtons(false, CurrentDialog.yesButton);
            Product product = _availableProducts[CurrentDialog.GetIndexOfSelection()];
            StartCooldown();

            switch (product)
            {
                case Product.Disinfectant:
                    StartCoroutine(BackendConnection.BuyDisinfectant(response =>
                    {
                        CloseInfoMessage();

                        if (HandlePossibleErrorResponse(response, OnActionFinished))
                        {
                            ReactToGenericBoolResponse(response,
                                () =>
                                {
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnBought,
                                        GameEventResult.Success, product);
                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString("buyDisinfectantSuccessMessage"),
                                        OnActionFinished);
                                },
                                () =>
                                {
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnBought,
                                        GameEventResult.NotAllowed, product);
                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString("buyDisinfectantNotAllowedMessage"),
                                        OnActionFinished);
                                }
                            );
                        }
                        else
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnBought,
                                GameEventResult.Error, product);
                        }
                    }));
                    break;

                case Product.HealthPoints:
                    StartCoroutine(BackendConnection.BuyHealthPoints(response =>
                    {
                        CloseInfoMessage();

                        if (HandlePossibleErrorResponse(response, OnActionFinished))
                        {
                            ReactToGenericBoolResponse(response,
                                () =>
                                {
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnBought,
                                        GameEventResult.Success, product);
                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString("buyHealthPointsSuccessMessage"),
                                        OnActionFinished);
                                },
                                () =>
                                {
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnBought,
                                        GameEventResult.NotAllowed, product);
                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString("buyHealthPointsNotAllowedMessage"),
                                        OnActionFinished);
                                }
                            );
                        }
                        else
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnBought,
                                GameEventResult.Error, product);
                        }
                    }));
                    break;

                case Product.HealthCheck:
                    StartCoroutine(BackendConnection.BuyHealthCheck(response =>
                    {
                        CloseInfoMessage();

                        if (HandlePossibleErrorResponse(response, OnActionFinished))
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnBought,
                                GameEventResult.Success, product);
                            if (response.Data.IsInfected)
                            {
                                ShowSuccessMessage(
                                    LocalizationUtility.GetLocalizedString("buyHealthCheckInfectedMessage",
                                        new[]
                                        {
                                            response.Data.HealthPoints.ToString("N2", CultureInfo.InvariantCulture),
                                            response.Data.StressLevel.ToString("N2", CultureInfo.InvariantCulture)
                                        }), OnActionFinished);
                            }
                            else
                            {
                                ShowSuccessMessage(
                                    LocalizationUtility.GetLocalizedString("buyHealthCheckNotInfectedMessage",
                                        new[]
                                        {
                                            response.Data.HealthPoints.ToString("N2", CultureInfo.InvariantCulture),
                                            response.Data.StressLevel.ToString("N2", CultureInfo.InvariantCulture)
                                        }), OnActionFinished);
                            }
                        }
                        else
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnBought,
                                GameEventResult.Error, product);
                        }
                    }));
                    break;

                default:
                    throw new UnityException($"Unknown product {product}!");
            }
        }
    }
}