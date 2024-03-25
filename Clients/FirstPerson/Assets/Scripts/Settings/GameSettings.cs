using UnityEngine;

namespace Settings
{
    /// <summary>
    /// This class holds default values for settings that can be changed in the game as well as
    /// some minimum and maximum values.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private int minDurationDay;
        [SerializeField] private int maxDurationDay;
        [SerializeField] private int defaultDurationDay;

        [SerializeField] private int defaultDurationEvening;

        [SerializeField] private int eveningDialogDuration;
        [SerializeField] private int registerPetitionDialogDuration;

        [SerializeField] private int minLobbySize;
        [SerializeField] private int defaultLobbySize;
        [SerializeField] private int maxLobbySize;

        [SerializeField] private int minCooldown;
        [SerializeField] private int defaultCooldown;

        [SerializeField] private int minPlayerNameLength;
        [SerializeField] private int maxPlayerNameLength;

        [SerializeField] private int minInstanceNameLength;
        [SerializeField] private int maxInstanceNameLength;

        [SerializeField] private float spectatorSpeed;
        [SerializeField] private float spectatorSprintMultiplier;
        [SerializeField] private float playerSpeed;
        [SerializeField] private float playerSprintMultiplier;
        [SerializeField] private float turnSmoothTime;

        [SerializeField] private float refreshTime;

        [SerializeField] private int minVolume;
        [SerializeField] private int maxVolume;

        [SerializeField] private int minVoiceDetectionThreshold;
        [SerializeField] private int maxVoiceDetectionThreshold;

        [SerializeField] private int smallKeyCodeMaxLength;

        public int MinDurationDay => minDurationDay;
        public int MaxDurationDay => maxDurationDay;
        public int DefaultDurationDay => defaultDurationDay;

        public int DefaultDurationEvening => defaultDurationEvening;

        public int EveningDialogDuration => eveningDialogDuration;
        public int RegisterPetitionDialogDuration => registerPetitionDialogDuration;

        public int MinLobbySize => minLobbySize;
        public int MaxLobbySize => maxLobbySize;
        public int DefaultLobbySize => defaultLobbySize;

        public int MinCooldown => minCooldown;
        public int DefaultCooldown => defaultCooldown;

        public int MinPlayerNameLength => minPlayerNameLength;
        public int MaxPlayerNameLength => maxPlayerNameLength;

        public int MinInstanceNameLength => minInstanceNameLength;
        public int MaxInstanceNameLength => maxInstanceNameLength;

        public float SpectatorSpeed => spectatorSpeed;
        public float SpectatorSprintMultiplier => spectatorSprintMultiplier;
        public float PlayerSpeed => playerSpeed;
        public float PlayerSprintMultiplier => spectatorSprintMultiplier;
        public float TurnSmoothTime => turnSmoothTime;


        public float RefreshTime => refreshTime;

        public int MinVolume => minVolume;
        public int MaxVolume => maxVolume;

        public int MinVoiceDetectionThreshold => minVoiceDetectionThreshold;
        public int MaxVoiceDetectionThreshold => maxVoiceDetectionThreshold;

        public int SmallKeyCodeMaxLength => smallKeyCodeMaxLength;
    }
}