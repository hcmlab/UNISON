using System;

namespace Settings
{
    [Serializable]
    public class PlayerSaveGame
    {
        public string InstanceUuid { get; private set; }
        public int PlayerID { get; private set; }
        public string PlayerName { get; private set; }

        public PlayerSaveGame(string instanceUuid, int playerID, string playerName)
        {
            InstanceUuid = instanceUuid;
            PlayerID = playerID;
            PlayerName = playerName;
        }
    }
}