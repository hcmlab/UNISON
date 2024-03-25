using System;
using Backend;
using Clients;
using PUN;
using Stations;
using UnityEngine;

public class VoiceActivityTracker : MonoBehaviour, IStateSubscriber<VoiceState>
{
    private PlayerManager currentPlayerManager;

    private (StationType, DateTime)? startedSpeaking;

    public void Setup(PlayerManager playerManager)
    {
        currentPlayerManager = playerManager;
    }

    public void OnUpdated(VoiceState voiceState, VoiceState oldVoiceState)
    {
        if (voiceState == oldVoiceState) return;

        if (voiceState == VoiceState.Speaking)
        {
            if (currentPlayerManager.PunPlayer.CurrentStationType != StationType.None)
            {
                startedSpeaking = (
                    currentPlayerManager.PunPlayer.CurrentStationType,
                    DateTime.Now
                );
            }
        }
        else if (startedSpeaking.HasValue)
        {
            StationType stationType = startedSpeaking.Value.Item1;
            DateTime start = startedSpeaking.Value.Item2;
            DateTime end = DateTime.Now;

            startedSpeaking = null;

            StartCoroutine(BackendConnection.SaveVoiceActivity(response =>
            {
                if (response.IsSuccess())
                {
                    Debug.Log("Saved new speaking time.");
                }
                else
                {
                    Debug.LogError($"Could not upload voice activity: \"{response.GetRawResponse()}\"");
                }
            }, stationType, start, end, true));
        }
    }
}
