using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Settings.AudioSettings;

namespace UI
{
    public enum TransitionType
    {
        State,
        Position
    }

    public enum TransitionSwitch
    {
        UnFaded,
        FadingOut,
        Faded,
        FadingIn
    }

    public class TransitionState
    {
        public float Duration;
        public TransitionSwitch Switch;
        public float Value;
        public bool InTransition => Switch == TransitionSwitch.FadingOut || Switch == TransitionSwitch.Faded;
    }

    public class TransitionCanvas : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AudioMixer audioMixer;

        [SerializeField] private float stateFadeDuration = 1.5f;
        [SerializeField] private float positionFadeDuration = 0.5f;

        private Dictionary<TransitionType, TransitionState> transitionStates;
        public bool InTransition => transitionStates.Values.Any(state => state.InTransition);

        private void Awake()
        {
            transitionStates = new Dictionary<TransitionType, TransitionState>
            {
                [TransitionType.State] = new TransitionState { Duration = stateFadeDuration },
                [TransitionType.Position] = new TransitionState { Duration = positionFadeDuration }
            };
        }

        private void Start()
        {
            UpdateTransition();
        }

        private void UpdateTransition()
        {
            var fadeValue = transitionStates.Values.Select(state => state.Value).Max();

            canvasGroup.alpha = fadeValue;
            canvasGroup.blocksRaycasts = InTransition;
            audioMixer.SetFloat("WorldVolume", AudioSettings.ScaleVolume(1 - fadeValue));
        }

        public void FadeOutImmediately(TransitionType transitionType)
        {
            var transitionState = transitionStates[transitionType];
            transitionState.Value = 1;
            transitionState.Switch = TransitionSwitch.Faded;
            UpdateTransition();
        }

        public void FadeInImmediately(TransitionType transitionType)
        {
            var transitionState = transitionStates[transitionType];
            transitionState.Value = 0;
            transitionState.Switch = TransitionSwitch.UnFaded;
            UpdateTransition();
        }

        public void FadeOut(TransitionType transitionType, Action afterFade = null)
        {
            var transitionState = transitionStates[transitionType];
            transitionState.Switch = TransitionSwitch.FadingOut;

            StartCoroutine(FadeOutRoutine(transitionState, () =>
            {
                if (transitionState.Switch == TransitionSwitch.FadingOut)
                {
                    transitionState.Value = 1;
                    transitionState.Switch = TransitionSwitch.Faded;
                    UpdateTransition();
                }

                afterFade?.Invoke();
            }));
        }

        public void FadeIn(TransitionType transitionType, Action afterFade = null)
        {
            var transitionState = transitionStates[transitionType];
            transitionState.Switch = TransitionSwitch.FadingIn;

            StartCoroutine(FadeInRoutine(transitionState, () =>
            {
                if (transitionState.Switch == TransitionSwitch.FadingIn)
                {
                    transitionState.Value = 0;
                    transitionState.Switch = TransitionSwitch.UnFaded;
                    UpdateTransition();
                }

                afterFade?.Invoke();
            }));
        }

        private IEnumerator FadeOutRoutine(TransitionState transitionState, Action afterFade)
        {
            if (transitionState.Duration > 0)
            {
                while (transitionState.Value < 1)
                {
                    if (transitionState.Switch != TransitionSwitch.FadingOut)
                    {
                        break;
                    }

                    transitionState.Value += Time.deltaTime / transitionState.Duration;
                    UpdateTransition();
                    yield return null;
                }
            }

            afterFade.Invoke();
        }

        private IEnumerator FadeInRoutine(TransitionState transitionState, Action afterFade)
        {
            if (transitionState.Duration > 0)
            {
                while (transitionState.Value > 0)
                {
                    if (transitionState.Switch != TransitionSwitch.FadingIn)
                    {
                        break;
                    }

                    transitionState.Value -= Time.deltaTime / transitionState.Duration;
                    UpdateTransition();
                    yield return null;
                }
            }

            afterFade.Invoke();
        }
    }
}