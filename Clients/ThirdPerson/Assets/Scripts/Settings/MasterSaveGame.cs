using System;
using ExitGames.Client.Photon;
using World;

namespace Settings
{
    [Serializable]
    public class MasterSaveGame
    {
        public string instanceUuid;
        public string playerName;

        public int lobbySize;
        public int daylightDuration;
        public float relaxCooldownInSeconds;

        public int gameRound;
        public DayState dayState;
        public float daytimeProgress;

        public MasterSaveGame()
        {
        } // Necessary for deserialization

        public MasterSaveGame(string instanceUuid, string playerName, int lobbySize, int daylightDuration,
            float relaxCooldownInSeconds, int gameRound, DayState dayState, float daytimeProgress)
        {
            this.instanceUuid = instanceUuid;
            this.playerName = playerName;

            this.lobbySize = lobbySize;
            this.daylightDuration = daylightDuration;
            this.relaxCooldownInSeconds = relaxCooldownInSeconds;

            this.gameRound = gameRound;
            this.dayState = dayState;
            this.daytimeProgress = daytimeProgress;
        }

        public MasterSaveGame(string instanceUuid, string playerName, int lobbySize, Hashtable roomProperties)
        {
            this.instanceUuid = instanceUuid;
            this.playerName = playerName;

            this.lobbySize = lobbySize;
            daylightDuration = (int)roomProperties["daylightDuration"];
            relaxCooldownInSeconds = (float)roomProperties["relaxCooldownInSeconds"];

            gameRound = (int)roomProperties["gameRound"];
            dayState = (DayState)roomProperties["dayState"];
            daytimeProgress = (float)roomProperties["daytimeProgress"];
        }
    }
}