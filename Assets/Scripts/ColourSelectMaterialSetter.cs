using UnityEngine;
using UnityEngine.Serialization;

public class ColourSelectMaterialSetter : MonoBehaviour
{
    [FormerlySerializedAs("_material")] public Material material;

    [SerializeField] private float distortionIntensity, speed, dissolveScale, dissolveAmount;
    // Start is called before the first frame update
    private void Start()
    {
        material = GetComponent<Material>();
        //_material = GetComponent<Image>().material;
        material.SetFloat("_DistortionIntensity", distortionIntensity);
        material.SetFloat("_Speed", speed);
        material.SetFloat("_DissolveScale", dissolveScale);
        material.SetFloat("_DissolveAmount", dissolveAmount);
        
        
    }
}
