using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public struct Anim_Type
{
    public float Scale;
    public Anim_Axis AxisX;
    public Anim_Axis AxisY;
    public Anim_Axis AxisZ;

    public Anim_Type(float scale)
    {
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
    public bool Invert;
    public bool Random;
    
    public Anim_Axis(float axisScale)
    {
        AxisScale = axisScale;
        AxisCurve = null;
        Invert = false;
        Random = false;
    }
}

public class PAnim_Recoil : MonoBehaviour
{
    [Button("Fire")]
    void Fire()
    {
        timer = 0;
        randomInvert = Random.value < 0.5f;
    }
    
    [Button("Fire Toggle")]
    public void ToggleFire()
    {
        if (isFiring)
        {
            StopFiring();
        }
        else
        {
            Fire();
            StartFiring();
        }
    }
    private void StartFiring()
    {
        isFiring = true;
        InvokeRepeating("Fire", FireRate, FireRate);
    }
    private void StopFiring()
    {
        isFiring = false;
        CancelInvoke("Fire");
    }
    
    
    [SerializeField] private float FireRate;
    public float slowmoRate = 1;

    public Anim_Type AnimPosition;
    public Anim_Type AnimRotation;
    
    [FoldoutGroup("Debug")]
    [SerializeField] private bool isFiring;
    [FoldoutGroup("Debug")]
    [SerializeField] private bool Recoil;
    [FoldoutGroup("Debug")]
    [SerializeField] private float timer;
    [FoldoutGroup("Debug")]
    [SerializeField, ReadOnly] private float TimeEndPosX;
    [FoldoutGroup("Debug")]
    [SerializeField, ReadOnly] private float TimeEndPosY;
    [FoldoutGroup("Debug")]
    [SerializeField, ReadOnly] private float TimeEndPosZ;
    
    [FoldoutGroup("Debug")]
    [SerializeField, ReadOnly] private float maxEndTime;
    
    private Vector3 defaultLocalPos;
    private Vector3 defaultLocalRot;
    private bool randomInvert;
    
    Vector3 addPos;
    Vector3 addRot;

    void Awake()
    {
        defaultLocalPos = transform.localPosition;
        defaultLocalRot = transform.localRotation.eulerAngles;

        Keyframe endkeyPosX;
        Keyframe endkeyPosY;
        Keyframe endkeyPosZ;
        if (AnimPosition.AxisX.AxisCurve != null && AnimPosition.AxisX.AxisCurve.length > 0)
        {
            endkeyPosX = AnimPosition.AxisX.AxisCurve.keys[AnimPosition.AxisX.AxisCurve.length - 1];
            TimeEndPosX = endkeyPosX.time;
        }

        if (AnimPosition.AxisY.AxisCurve != null && AnimPosition.AxisY.AxisCurve.length > 0)
        {
            endkeyPosY = AnimPosition.AxisY.AxisCurve.keys[AnimPosition.AxisY.AxisCurve.length - 1];
            TimeEndPosY = endkeyPosY.time;
        }

        if (AnimPosition.AxisZ.AxisCurve != null  && AnimPosition.AxisZ.AxisCurve.length > 0)
        {
            endkeyPosZ = AnimPosition.AxisZ.AxisCurve.keys[AnimPosition.AxisZ.AxisCurve.length - 1];
            TimeEndPosZ = endkeyPosZ.time;
        }

        maxEndTime = Mathf.Max(TimeEndPosX, TimeEndPosY, TimeEndPosZ);
        timer = maxEndTime;
    }

    private void Update()
    {
        Kickback();
    }

    void Kickback()
    {
        if (timer > FireRate) Recoil = false;
        else Recoil = true;
        
        
        timer += Time.fixedDeltaTime * slowmoRate;
        
        // Position
        addPos.x = CalculateAxisValue(AnimPosition, AnimPosition.AxisX, randomInvert);
        addPos.y = CalculateAxisValue(AnimPosition, AnimPosition.AxisY, randomInvert);
        addPos.z = CalculateAxisValue(AnimPosition, AnimPosition.AxisZ, randomInvert);

        // Rotation
        addRot.x = CalculateAxisValue(AnimRotation, AnimRotation.AxisX, randomInvert) * Mathf.Rad2Deg;
        addRot.y = CalculateAxisValue(AnimRotation, AnimRotation.AxisY, randomInvert) * Mathf.Rad2Deg;
        addRot.z = CalculateAxisValue(AnimRotation, AnimRotation.AxisZ, randomInvert) * Mathf.Rad2Deg;
        
        transform.localRotation = Quaternion.Euler(defaultLocalRot + addRot);
        transform.localPosition = defaultLocalPos + addPos;
        
    }
    
    float CalculateAxisValue(Anim_Type type, Anim_Axis axis, bool randomInvert = false)
    {
        if (axis.Random)
        {
            return randomInvert ? -axis.AxisCurve.Evaluate(timer) * type.Scale * axis.AxisScale : axis.AxisCurve.Evaluate(timer) * type.Scale * axis.AxisScale;
        }
        else
        {
            return axis.Invert ? -axis.AxisCurve.Evaluate(timer) * type.Scale * axis.AxisScale : axis.AxisCurve.Evaluate(timer) * type.Scale * axis.AxisScale;
        }
    }
}
