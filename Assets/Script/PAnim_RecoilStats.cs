using UnityEngine;

public enum WeaponType
{
    Rifle,
    Shotgun,
    Pistol
}

[CreateAssetMenu(fileName = "RecoilSettings", menuName = "ScriptableObjects/RecoilSettings")]
public class RecoilSettings : ScriptableObject
{
    public string name;
    public WeaponType weaponType;
    public Anim_Type AnimPosition;
    public Anim_Type AnimRotation;
}