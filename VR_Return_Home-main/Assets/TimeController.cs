using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.UI.Image;

public class TimeController : MonoBehaviour
{
    [SerializeField] private float timerMultiplier;
    [SerializeField] private float startHour;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Light sunLight;
    [SerializeField] private float sunriseHour;
    [SerializeField] private float sunsetHour;
    [SerializeField] private Color dayAmbientLight;
    [SerializeField] private Color nightAmbientLight;
    [SerializeField] private AnimationCurve lightChangeCurve;
    [SerializeField] private float maxSunLightIntensity;
    [SerializeField] private Light moonLight;
    [SerializeField] private float maxMoonLightIntensity;
    [SerializeField] private Color dayFogColor; // New
    [SerializeField] private Color nightFogColor; // New

    [SerializeField] private float fogChangeDuration = 600.0f; // Duration in seconds for the fog color to transition
    [SerializeField] private float fogChangeThreshold = 15.0f; // Time range in minutes around sunrise/sunset for the fog transition

    private Color targetFogColor; // The target fog color for the current phase
    private Color initialFogColor; // The initial fog color for the current phase
    private float fogChangeStartTime; // The time when the fog color transition started


    private DateTime currentTime;
    private TimeSpan sunriseTime;
    private TimeSpan sunsetTime;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
        sunriseTime = TimeSpan.FromHours(sunriseHour);
        sunsetTime = TimeSpan.FromHours(sunsetHour);
        RenderSettings.ambientLight = Color.black;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimeOfDay();
        RotateSun();
        UpdateLightSettings();
    }

    private void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime * timerMultiplier);

        if (timeText != null)
        {
            timeText.text = currentTime.ToString("HH:mm");
        }
    }

    private void RotateSun()
    {
        float sunLightRotation;

        if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime)
        {
            TimeSpan sunriseToSunsetDuration = CalculateTimeDifference(sunriseTime, sunsetTime);
            TimeSpan timeSinceSunrise = CalculateTimeDifference(sunriseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunrise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(0, 180, (float)percentage);
        }
        else
        {
            TimeSpan sunsetToSunriseDuration = CalculateTimeDifference(sunsetTime, sunriseTime);
            TimeSpan timeSinceSunset = CalculateTimeDifference(sunsetTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunset.TotalMinutes / sunsetToSunriseDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(180, 360, (float)percentage);
        }
        sunLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
    }

    private void UpdateLightSettings()
    {
        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        sunLight.intensity = Mathf.Lerp(0, maxSunLightIntensity, lightChangeCurve.Evaluate(dotProduct));
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, lightChangeCurve.Evaluate(dotProduct));

        // Determine if it's day or night and set the fog color accordingly

        //RenderSettings.fogColor = IsDaytime() ? dayFogColor : nightFogColor;
        //if (IsDaytime())
        //{
        //    Color darkenedColor = new Color(
        //                    RenderSettings.fogColor.r - amount,
        //                    RenderSettings.fogColor.g - amount,
        //                    RenderSettings.fogColor.b - amount,
        //                    RenderSettings.fogColor.a
        //                );
        //    RenderSettings.fogColor = darkenedColor;
        //}
        //else
        //{
        //    Color darkenedColor = new Color(
        //        RenderSettings.fogColor.r + amount,
        //        RenderSettings.fogColor.g + amount,
        //        RenderSettings.fogColor.b + amount,
        //        RenderSettings.fogColor.a
        //    );
        //    RenderSettings.fogColor = darkenedColor;
        //}

        //print(RenderSettings.fogColor);

        if (IsDaytime())
        {
            if (IsNearSunriseOrSunset())
            {
                targetFogColor = dayFogColor;
                SmoothlyChangeFogColor();
            }
            else
            {
                // Set the fog color directly to dayFogColor
                RenderSettings.fogColor = dayFogColor;
            }
        }
        else
        {
            if (IsNearSunriseOrSunset())
            {
                targetFogColor = nightFogColor;
                SmoothlyChangeFogColor();
            }
            else
            {
                // Set the fog color directly to nightFogColor
                RenderSettings.fogColor = nightFogColor;
            }
        }
    }

    private bool IsNearSunriseOrSunset()
    {
        // Calculate the time difference between the current time and the nearest sunrise or sunset
        TimeSpan timeToSunrise = CalculateTimeDifference(currentTime.TimeOfDay, sunriseTime);
        TimeSpan timeToSunset = CalculateTimeDifference(currentTime.TimeOfDay, sunsetTime);

        // Check if the current time is within the fogChangeThreshold minutes of sunrise or sunset
        return timeToSunrise.TotalMinutes <= fogChangeThreshold || timeToSunset.TotalMinutes <= fogChangeThreshold;
    }


    private void SmoothlyChangeFogColor()
    {
        if (initialFogColor != targetFogColor)
        {
            if (fogChangeStartTime == 0.0f)
            {
                // Initialize the fog color transition
                initialFogColor = RenderSettings.fogColor;
                fogChangeStartTime = Time.time;
            }

            // Calculate the elapsed time since the fog color transition started
            float elapsedTime = Time.time - fogChangeStartTime;

            // Calculate the interpolation factor based on elapsed time and transition duration
            float t = Mathf.Clamp01(elapsedTime / fogChangeDuration);

            // Smoothly interpolate between the initial and target fog colors
            RenderSettings.fogColor = Color.Lerp(initialFogColor, targetFogColor, t);

            // Check if the fog color transition is complete
            if (t >= 1.0f)
            {
                // Reset variables for the next transition
                initialFogColor = targetFogColor;
                fogChangeStartTime = 0.0f;
            }
        }
    }

    private bool IsDaytime()
    {
        return currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime;
    }

    private TimeSpan CalculateTimeDifference(TimeSpan fromTime, TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;

        if (difference.TotalSeconds < 0)
        {
            difference += TimeSpan.FromHours(24);
        }

        return difference;
    }
}
