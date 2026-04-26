using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcholocationPulse : MonoBehaviour
{
    public float pulseMaxRadius = 15f;
    public float pulseDuration = 0.8f;
    public KeyCode pulseKey = KeyCode.E;

    private float _pulseTimer = 0f ;
    private bool _isPulsing = false;
    private VoidShaderUpdater _voidUpdater;

    // Start is called before the first frame update
    void Start()
    {
        _voidUpdater = GetComponent<VoidShaderUpdater>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(pulseKey) && !_isPulsing)
            StartPulse();

        if (_isPulsing)
            UpdatePulse();
    }

    void StartPulse()
    {
        _isPulsing = true;
        _pulseTimer = 0f;

    }

    void UpdatePulse ()
    {
        _pulseTimer += Time.deltaTime;
        float progress  = _pulseTimer / pulseDuration ;


        //Expand out then contract back using a curve
        float radius = Mathf.Sin(progress * Mathf.PI)* pulseMaxRadius;
        Shader.SetGlobalFloat("_SoundRadius" , radius);
        
        if (_pulseTimer >= pulseDuration)
        {
            _isPulsing = false ;

            //Reset tp nase Radius from VoidShaderUpdater
            Shader.SetGlobalFloat("_SoundRadius" , _voidUpdater.soundRadius);
        }

    }
}
