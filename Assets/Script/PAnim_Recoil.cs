using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class PAnim_Recoil : MonoBehaviour
{
    [Button("Fire")]
    void Fire()
    {
        timer = 0;
        randomInvert = Random.value < 0.5f;
        
        if(GenerateGraphMode) GenerateRandomCurves();
    }
    
    [Button("Fire Toggle")]
    public void ToggleFire(bool toggle = true)
    {
        if (!toggle)
        {
            StopFiring();
        }
        else
        {
            StopFiring();
            Fire();
            StartFiring();
        }
    }
    private void StartFiring()
    {
        autoFire = true;
        InvokeRepeating("Fire", FireRate, FireRate);
    }
    private void StopFiring()
    {
        autoFire = false;
        CancelInvoke("Fire");
    }

    private void OnValidate()
    {
        if (autoFire)
        {
            StopFiring();
            Fire();
            StartFiring();   
        }
    }


    [SerializeField] private float FireRate;
    public float slowmoRate = 1;

    public Anim_Type AnimPosition;
    public Anim_Type AnimRotation;

    [FoldoutGroup("Test")] 
    [SerializeField] private bool GenerateGraphMode = false;
    
    [FoldoutGroup("Debug")]
    [SerializeField] private bool autoFire;
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
        if (timer > maxEndTime + 1) return;
        
        if (timer > FireRate) Recoil = false;
        else Recoil = true;
        
        
        timer += Time.fixedDeltaTime * slowmoRate;

        if (GenerateGraphMode)
        {
            // Position Test
            addPos.x = CalculateAxisValue_RandomType(AnimPosition, AnimPosition.AxisX, randomInvert);
            addPos.y = CalculateAxisValue_RandomType(AnimPosition, AnimPosition.AxisY, randomInvert);
            addPos.z = CalculateAxisValue_RandomType(AnimPosition, AnimPosition.AxisZ, randomInvert);
        
            // Rotation Test
            addRot.x = CalculateAxisValue_RandomType(AnimRotation, AnimRotation.AxisX, randomInvert) * Mathf.Rad2Deg;
            addRot.y = CalculateAxisValue_RandomType(AnimRotation, AnimRotation.AxisY, randomInvert) * Mathf.Rad2Deg;
            addRot.z = CalculateAxisValue_RandomType(AnimRotation, AnimRotation.AxisZ, randomInvert) * Mathf.Rad2Deg;
        }
        else
        {
            // Position
            addPos.x = CalculateAxisValue(AnimPosition, AnimPosition.AxisX, randomInvert);
            addPos.y = CalculateAxisValue(AnimPosition, AnimPosition.AxisY, randomInvert);
            addPos.z = CalculateAxisValue(AnimPosition, AnimPosition.AxisZ, randomInvert);

            // Rotation
            addRot.x = CalculateAxisValue(AnimRotation, AnimRotation.AxisX, randomInvert) * Mathf.Rad2Deg;
            addRot.y = CalculateAxisValue(AnimRotation, AnimRotation.AxisY, randomInvert) * Mathf.Rad2Deg;
            addRot.z = CalculateAxisValue(AnimRotation, AnimRotation.AxisZ, randomInvert) * Mathf.Rad2Deg;
        }
        
        if(AnimPosition.Toggle)
            transform.localPosition = defaultLocalPos + addPos;
        if(AnimRotation.Toggle) 
            transform.localRotation = Quaternion.Euler(defaultLocalRot + addRot);
        
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
    
    float CalculateAxisValue_RandomType(Anim_Type type, Anim_Axis axis, bool randomInvert = false)
    {
        if (!axis.Toggle || axis.GenCurve == null || axis.AxisCurve.length <= 0) return 0;
        if (axis.Random)
        {
            return randomInvert ? -axis.GenCurve.Evaluate(timer) * type.Scale * axis.AxisScale : axis.GenCurve.Evaluate(timer) * type.Scale * axis.AxisScale;
        }
        else
        {
            return axis.Invert ? -axis.GenCurve.Evaluate(timer) * type.Scale * axis.AxisScale : axis.GenCurve.Evaluate(timer) * type.Scale * axis.AxisScale;
        }
    }

    [Button("Gen Curve")]
    void GenerateRandomCurves()
    {
        /* nope, not gonna use it cuz too lazy, haha
        AnimPosition.AxisX.AxisCurve = GenerateRandomCurve_Curve(AnimPosition.AxisX.AxisCurve, AnimPosition.AxisX.AxisCurve2);
        AnimPosition.AxisY.AxisCurve = GenerateRandomCurve_Curve(AnimPosition.AxisY.AxisCurve, AnimPosition.AxisY.AxisCurve2);
        AnimPosition.AxisZ.AxisCurve = GenerateRandomCurve_Curve(AnimPosition.AxisZ.AxisCurve, AnimPosition.AxisZ.AxisCurve2);

        AnimRotation.AxisX.AxisCurve = GenerateRandomCurve_Curve(AnimRotation.AxisX.AxisCurve, AnimRotation.AxisX.AxisCurve2);
        AnimRotation.AxisY.AxisCurve = GenerateRandomCurve_Curve(AnimRotation.AxisY.AxisCurve, AnimRotation.AxisY.AxisCurve2);
        AnimRotation.AxisZ.AxisCurve = GenerateRandomCurve_Curve(AnimRotation.AxisZ.AxisCurve, AnimRotation.AxisZ.AxisCurve2);
        */

        AnimPosition.AxisX.GenCurve = GenerateRandomCurve(AnimPosition.AxisX.AxisCurve, -0.02f, 0.02f, AnimPosition.AxisX.Toggle);
        AnimPosition.AxisY.GenCurve = GenerateRandomCurve(AnimPosition.AxisY.AxisCurve, -0.02f, 0.02f, AnimPosition.AxisY.Toggle);
        AnimPosition.AxisZ.GenCurve = GenerateRandomCurve(AnimPosition.AxisZ.AxisCurve, -0.02f, 0.02f, AnimPosition.AxisZ.Toggle);
        
        AnimRotation.AxisX.GenCurve = GenerateRandomCurve(AnimRotation.AxisX.AxisCurve, -0.005f, 0.005f, AnimRotation.AxisX.Toggle);
        AnimRotation.AxisY.GenCurve = GenerateRandomCurve(AnimRotation.AxisY.AxisCurve, -0.005f, 0.005f, AnimRotation.AxisY.Toggle);
        AnimRotation.AxisZ.GenCurve = GenerateRandomCurve(AnimRotation.AxisZ.AxisCurve, -0.005f, 0.005f, AnimRotation.AxisZ.Toggle);
    }
    AnimationCurve GenerateRandomCurve_Curve(AnimationCurve curve1, AnimationCurve curve2)
    {
        AnimationCurve randomCurve = new AnimationCurve();

        int numPoints = Mathf.Min(curve1.length, curve2.length);

        for (int i = 0; i < numPoints; i++)
        {
            float randomTime = Random.Range(curve1.keys[i].time, curve2.keys[i].time);
            float randomValue = Random.Range(curve1.keys[i].value, curve2.keys[i].value);
            randomCurve.AddKey(randomTime, randomValue);
        }

        return randomCurve;
    }
    AnimationCurve GenerateRandomCurve(AnimationCurve curve, float range_Min, float range_Max, bool toggle)
    {
        if (curve.length <= 0) return null;
        
        AnimationCurve randomCurve = new AnimationCurve();
        if (!toggle) return randomCurve;
        
        // Add keyframe at time 0 with value 0
        randomCurve.AddKey(new Keyframe(0f, 0f));

        
        for (int i = 0; i < curve.keys.Length; i++)
        {
            Keyframe theKey = curve.keys[i];
            
            float randomTime = theKey.time;
            theKey.value += Random.Range(range_Min, range_Max);

            if (i == curve.keys.Length - 1) theKey.value = 0;
            
            randomCurve.AddKey(theKey);
        }
        
        return randomCurve;
    }
}
