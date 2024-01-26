using System.Collections;
using UnityEngine;

public class RandomFootsteps : MonoBehaviour
{
    public AudioClip[] footstepSounds; // Array to hold your footstep sound effects
    private AudioSource audioSource;

    // Adjust these parameters to control the footstep sound timing
    public float minStepInterval = 0.5f;
    public float maxStepInterval = 1.0f;
    public float volumeMultiplier = 0.5f;

    private float nextStepTime;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Initialize the next step time
        nextStepTime = Time.time;
    }

    void Update()
    {
        // Check if the player is moving and if it's time for the next step
        if (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f)
        {
            if (Time.time >= nextStepTime)
            {
                PlayRandomFootstepSound();

                // Set the next step time based on a random interval
                nextStepTime = Time.time + Random.Range(minStepInterval, maxStepInterval);
            }
        }
    }

    void PlayRandomFootstepSound()
    {
        // Check if there are footstep sounds assigned
        if (footstepSounds.Length > 0)
        {
            // Select a random footstep sound from the array
            AudioClip randomFootstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];

            // Play the selected footstep sound
            audioSource.clip = randomFootstepSound;
            audioSource.volume = volumeMultiplier;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("No footstep sounds assigned to the FootstepController!");
        }
    }
}