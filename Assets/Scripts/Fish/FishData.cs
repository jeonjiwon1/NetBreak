using UnityEngine;

[CreateAssetMenu(
    fileName = "FishData_",
    menuName = "NetBreak/Fish Data"
)]
public class FishData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string fishName;

    [Header("Capture")]
    [SerializeField] private float maxResistance = 10f;
    [SerializeField] private int catchValue = 1;
    [SerializeField] private int goldReward = 1;
    [SerializeField] private int expReward = 1;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float baitAttraction = 1f;
    [SerializeField] private float schoolStrength = 1f;
    [SerializeField] private float lifetime = 15f;

    public string FishName => fishName;
    public float MaxResistance => maxResistance;
    public int CatchValue => catchValue;
    public int GoldReward => goldReward;
    public int ExpReward => expReward;

    public float MoveSpeed => moveSpeed;
    public float BaitAttraction => baitAttraction;
    public float SchoolStrength => schoolStrength;
    public float Lifetime => lifetime;
}