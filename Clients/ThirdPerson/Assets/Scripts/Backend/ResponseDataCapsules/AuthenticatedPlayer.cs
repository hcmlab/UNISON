using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class AuthenticatedPlayer
    {
        [JsonConstructor]
        public AuthenticatedPlayer(int playerID, string token)
        {
            ID = playerID;
            Token = token;
        }

        public int ID { get; }
        public string Token { get; }
    }
}