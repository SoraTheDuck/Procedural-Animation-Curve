using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    public bool Toggle;
    public float AxisScale;
    public AnimationCurve AxisCurve;
    [SerializeField] public AnimationCurve GenCurve;
    
    public bool Invert;
    public bool Random;
    
    public Anim_Axis(float axisScale)
    {
        Toggle = true;
        AxisScale = axisScale;
        AxisCurve = null;
        GenCurve = null;
        
        Invert = false;
        Random = false;
    }
}

public enum WeaponType
{
    Rifle,
    Shotgun,
    Pistol
}
