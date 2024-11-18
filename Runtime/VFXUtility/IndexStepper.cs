using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;



[ExecuteAlways]
public class IndexStepper : MonoBehaviour
{
    public bool pause = true;
    public float currentIndexTime = 0f;
    public float stepsPerSecond = 1f;
    
    private VisualEffect _effect;
    private void Start()
    {
        _effect = gameObject.GetComponent<VisualEffect>();
    }

    private void Update()
    {
        if (!pause)
        {
            currentIndexTime += Time.deltaTime * stepsPerSecond;
            _effect.SetFloat("indexTime", currentIndexTime);
        }
    }
}
