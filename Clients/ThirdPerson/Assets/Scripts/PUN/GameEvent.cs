using System;
using System.Linq;
using Photon.Realtime;
using Stations;
using Stations.Zones.Mall;
using Stations.Zones.TownHall;
using UnityEngine;

namespace PUN
{
    public class GameEvent
    {
        private const string DateTimeUtcFormat = "yyyy-MM-ddTHH:mm:ssK";
        private readonly object[] additionalData;
        private readonly string createdAt;

        private readonly Player player;

        public GameEvent(Player player, GameEventType gameEventType, GameEventResult gameEventResult,
            params object[] additionalData)
        {
            this.player = player;
            GameEventType = gameEventType;
            GameEventResult = gameEventResult;
            createdAt = DateTime.Now.ToUniversalTime().ToString(DateTimeUtcFormat);
            this.additionalData = additionalData;
        }

        public GameEvent(Player player, object[] data)
        {
            this.player = player;
            GameEventType = (GameEventType)data[0];
            GameEventResult = (GameEventResult)data[1];
            createdAt = (string)data[2];
            additionalData = data.Skip(3).ToArray();
        }

        public GameEventType GameEventType { get; }
        public GameEventResult GameEventResult { get; }

        public object[] GetData()
        {
            var data = new object[additionalData.Length + 3];
            data[0] = GameEventType;
            data[1] = GameEventResult;
            data[2] = createdAt;
            for (var i = 0; i < additionalData.Length; ++i)
            {
                data[i + 3] = additionalData[i];
            }

            return data;
        }

