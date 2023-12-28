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
    #region Variables
    
    [SerializeField] private float FireRate;
    public float slowmoRate = 1;

    [FoldoutGroup("Animation")] 
    public Anim_Type AnimPosition;
    [FoldoutGroup("Animation")] 
    public Anim_Type AnimRotation;

    [FoldoutGroup("Other Options")] 
    [SerializeField] private bool GenerateGraphMode = false;

    #region Debug vars
    [FoldoutGroup("Debug")]
    [SerializeField] private bool autoFire;
    [FoldoutGroup("Debug")]
    [SerializeField] private bool Recoil;
    [FoldoutGroup("Debug")]
    [SerializeField] private float timer;
    
    //[FoldoutGroup("Debug")] [SerializeField] 
    private float TimeEndPosX;
    //[FoldoutGroup("Debug")] [SerializeField] 
    private float TimeEndPosY;
    //[FoldoutGroup("Debug")] [SerializeField] 
    private float TimeEndPosZ;
    //[FoldoutGroup("Debug")] [SerializeField] 
    private float TimeEndRotX;
    //[FoldoutGroup("Debug")] [SerializeField] 
    private float TimeEndRotY;
    //[FoldoutGroup("Debug")] [SerializeField] 
    private float TimeEndRotZ;
    
    [FoldoutGroup("Debug")]
    [SerializeField, ReadOnly] private float maxEndTime;
    #endregion

    #region for Calculate
    
    private Vector3 defaultLocalPos;
    private Vector3 defaultLocalRot;
    private bool randomInvert;
    
    Vector3 addPos;
    Vector3 addRot;
    
    #endregion
    
    #endregion
    

    #region Pure Calculate

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
    
    
    AnimationCurve GenerateRandomCurve_Curve(AnimationCurve curve1, AnimationCurve curve2)
    {
        AnimationCurve randomCurve = new AnimationCurve();

        int numPoints = Mathf.Max(curve1.length, curve2.length);
        float prevTime = Mathf.NegativeInfinity; // make sure duplicate wont ded :P
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1); // Interpolation parameter

            float value1 = curve1.Evaluate(t);
            float value2 = curve2.Evaluate(t);

            if (Mathf.Approximately(t, prevTime)) randomCurve.AddKey(new Keyframe(t, value1));
            else
            {
                float randomValue = Random.Range(value1, value2);
                randomCurve.AddKey(new Keyframe(t, randomValue));
            }

            prevTime = t;
        }

        return randomCurve;
    }
    
    AnimationCurve GenerateRandomCurve(Anim_Axis animAxis, bool RotationMode = false)
    {
        if (animAxis.AxisCurve.length <= 0) return null;
        
        AnimationCurve randomCurve = new AnimationCurve();
        if (!animAxis.Toggle) return randomCurve;
        
        // Add keyframe at time 0 with value 0
        randomCurve.AddKey(new Keyframe(0f, 0f));

        float min = animAxis.RandomMin * animAxis.ScaleRandom * (RotationMode ? 0.1f : 1f);
        float max = animAxis.RandomMax * animAxis.ScaleRandom * (RotationMode ? 0.1f : 1f);
        
        for (int i = 0; i < animAxis.AxisCurve.keys.Length; i++)
        {
            Keyframe theKey = animAxis.AxisCurve.keys[i];
            
            float randomTime = theKey.time;
            theKey.value += Random.Range(min, max);

            if (i == animAxis.AxisCurve.keys.Length - 1) theKey.value = 0;
            
            randomCurve.AddKey(theKey);
        }
        
        return randomCurve;
    }

    void CalculateMaxTimeEnds()
    {
        Keyframe endkeyPosX, endkeyPosY, endkeyPosZ, endkeyRotX, endkeyRotY, endkeyRotZ;
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
        
        if (AnimRotation.AxisX.AxisCurve != null  && AnimRotation.AxisX.AxisCurve.length > 0)
        {
            endkeyRotX = AnimRotation.AxisX.AxisCurve.keys[AnimRotation.AxisX.AxisCurve.length - 1];
            TimeEndRotX = endkeyRotX.time;
        }
        if (AnimRotation.AxisY.AxisCurve != null  && AnimRotation.AxisY.AxisCurve.length > 0)
        {
            endkeyRotY = AnimRotation.AxisY.AxisCurve.keys[AnimRotation.AxisY.AxisCurve.length - 1];
            TimeEndRotY = endkeyRotY.time;
        }
        if (AnimRotation.AxisZ.AxisCurve != null  && AnimRotation.AxisZ.AxisCurve.length > 0)
        {
            endkeyRotZ = AnimRotation.AxisZ.AxisCurve.keys[AnimRotation.AxisZ.AxisCurve.length - 1];
            TimeEndRotZ = endkeyRotZ.time;
        }
    }

    #endregion

    void Awake()
    {
        defaultLocalPos = transform.localPosition;
        defaultLocalRot = transform.localRotation.eulerAngles;

        CalculateMaxTimeEnds();
        maxEndTime = Mathf.Max(TimeEndPosX, TimeEndPosY, TimeEndPosZ, TimeEndRotX, TimeEndRotY, TimeEndRotZ);
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
            // Position Graph Gen
            addPos.x = CalculateAxisValue_RandomType(AnimPosition, AnimPosition.AxisX, randomInvert);
            addPos.y = CalculateAxisValue_RandomType(AnimPosition, AnimPosition.AxisY, randomInvert);
            addPos.z = CalculateAxisValue_RandomType(AnimPosition, AnimPosition.AxisZ, randomInvert);
        
            // Rotation Graph Gen
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
    
    
    #region Activate and Inspector
    
    //[BoxGroup("Buttons")]
    //[Button("Gen Curve")]
    void GenerateRandomCurves()
    {
        AnimPosition.AxisX.GenCurve = GenerateRandomCurve(AnimPosition.AxisX);
        AnimPosition.AxisY.GenCurve = GenerateRandomCurve(AnimPosition.AxisY);
        AnimPosition.AxisZ.GenCurve = GenerateRandomCurve(AnimPosition.AxisZ);

        AnimRotation.AxisX.GenCurve = GenerateRandomCurve(AnimRotation.AxisX, true);
        AnimRotation.AxisY.GenCurve = GenerateRandomCurve(AnimRotation.AxisY, true);
        AnimRotation.AxisZ.GenCurve = GenerateRandomCurve(AnimRotation.AxisZ, true);
    }
    
    [BoxGroup("Buttons")]
    [Button("Fire")]
    void Fire()
    {
        timer = 0;
        randomInvert = Random.value < 0.5f;
        
        if(GenerateGraphMode) GenerateRandomCurves();
    }
    
    [BoxGroup("Buttons")]
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
    
    #endregion

}
