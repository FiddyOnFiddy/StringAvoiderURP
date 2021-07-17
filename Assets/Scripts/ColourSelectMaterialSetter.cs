using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColourSelectMaterialSetter : MonoBehaviour
{
    public Material _material;

    [SerializeField] private float distortionIntensity, speed, dissolveScale, dissolveAmount;
    // Start is called before the first frame update
    private void Start()
    {
        //_material = GetComponent<Material>();
        _material = GetComponent<Image>().material;
        _material.SetFloat("_DistortionIntensity", distortionIntensity);
        _material.SetFloat("_Speed", speed);
        _material.SetFloat("_DissolveScale", dissolveScale);
        _material.SetFloat("_DissolveAmount", dissolveAmount);
        
        
    }
}
