using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSwap : MonoBehaviour
{
    public AudioClip newTrack;

    private void OnTriggerEnter(Collider collider)
    {
        AudioManager.instance.ReturnToDefault();
    }
    private void OnTriggerExit(Collider collider)
    {
        AudioManager.instance.SwapTrack(newTrack);

    }
}
