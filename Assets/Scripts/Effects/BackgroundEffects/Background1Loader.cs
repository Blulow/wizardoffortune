using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class CircleLayer
{
    public Vector2 position = new Vector2(0.5f, 0.5f);
    public float radius = 0.5f;
    public float alpha = 0.5f;
    public float softness = 0.05f;
    public float noiseStrength = 0.1f;
    public float noiseSpeed = 0.1f;
    public float pulseAmount = 0.05f;
    public float pulseSpeed = 3.0f;
}

public class Background1Loader : MonoBehaviour
{
    [SerializeField] CircleLayer[] circleLayers;

    void Start()
    {
        UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
        if (img == null)
        {
            Debug.LogError("no image in object");
            return;
        }
        Material mat = img.material;
        if (mat == null)
        {
            Debug.LogError("no material in image");
            return;
        }

        Vector4[] parsedLayers = ParseCircleLayersToPackedArray(circleLayers);
        
        if (parsedLayers.Length == 0) {
            Debug.Log("no circle layers put");
            return;
        }

        mat.SetVectorArray("_CircleLayers", parsedLayers);
    }

    Vector4[] ParseCircleLayersToPackedArray(CircleLayer[] layers)
    {
        Vector4[] flattenedLayers = new Vector4[layers.Length * 3];

        for (int i = 0; i < layers.Length; i++)
        {
            CircleLayer layer = layers[i];
            flattenedLayers[i * 3 + 0] = new Vector4(layer.position.x, layer.position.y, layer.radius, layer.alpha);
            flattenedLayers[i * 3 + 1] = new Vector4(layer.softness, layer.noiseStrength, layer.noiseSpeed, layer.pulseAmount);
            flattenedLayers[i * 3 + 2] = new Vector4(layer.pulseSpeed, 0, 0, 0);
        }
        return flattenedLayers;
    }
}