        public string GetFeedMessage()
        {
            switch (GameEventType)
            {
                case GameEventType.OnDaylight:
                    var daylightRound = (int)additionalData[0];
                    return $"Round {daylightRound}: It has become day.";

                case GameEventType.OnEvening:
                    var eveningRound = (int)additionalData[0];
                    return $"Round {eveningRound}: It has become evening.";

                case GameEventType.OnJoinedGame:
                    return $"{player.NickName} joined the game.";

                case GameEventType.OnLeftGame:
                    return $"{player.NickName} left the game.";

                case GameEventType.OnStartedDayInHospital:
                    var numberDayInHospital = (int)additionalData[0];
                    return $"{player.NickName} started the day in the hospital for {numberDayInHospital} day(s).";

                case GameEventType.OnStartedDayInHome:
                    return $"{player.NickName} started the day at home.";

                case GameEventType.OnReleasedFromHospital:
                    return $"{player.NickName} was released from hospital and started the day at home.";


                case GameEventType.OnStationEntered:
                    var newStation = (StationType)additionalData[0];
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} entered {newStation}.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to enter {newStation}.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to enter {newStation}.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnStationLeft:
                    var oldStation = (StationType)additionalData[0];
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} left {oldStation}.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to leave {oldStation}.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to leave {oldStation}.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnEveningStationSelected:
                    var eveningStation = (StationType)additionalData[0];
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} spends their evening at {eveningStation}.",
                        GameEventResult.NotAllowed =>
                            $"{player.NickName} was not allowed to spend their evening at {eveningStation}.",
                        GameEventResult.Error =>
                            $"Error when {player.NickName} tried to spend their evening at {eveningStation}.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnGlobalFundsRequested:
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} requested global funds.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to request global funds.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to request global funds.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnGraphsRequested:
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} requested graphs.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to request graphs.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to request graphs.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnPetitionsRequested:
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} requested petitions.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to request petitions.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to request petitions.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnMoneyEarned:
                    return GameEventResult switch
                    {
                        GameEventResult.Success =>
                            $"{player.NickName} earned {additionalData[0]} money units by working.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to earn money units.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to earn money units.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnLearned:
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} learned.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to learn.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to learn.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnCarePackageBought:
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} bought a care package.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to buy a care package.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to buy a care package.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnRelaxed:
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} relaxed.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to relax.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to relax.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnBought:
                    var boughtProduct = (Product)additionalData[0];
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} bought {boughtProduct}.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to buy {boughtProduct}.",
                        GameEventResult.Error => $"Error when {player.NickName} tried buy {boughtProduct}.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnInvested:
                    var investedFund = (Fund)additionalData[0];
                    var investedMoneyUnits = (double)additionalData[1];
                    return GameEventResult switch
                    {
                        GameEventResult.Success =>
                            $"{player.NickName} invested {investedMoneyUnits} money units into {investedFund} fund.",
                        GameEventResult.NotAllowed =>
                            $"{player.NickName} was not allowed to invest {investedMoneyUnits} money units into {investedFund} fund.",
                        GameEventResult.Error =>
                            $"Error when {player.NickName} tried to invest {investedMoneyUnits} money units into {investedFund} fund.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnMoneySent:
                    var receiverName = (string)additionalData[0];
                    var moneyUnitsSent = (double)additionalData[1];
                    return GameEventResult switch
                    {
                        GameEventResult.Success =>
                            $"{player.NickName} sent {moneyUnitsSent} money units to {receiverName}.",
                        GameEventResult.NotAllowed =>
                            $"{player.NickName} was not allowed to send {moneyUnitsSent} money units to {receiverName}.",
                        GameEventResult.Error =>
                            $"Error when {player.NickName} tried to send {moneyUnitsSent} money units to {receiverName}.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnHelpDeskOpened:
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} opened the help desk.",
                        GameEventResult.NotAllowed => $"{player.NickName} was not allowed to open the help desk.",
                        GameEventResult.Error => $"Error when {player.NickName} tried to open the help desk.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnVoted:
                    var petitionID = (int)additionalData[0];
                    var positiveVote = (bool)additionalData[1];
                    var voteResult = positiveVote ? "YES" : "NO";
                    return GameEventResult switch
                    {
                        GameEventResult.Success => $"{player.NickName} voted {voteResult} on petition #{petitionID}.",
                        GameEventResult.NotAllowed =>
                            $"{player.NickName} was not allowed to vote {voteResult} on petition #{petitionID}.",
                        GameEventResult.Error =>
                            $"Error when {player.NickName} tried to vote {voteResult} on petition #{petitionID}.",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnPetitionRegistered:
                    var petitionText = (string)additionalData[0];
                    return GameEventResult switch
                    {
                        GameEventResult.Success =>
                            $"{player.NickName} registered new petition \"{petitionText}\".",
                        GameEventResult.NotAllowed =>
                            $"{player.NickName} was not allowed to register new petition \"{petitionText}\".",
                        GameEventResult.Error =>
                            $"Error when {player.NickName} tried to register new petition \"{petitionText}\".",
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                case GameEventType.OnPetitionChanged:
                    var petitionAction = (PetitionAction)additionalData[0];
                    var changedPetitionID = (int)additionalData[1];
                    return GameEventResult switch
                    {
                        GameEventResult.Success => petitionAction switch
                        {
                            PetitionAction.Open => $"{player.NickName} opened petition #{changedPetitionID}.",
                            PetitionAction.Close => $"{player.NickName} closed petition #{changedPetitionID}.",
                            PetitionAction.Delete => $"{player.NickName} deleted petition #{changedPetitionID}.",
                            _ => throw new UnityException($"Unknown petition action \"{petitionAction}\"!")
                        },
                        GameEventResult.NotAllowed => petitionAction switch
                        {
                            PetitionAction.Open =>
                                $"{player.NickName} was not allowed to open petition #{changedPetitionID}.",
                            PetitionAction.Close =>
                                $"{player.NickName} was not allowed to close petition #{changedPetitionID}.",
                            PetitionAction.Delete =>
                                $"{player.NickName} was not allowed to delete petition #{changedPetitionID}.",
                            _ => throw new UnityException($"Unknown petition action \"{petitionAction}\"!")
                        },
                        GameEventResult.Error => petitionAction switch
                        {
                            PetitionAction.Open =>
                                $"Error when {player.NickName} tried to open petition #{changedPetitionID}.",
                            PetitionAction.Close =>
                                $"Error when {player.NickName} tried to close petition #{changedPetitionID}.",
                            PetitionAction.Delete =>
                                $"Error when {player.NickName} tried to delete petition #{changedPetitionID}.",
                            _ => throw new UnityException($"Unknown petition action \"{petitionAction}\"!")
                        },
                        _ => throw new UnityException($"Unknown game event result \"{GameEventResult}\"!")
                    };

                default:
                    throw new UnityException($"Unknown game event type \"{GameEventType}\"!");
            }
        }
    }
}