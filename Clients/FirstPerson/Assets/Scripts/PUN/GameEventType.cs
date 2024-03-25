namespace PUN
{
    public enum GameEventType
    {
        OnDaylight,
        OnEvening,
        OnJoinedGame,
        OnLeftGame,

        OnStartedDayInHospital,
        OnStartedDayInHome,
        OnReleasedFromHospital,

        // Stations and Zones
        OnStationEntered,
        OnStationLeft,
        OnEveningStationSelected,
        OnGraphsRequested,
        OnGlobalFundsRequested,
        OnMoneyEarned,
        OnLearned,
        OnCarePackageBought,
        OnRelaxed,
        OnBought,
        OnInvested,
        OnMoneySent,
        OnHelpDeskOpened,
        OnVoted,
        OnPetitionRegistered,
        OnPetitionChanged,
        OnPetitionsRequested,
    }
}