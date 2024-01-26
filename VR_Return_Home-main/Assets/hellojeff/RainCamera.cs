using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RainCamera : MonoBehaviour
{
    public Material material;

    private Material _currentMaterial;
    private DepthOfFieldEffect _depthOfFieldEffect;

    private void Start()
    {
        _depthOfFieldEffect = GetComponent<DepthOfFieldEffect>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_currentMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit(source, destination, _currentMaterial);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {

            _depthOfFieldEffect.enabled = !_depthOfFieldEffect.enabled;
            if (_depthOfFieldEffect.enabled)
            {
                _currentMaterial = null;
            } else
            {
                _currentMaterial = material;
            }

        }
    }
}
