using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Backend.ResponseDataCapsules;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Stations;
using UI.Menu;
using UnityEngine;
using UnityEngine.Networking;

namespace Backend
{
    public enum HttpRequestMethod
    {
        Get,
        Post
    }

    public static class BackendConnection
    {
        
        public const string Url = "Placeholder";

        public const string MasterClientName = "MasterClient";

        private static readonly List<string> WebsitesToCheck = new List<string>
        {
            "https://www.google.com", "https://www.bing.com", "https://www.facebook.com", "https://www.microsoft.com"
        };

        private static bool? LoggedInAsPlayer { get; set; }
        public static int? PlayerID { get; private set; }
        private static string Token { get; set; }
        public static string InstanceName { get; set; }

        public static bool IsLoggedInAsPlayer()
        {
            if (LoggedInAsPlayer.HasValue)
            {
                if (!PlayerID.HasValue || Token == null)
                {
                    throw new UnityException("Player ID or token missing!");
                }

                Debug.Log(LoggedInAsPlayer.Value ? "Logged in as player." : "Not logged in as player.");
                return LoggedInAsPlayer.Value;
            }

            if (PlayerID.HasValue || Token != null)
            {
                throw new UnityException("Player ID or token set!");
            }

            Debug.Log("Not logged in (as player).");
            return false;
        }

        public static bool IsLoggedInAsMasterClient()
        {
            if (LoggedInAsPlayer.HasValue)
            {
                if (!PlayerID.HasValue || Token == null)
                {
                    throw new UnityException("Player ID or token missing!");
                }

                Debug.Log(!LoggedInAsPlayer.Value ? "Logged in as master client." : "Not logged in as master client.");
                return !LoggedInAsPlayer.Value;
            }

            if (PlayerID.HasValue || Token != null)
            {
                throw new UnityException("Player ID or token set!");
            }

            Debug.Log("Not logged in (as master client).");
            return false;
        }

        public static void LoginAsPlayer(int playerID, string token)
        {
            Debug.Log("Logging in as player.");
            LoggedInAsPlayer = true;
            PlayerID = playerID;
            Token = token;
        }

        public static void LoginAsMasterClient(int playerID, string token)
        {
            Debug.Log("Logging in as master client.");
            LoggedInAsPlayer = false;
            PlayerID = playerID;
            Token = token;
        }

        public static void Logout()
        {
            Debug.Log("Logging out.");
            LoggedInAsPlayer = null;
            InstanceName = null;
            PlayerID = null;
            Token = null;
        }

        public static IEnumerator CheckIsInternetAvailable(Action<bool> callback)
        {
            var isInternetAvailable = false;
            // Check multiple websites to avoid false negatives.
            foreach (var website in WebsitesToCheck)
            {
                yield return CheckIsWebsiteAvailable(
                    result => { isInternetAvailable = result == UnityWebRequest.Result.Success; }, website);

                if (isInternetAvailable)
                {
                    callback(true);
                    yield break;
                }
            }

            callback(false);
        }

        public static IEnumerator CheckIsBackendAvailable(Action<bool> callback)
        {
            var unityWebRequest = UnityWebRequest.Get(Url);
            yield return unityWebRequest.SendWebRequest();
            callback(unityWebRequest.result == UnityWebRequest.Result.Success);
        }

        private static IEnumerator CheckIsWebsiteAvailable(Action<UnityWebRequest.Result> callback, string websiteUrl)
        {
            var unityWebRequest = UnityWebRequest.Get(websiteUrl);
            yield return unityWebRequest.SendWebRequest();
            callback(unityWebRequest.result);
        }

