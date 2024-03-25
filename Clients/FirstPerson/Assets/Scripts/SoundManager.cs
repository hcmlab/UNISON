using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (audioSource != null)
        {
            ConfigAudioSource();
        }
    }

    protected virtual void ConfigAudioSource()
    {
    }

    protected void Stop()
    {
        audioSource.loop = false;
        audioSource.Stop();
    }

    // Loop one specific clip
    protected void PlayLooped(AudioClip clip)
    {
        audioSource.loop = true;
        audioSource.clip = clip;
        audioSource.Play();
    }

    protected void PlayOneFadingOut(AudioClip clip)
    {
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();
        StartCoroutine(FadeOut(20f));
    }

    private IEnumerator FadeOut(float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    // Play one specific clip
    protected void PlayOne(AudioClip clip, float volumeScale = 1f)
    {
        if (clip)
        {
            audioSource.PlayOneShot(clip, volumeScale);
        }
    }

    // Play one clip of an array of clips randomly
    protected void PlayRandom(AudioClip[] clips, float volumeScale = 1f)
    {
        if (clips.Length > 0)
        {
            int index = Random.Range(0, clips.Length);
            PlayOne(clips[index], volumeScale);
        }
    }
}