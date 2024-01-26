using System;
using System.Collections;
using UnityEngine;

using Random = UnityEngine.Random;

[RequireComponent(typeof(Renderer))]
public class BloomActivate : MonoBehaviour
{

    new Renderer renderer; // new hides the parent <renderer> property.
    Material material;
    Color emissionColor;

    public GameObject xrCamera;
    public GameObject xrOrigin;


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

    /*IEnumerator*/void  Toggle()
    {
        bool toggle = false;
        float newInten = 1f;
        //while (true)
        //{
            // yield return new WaitForSeconds(0.01f);
            Tuple<double, double> dist = getDistance();
            if (dist.Item1 <= 10 && dist.Item2 <= 10)
            {
                newInten = 5f;
                toggle = true;
            }
            else if (dist.Item1 <= 20 && dist.Item2 <= 20)
            {
                newInten = 3f;
                toggle = true;
            }
            else if (dist.Item1 <= 30 && dist.Item2 <= 30)
            {
                newInten = 2f;
                toggle = true;
            }
        else
            {
                newInten = .05f;
                toggle = false;
            }
            Activate(toggle, newInten);
            /*toggle = true;*/
        // }
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

    Tuple<double, double> getDistance()
    {
        // get player location 
        // get flower location
        double xLoc = /*xrOrigin.transform.position.x*/ -xrCamera.transform.position.x;
        double zLoc = /*xrOrigin.transform.position.z*/ -xrCamera.transform.position.z;
        // figure out difference

        double xDiff = /*Math.Abs(*/-this.transform.position.x/*)*/ - Math.Abs(xLoc);
        double zDiff = /*Math.Abs(*/-this.transform.position.z/*)*/ - Math.Abs(zLoc);

        return new Tuple<double, double>(Math.Abs(xDiff), Math.Abs(zDiff));

        // return new Tuple<double, double>(50, 50);
    }

    void Update()
    {
        Toggle();
    }
}