        private static IEnumerator ExecuteAction(HttpRequestMethod httpRequestMethod, string action, bool logAction,
            Action<Response> callback, params object[] values)
        {
            var (requestUrl, unityWebRequest) = CreateWebRequest(httpRequestMethod, action, values);
            if (logAction)
            {
                Debug.Log($"Executing action {action} ({requestUrl}).");
            }

            yield return unityWebRequest.SendWebRequest();

            Response response;
            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                if (logAction)
                {
                    Debug.LogError($"Received error response \"{unityWebRequest.error}\".");
                }

                response = new Response(action, requestUrl, unityWebRequest.error, false);
            }
            else
            {
                if (logAction)
                {
                    Debug.Log($"Received response \"{unityWebRequest.downloadHandler.text}\".");
                }

                response = new Response(action, requestUrl, unityWebRequest.downloadHandler.text);
            }

            callback(response);
        }

        private static IEnumerator ExecuteActionAndConvert<T>(HttpRequestMethod httpRequestMethod, string action,
            bool logAction, Func<string, T> convertFunction, Action<ParameterizedResponse<T>> callback,
            params object[] values)
        {
            var (requestUrl, unityWebRequest) = CreateWebRequest(httpRequestMethod, action, values);
            if (logAction)
            {
                Debug.Log($"Executing action {action} ({requestUrl}).");
            }

            yield return unityWebRequest.SendWebRequest();

            ParameterizedResponse<T> parameterizedResponse;
            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                if (logAction)
                {
                    Debug.LogError($"Received error response: \"{unityWebRequest.error}\"");
                }

                parameterizedResponse = new ParameterizedResponse<T>(action, requestUrl, unityWebRequest.error);
            }
            else
            {
                if (logAction)
                {
                    Debug.Log($"Received response: \"{unityWebRequest.downloadHandler.text}\"");
                }

                parameterizedResponse = new ParameterizedResponse<T>(action, requestUrl,
                    unityWebRequest.downloadHandler.text, convertFunction);
            }

