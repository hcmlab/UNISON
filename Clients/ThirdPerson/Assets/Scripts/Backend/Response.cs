using System.Globalization;
using UnityEngine;

namespace Backend
{
    // 'Client Error: You should be in the hospital right now!'
    // 'Client Error: The game is in lockdown and therefore you cannot visit the lounge.'
    // 'Client Error: You can only buy a care package, if you have 2 days left in the hospital.'
    // 'Client Error: You have already bought a care package.'
    // 'Client Error: You have insufficient money units!'
    // 'Client Error: The minimum wage cannot exceed the maximum wage!'
    // 'Client Error: You have already voted on this petition!'
    // 'Client Error: Value missing'
    // 'Client Error: You have already worked today!'
    // 'Client Error: You have already learned today!'
    public class Response
    {
        private readonly string message;
        private readonly ResponseType responseType;

        public Response(string action, string requestUrl, string message, bool success = true)
        {
            if (message == null)
            {
                throw new UnityException("Message cannot be null!");
            }

            this.message = message.Trim();
            var simplifiedMessage = this.message.ToLower();

            if (success)
            {
                if (simplifiedMessage.StartsWith("client error"))
                {
                    Debug.LogWarning($"Client error \"{message}\" for action {action} and request URL {requestUrl}!");
                    responseType = ResponseType.ClientError;
                }
                else if (simplifiedMessage.StartsWith("<br />") || simplifiedMessage.StartsWith("error"))
                {
                    Debug.LogError($"Server error \"{message}\" for action {action} and request URL {requestUrl}!");
                    responseType = ResponseType.ServerError;
                }
                else
                {
                    responseType = ResponseType.Success;
                }
            }
            else
            {
                Debug.LogError(
                    $"Unrecognized server error \"{message}\" for action {action} and request URL {requestUrl}!");
                responseType = ResponseType.ServerError;
            }
        }

        public bool TryGetResponseAsBool(out bool value)
        {
            var simplifiedMessage = message.ToLower();
            switch (simplifiedMessage)
            {
                case "1":
                case "true":
                case "yes":
                    value = true;
                    return true;

                case "":
                case "0":
                case "false":
                case "no":
                    value = false;
                    return true;

                default:
                    value = false;
                    return false;
            }
        }

        public bool TryGetResponseAsInt(out int value)
        {
            if (int.TryParse(message, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                value = result;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetResponseAsDouble(out double value)
        {
            if (double.TryParse(message, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                value = result;
                return true;
            }

            value = default;
            return false;
        }

        public string GetRawResponse()
        {
            return message;
        }

        public string GetLocalizedResponse()
        {
            switch (message)
            {
                case "Client Error: Action missing":
                case "Client Error: Value missing":
                case "Client Error: Instance name missing":
                case "Client Error: Player ID missing":
                case "Client Error: Token missing":
                case "Client Error: Invalid token":
                    Debug.LogError($"Unrecoverable client error \"{message}\"!");
                    return LocalizationUtility.GetLocalizedString("criticalClientError", "contactAdmin");

                case "Client Error: You should be in the hospital right now":
                    Debug.LogError($"Critical client error \"{message}\"!");
                    return LocalizationUtility.GetLocalizedString("notInHospitalError", "contactAdmin");

                case "Client Error: The minimum wage cannot exceed the maximum wage":
                    return LocalizationUtility.GetLocalizedString("minimumMaximumWageError");

                case "Client Error: You have already voted on this petition":
                    return LocalizationUtility.GetLocalizedString("alreadyVotedError");

                case "Client Error: You have already learned today":
                    return LocalizationUtility.GetLocalizedString("alreadyLearnedError");

                case "Client Error: You have already worked today":
                    return LocalizationUtility.GetLocalizedString("alreadyWorkedError");

                case "Client Error: You have insufficient money units":
                    return LocalizationUtility.GetLocalizedString("insufficientMoneyError");

                case "Client Error: You have already bought a care package":
                    return LocalizationUtility.GetLocalizedString("alreadyBoughtCarePackageError");

                case "Client Error: You can only buy a care package, if you have 2 days left in the hospital":
                    return LocalizationUtility.GetLocalizedString("carePackageOnlyOn2DaysError");

                case "Client Error: The game is in lockdown and therefore you cannot visit the lounge":
                    return LocalizationUtility.GetLocalizedString("gameInLockdownError");

                default:
                    Debug.LogError($"Unknown error \"{message}\"!");
                    return message;
            }
        }

        public bool IsSuccess()
        {
            return responseType == ResponseType.Success;
        }

        public ResponseType GetResponseType()
        {
            return responseType;
        }
    }
}