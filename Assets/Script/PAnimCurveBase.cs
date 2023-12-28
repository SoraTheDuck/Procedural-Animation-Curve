using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

[Serializable]
public struct Anim_Type
{
    public bool Toggle;
    public float Scale;
    public Anim_Axis AxisX;
    public Anim_Axis AxisY;
    public Anim_Axis AxisZ;

    public Anim_Type(float scale)
    {
        Toggle = true;
        Scale = scale;
        AxisX = new Anim_Axis(1f);
        AxisY = new Anim_Axis(1f);
        AxisZ = new Anim_Axis(1f);
    }
}

[Serializable]
public struct Anim_Axis
{
    public float AxisScale;
    public AnimationCurve AxisCurve;
    [SerializeField] public AnimationCurve GenCurve;
    
    [FoldoutGroup("Settings")] public bool Toggle;
    [FoldoutGroup("Settings")] public bool Invert;
    [FoldoutGroup("Settings")] public bool Random;
    [FoldoutGroup("Settings")][Button("Clear")] void ClearCurve() => RemoveAllKeyframes(GenCurve);
    [FoldoutGroup("Settings")] public float RandomMin;
    [FoldoutGroup("Settings")] public float RandomMax;
    [FoldoutGroup("Settings")] public float ScaleRandom;

    
    
    public Anim_Axis(float axisScale)
    {
        Toggle = true;
        AxisScale = axisScale;
        AxisCurve = null;
        GenCurve = null;
        
        Invert = false;
        Random = false;
        RandomMin = -0.02f;
        RandomMax = 0.03f;
        ScaleRandom = 1;
    }
    
    void RemoveAllKeyframes(AnimationCurve curve)
    {
        List<Keyframe> keyframes = new List<Keyframe>(curve.keys);
        keyframes.Clear();
        curve.keys = keyframes.ToArray();
    }
}

public enum WeaponType
{
    Rifle,
    Shotgun,
    Pistol
}
