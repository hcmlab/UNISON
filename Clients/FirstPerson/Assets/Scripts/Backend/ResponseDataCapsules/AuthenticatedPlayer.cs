using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class AuthenticatedPlayer
    {
        public int ID { get; }
        public string Token { get; }

        [JsonConstructor]
        public AuthenticatedPlayer(int playerID, string token)
        {
            ID = playerID;
            Token = token;
        }
    }
}