using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidShaderUpdater : MonoBehaviour
{
    public float soundRadius = 5f;
    // Start is called before the first frame update
    void Start()
    {
        Shader.SetGlobalFloat("_SoundRadius" , soundRadius);
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_SoundOrigin" , transform.position);
        Debug.Log("SoundOrigin: " + transform.position);
    }
}
