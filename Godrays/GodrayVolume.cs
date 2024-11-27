using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Godray")]
public class GodrayVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter overallScattering = new ClampedFloatParameter(0.25f,-1f,1f,true);
    public ClampedFloatParameter minScattering = new ClampedFloatParameter(0.8f, 0.01f, 3f, true);
    public ClampedFloatParameter maxScattering = new ClampedFloatParameter(3f, 0.01f, 3f, true);
    public ClampedFloatParameter intensity = new ClampedFloatParameter(4.5f, 0.2f, 10f, true);
    public ClampedIntParameter steps = new ClampedIntParameter(35, 5, 100, true);
    public ClampedIntParameter maxDistance = new ClampedIntParameter(5, 1, 10, true);
    public DownSampleModeParameter downSample = new DownSampleModeParameter(DownSample.quarter, true);
    public PhaseFunctionModeParameter phaseFunction = new PhaseFunctionModeParameter(PhaseFunction.HenyeyGreenstein, true);
    public ClampedFloatParameter gaussianAmount = new ClampedFloatParameter(2.5f, 1f, 10f, true);
    public ClampedIntParameter gaussianSamples = new ClampedIntParameter(6, 3, 7, true);

    public float[] getValues()
    {
        float[] hold = new float[10];
        hold[0] = overallScattering.value;
        hold[1] = minScattering.value;
        hold[2] = maxScattering.value;
        hold[3] = intensity.value;
        hold[4] = steps.value;
        hold[5] = maxDistance.value;
        hold[6] = ((float)downSample.value);
        hold[7] = ((float)phaseFunction.value);
        hold[8] = gaussianAmount.value;
        hold[9] = gaussianSamples.value;
        return hold;
    }

    public bool IsActive() => true;
    public bool IsTileCompatible() => false;
}

public enum DownSample
{
    off = 1,
    half = 2,
    quarter = 4
}
[Serializable]
public sealed class DownSampleModeParameter : VolumeParameter<DownSample>
{
    public DownSampleModeParameter(DownSample value, bool overrideState = false) : base(value, overrideState) { }
}

public enum PhaseFunction
{
    HenyeyGreenstein,
    Schlick
}
[Serializable]
public sealed class PhaseFunctionModeParameter : VolumeParameter<PhaseFunction>
{
    public PhaseFunctionModeParameter(PhaseFunction value, bool overrideState = false) : base(value, overrideState) { }
}