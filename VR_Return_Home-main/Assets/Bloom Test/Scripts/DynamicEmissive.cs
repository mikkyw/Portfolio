using System;
using System.Collections;
using UnityEngine;

using Random = UnityEngine.Random;

[RequireComponent(typeof(Renderer))]
public class DynamicEmissive : MonoBehaviour
{

    new Renderer renderer; // new hides the parent <renderer> property.
    Material material;
    Color emissionColor;

    private float waitTime = 2.0f;
    private float timer = 0.0f;

    private bool toggle = false;

    public bool range = false;
    public bool increment = false;

    private float intensity = 0.5f;

    void Start()
    {
        // Gets access to the renderer and material components as we need to
        // modify them during runtime.
        renderer = GetComponent<Renderer>();
        material = renderer.material;

        // Gets the initial emission colour of the material, as we have to store
        // the information before turning off the light.
        emissionColor = material.GetColor("_EmissionColor");

        // Start a coroutine to toggle the light on / off.
        /*StartCoroutine(*/Toggle()/*)*/;
    }

    /*IEnumerator*/void Toggle()
    {
        // bool toggle = false;
        // while (true)
        // {
        // yield return new WaitForSeconds(1f);
        if (range)
        {
            Activate(toggle, Random.Range(1f, 5f));
        }
        else if (increment)
        {
            if (intensity >= 5.0f)
                intensity = 0.5f;
            else
                intensity += 0.5f;
            Activate(toggle, intensity);
        }
        else
        {
            Activate(toggle, 3f);
        }
        
        toggle = !toggle;
        //}
    }

    // Call this method to turn on or turn off emissive light.
    public void Activate(bool on, float intensity)
    {
        if (on)
        {

            // Enables emission for the material, and make the material use
            // realtime emission.
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            // Update the emission color and intensity of the material.
            material.SetColor("_EmissionColor", emissionColor * intensity);

            // Makes the renderer update the emission and albedo maps of our material.
            RendererExtensions.UpdateGIMaterials(renderer);

            // Inform Unity's GI system to recalculate GI based on the new emission map.
            DynamicGI.SetEmissive(renderer, emissionColor * intensity);
            DynamicGI.UpdateEnvironment();

        }
        else
        {

            material.DisableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

            material.SetColor("_EmissionColor", Color.black);
            RendererExtensions.UpdateGIMaterials(renderer);

            DynamicGI.SetEmissive(renderer, Color.black);
            DynamicGI.UpdateEnvironment();

        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Check if we have reached beyond 3 seconds.
        // Subtracting two is more accurate over time than resetting to zero.
        if (timer > waitTime)
        {
            Toggle();


            // Remove the recorded 3 seconds.
            timer = 0;
        }
        
    }
}