            callback(parameterizedResponse);
        }

        private static (string, UnityWebRequest) CreateWebRequest(HttpRequestMethod httpRequestMethod, string action,
            object[] values)
        {
            string requestUrl;
            UnityWebRequest unityWebRequest;

            switch (httpRequestMethod)
            {
                case HttpRequestMethod.Get:
                    requestUrl = BuildRequestURL(action, values);
                    unityWebRequest = UnityWebRequest.Get(requestUrl);
                    break;

                case HttpRequestMethod.Post:
                    requestUrl = BuildRequestURL(action);
                    unityWebRequest = UnityWebRequest.Post(requestUrl, BuildFormFields(values));
                    break;

                default:
                    throw new UnityException($"Unknown HTTP request method {httpRequestMethod}!");
            }

            return (requestUrl, unityWebRequest);
        }

        private static Dictionary<string, string> BuildFormFields(object[] values)
        {
            var formFields = new Dictionary<string, string>();

            for (var i = 0; i < values.Length; ++i)
            {
                var possibleValue = values[i];
                if (possibleValue == null)
                {
                    throw new UnityException($"value{i + 1} cannot be null!");
                }

                formFields[$"value{i + 1}"] = QueryValueToString(possibleValue);
            }

            return formFields;
        }

        private static string BuildRequestURL(string action, [ItemCanBeNull] object[] values = null)
        {
            var queryBuilder = new StringBuilder(Url);
            queryBuilder.Append("?ac=").Append(Uri.EscapeDataString(action));

            if (InstanceName != null)
            {
                queryBuilder.Append("&inst=").Append(Uri.EscapeDataString(InstanceName));
            }

            if (PlayerID.HasValue)
            {
                queryBuilder.Append("&playerID=").Append(PlayerID.Value);
            }

            if (Token != null)
            {
                queryBuilder.Append("&token=").Append(Uri.EscapeDataString(Token));
            }

            if (values != null)
            {
                for (var i = 0; i < values.Length; ++i)
                {
                    var possibleValue = values[i];
                    if (possibleValue == null)
                    {
                        throw new UnityException($"value{i + 1} cannot be null!");
                    }

                    queryBuilder
                        .Append("&value")
                        .Append(i + 1)
                        .Append("=")
                        .Append(Uri.EscapeDataString(QueryValueToString(possibleValue)));
                }
            }

            return queryBuilder.ToString();
        }

        private static string QueryValueToString(object value)
        {
            return value switch
            {
                int intValue => intValue.ToString(CultureInfo.InvariantCulture),
                float floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
                double doubleValue => doubleValue.ToString(CultureInfo.InvariantCulture),
                bool boolValue => boolValue.ToString(CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }

        public static IEnumerator VisitStation(Action<Response> callback, StationType stationType)
        {
            yield return stationType switch
            {
                StationType.Home => VisitHome(callback, true),
                StationType.Hospital => VisitHospital(callback, true),
                StationType.Lounge => VisitLounge(callback, true),
                StationType.Mall => VisitMall(callback, true),
                StationType.MarketSquare => VisitMarketSquare(callback, true),
                StationType.Office => VisitOffice(callback, true),
                StationType.School => VisitSchool(callback, true),
                StationType.TownHall => VisitTownHall(callback, true),
                _ => throw new UnityException($"Cannot visit station \"{stationType}\"!")
            };
        }

        public static IEnumerator ExitStation(Action<Response> callback, StationType stationType)
        {
            yield return stationType switch
            {
                StationType.Home => ExitHome(callback, true),
                StationType.Hospital => ExitHospital(callback, true),
                StationType.Lounge => ExitLounge(callback, true),
                StationType.Mall => ExitMall(callback, true),
                StationType.MarketSquare => ExitMarketSquare(callback, true),
                StationType.Office => ExitOffice(callback, true),
                StationType.School => ExitSchool(callback, true),
                StationType.TownHall => ExitTownHall(callback, true),
                _ => throw new UnityException($"Cannot exit station \"{stationType}\"!")
            };
        }

        /// ############
        /// ### Mall ###
        /// ############
        private static IEnumerator VisitMall(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "m01", logAction, callback);
        }

        public static IEnumerator BuyDisinfectant(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "m03", logAction, callback);
        }

        public static IEnumerator BuyHealthPoints(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "m04", logAction, callback);
        }

        public static IEnumerator BuyHealthCheck(Action<ParameterizedResponse<HealthCheck>> callback,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "m05", logAction,
                JsonConvert.DeserializeObject<HealthCheck>, callback);
        }

        public static IEnumerator InvestIntoVaccineFund(Action<Response> callback, double moneyUnits,
            bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "m06", logAction, callback, moneyUnits);
        }

        public static IEnumerator InvestIntoStocks(Action<Response> callback, double moneyUnits, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "m07", logAction, callback, moneyUnits);
        }

        public static IEnumerator SendMoneyToUser(Action<Response> callback, int receiverID, double moneyUnits,
            bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "m08", logAction, callback, receiverID, moneyUnits);
        }

        private static IEnumerator ExitMall(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "m99", logAction, callback);
        }

        /// ##############
        /// ### Office ###
        /// ##############
        private static IEnumerator VisitOffice(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "o01", logAction, callback);
        }

        public static IEnumerator EarnMoney(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "o02", logAction, callback);
        }

        private static IEnumerator ExitOffice(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "o99", logAction, callback);
        }

        /// ##############
        /// ### School ###
        /// ##############
        private static IEnumerator VisitSchool(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "s01", logAction, callback);
        }

        public static IEnumerator Learn(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "s02", logAction, callback);
        }

        private static IEnumerator ExitSchool(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "s99", logAction, callback);
        }

        /// ################
        /// ### Lounge ###
        /// ################
        private static IEnumerator VisitLounge(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "l01", logAction, callback);
        }

        public static IEnumerator Relax(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "l02", logAction, callback);
        }

        private static IEnumerator ExitLounge(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "l99", logAction, callback);
        }

        /// ################
        /// ### TownHall ###
        /// ################
        private static IEnumerator VisitTownHall(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "t01", logAction, callback);
        }

        private static IEnumerator ExitTownHall(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "t99", logAction, callback);
        }

        /// #############
        /// ### Votes ###
        /// #############
        public static IEnumerator ListAllPetitionTemplates(Action<ParameterizedResponse<List<Template>>> callback,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "votetemp", logAction,
                JsonConvert.DeserializeObject<List<Template>>, callback, MainMenu.CurrentLanguage);
        }

        public static IEnumerator Vote(Action<Response> callback, int petitionID, bool positiveVote,
            bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "vote", logAction, callback, petitionID,
                positiveVote ? "yes" : "no");
        }

        public static IEnumerator OpenPetition(Action<Response> callback, int petitionID, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "openvote", logAction, callback, petitionID);
        }

        public static IEnumerator ClosePetition(Action<Response> callback, int petitionID, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "closevote", logAction, callback, petitionID);
        }

        public static IEnumerator RegisterNewPetition(Action<Response> callback, int petitionTemplateID, int value,
            bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "votedev", logAction, callback, petitionTemplateID,
                value);
        }

        public static IEnumerator GetOpenPetitions(Action<ParameterizedResponse<List<Petition>>> callback,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "listopenpetitions", logAction,
                JsonConvert.DeserializeObject<List<Petition>>, callback, MainMenu.CurrentLanguage);
        }

        public static IEnumerator GetPetitions(Action<ParameterizedResponse<List<Petition>>> callback,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "listpetitions", logAction,
                JsonConvert.DeserializeObject<List<Petition>>, callback, MainMenu.CurrentLanguage);
        }

        public static IEnumerator GetPetition(Action<ParameterizedResponse<Petition>> callback, int petitionID,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "listpetition", logAction,
                JsonConvert.DeserializeObject<Petition>, callback, petitionID, MainMenu.CurrentLanguage);
        }

        public static IEnumerator DeletePetition(Action<Response> callback, int petitionID, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "delvote", logAction, callback, petitionID);
        }

        /// ################
        /// ### Hospital ###
        /// ################
        private static IEnumerator VisitHospital(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "h01", logAction, callback);
        }

        public static IEnumerator BuyCarePackage(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "h02", logAction, callback);
        }

        private static IEnumerator ExitHospital(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "h99", logAction, callback);
        }

        /// ####################
        /// ### MarketSquare ###
        /// ####################
        private static IEnumerator VisitMarketSquare(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "sq01", logAction, callback);
        }

        private static IEnumerator ExitMarketSquare(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "sq99", logAction, callback);
        }

        /// ############
        /// ### Home ###
        /// ############
        private static IEnumerator VisitHome(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "q01", logAction, callback);
        }

        private static IEnumerator ExitHome(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "q99", logAction, callback);
        }

        /// #####################
        /// ### Miscellaneous ###
        /// #####################
        public static IEnumerator Login(Action<ParameterizedResponse<AuthenticatedPlayer>> callback,
            [NotNull] string name, [NotNull] string password,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "login", logAction,
                response => response == "false" ? null : JsonConvert.DeserializeObject<AuthenticatedPlayer>(response),
                callback, name, password);
        }

        public static IEnumerator RegisterPlayerForInstance(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "addtoinstance", logAction, callback);
        }

        public static IEnumerator Initialize(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "init", logAction, callback);
        }

        public static IEnumerator GetPlayerProperty(Action<Response> callback, PlayerProperty playerProperty,
            bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "ask", logAction, callback,
                playerProperty.GetNameForBackend());
        }

        public static IEnumerator GetGameProperty(Action<Response> callback, GameProperty gameProperty,
            bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "askg", logAction, callback,
                gameProperty.GetNameForBackend());
        }

        public static IEnumerator GetGlobalFunds(Action<ParameterizedResponse<Funds>> callback, bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "askgf", logAction,
                JsonConvert.DeserializeObject<Funds>, callback);
        }

        public static IEnumerator AdvanceToNextRound(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "nr", logAction, callback);
        }

        public static IEnumerator CreateInstance(Action<Response> callback, string possibleInstanceUuid = null,
            bool logAction = false)
        {
            InstanceName = possibleInstanceUuid ?? Guid.NewGuid().ToString();
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            const bool isTesting = true;
#else
            const bool isTesting = false;
#endif
            yield return ExecuteAction(HttpRequestMethod.Get, "ci", logAction, callback, isTesting ? 1 : 0);
        }

        public static IEnumerator ListAllInstances(Action<ParameterizedResponse<List<string>>> callback,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "li", logAction, response =>
            {
                if (response.ToLower().Contains("no instance"))
                {
                    return new List<string>();
                }

                return JsonConvert.DeserializeObject<List<string>>(response);
            }, callback);
        }

        public static IEnumerator GetPlayers(Action<ParameterizedResponse<List<Player>>> callback,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "getplayer", logAction,
                JsonConvert.DeserializeObject<List<Player>>, callback);
        }

        public static IEnumerator GetCurrentPlayer(Action<ParameterizedResponse<Player>> callback,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "getplayer", logAction, response =>
            {
                var simplePlayers = JsonConvert.DeserializeObject<List<Player>>(response);
                foreach (var simplePlayer in simplePlayers)
                {
                    if (PlayerID == simplePlayer.ID)
                    {
                        return simplePlayer;
                    }
                }

                return null;
            }, callback);
        }

        public static IEnumerator GetGameRound(Action<ParameterizedResponse<GameRound>> callback,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "getgame", logAction,
                JsonConvert.DeserializeObject<GameRound>, callback);
        }

        public static IEnumerator GetHealthStatus(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "askhlt", logAction, callback);
        }

        public static IEnumerator GetDaysRemainingInHospital(Action<Response> callback, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "inhospital", logAction, callback);
        }

        public static IEnumerator GetScale(Action<Response> callback, Scale scale, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "getscale", logAction, callback,
                scale.GetNameForBackend());
        }

        public static IEnumerator GetScales(Action<ParameterizedResponse<Dictionary<Scale, int>>> callback,
            [NotNull] List<Scale> scaleIDs, bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "getscales", logAction, response =>
            {
                var deserializedResults =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

                var results = new Dictionary<Scale, int>();
                foreach (var result in deserializedResults)
                {
                    results[DeserializationUtility.GetScaleFromBackendName(result.Key)] =
                        DeserializationUtility.IntOrBoolAsStringToInt(result.Value);
                }

                return results;
            }, callback, JsonConvert.SerializeObject(scaleIDs.ConvertAll(scaleID => scaleID.GetNameForBackend())));
        }

        public static IEnumerator GetGraphs(Action<ParameterizedResponse<Dictionary<string, string>>> callback,
            bool logAction = false)
        {
            yield return ExecuteActionAndConvert(HttpRequestMethod.Get, "getgraphs", logAction,
                JsonConvert.DeserializeObject<Dictionary<string, string>>, callback, MainMenu.CurrentLanguage);
        }

        public static IEnumerator SaveVoiceActivity(Action<Response> callback, StationType stationType, DateTime start,
            DateTime end, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Post, "savevoiceactivity", logAction, callback,
                stationType.GetNameForBackend(),
                start.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture),
                end.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture));
        }

        public static IEnumerator SetCreativeTime(Action<Response> callback, int creativePlayerID, DateTime start,
            DateTime end, bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "setcreativetime", logAction, callback, creativePlayerID,
                start.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture),
                end.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture));
        }

        public static IEnumerator SaveEveningActivityStation(Action<Response> callback, StationType stationType,
            bool logAction = false)
        {
            yield return ExecuteAction(HttpRequestMethod.Get, "saveeveningactivity", logAction, callback,
                stationType.GetNameForBackend());
        }
    }
}