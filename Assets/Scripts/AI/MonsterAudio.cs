using System.Collections;
using UnityEngine;

public class MonsterAudio : MonoBehaviour
{
    public AudioSource audioSource;

    // Event Listeners
    [Header("Events")]
    public GameEvent onAudioStopped;

    public void TriggerAudio(Component sender, object data)
    {
        if (data is AudioClip)
        {
            audioSource.PlayOneShot((AudioClip)data);
            // StartCoroutine(ClipCoroutine(audioSource));
        }
    }

    // Not currently checking for when clip ends
    private IEnumerator ClipCoroutine(AudioSource source)
    {
        var waitForClipRemainingTime = new WaitForSeconds(source.GetClipRemainingTime());
        yield return waitForClipRemainingTime;
        onAudioStopped.TriggerEvent(this, true);
    }
}
