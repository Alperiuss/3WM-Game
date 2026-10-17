using UnityEngine;

[CreateAssetMenu(fileName = "InteractableData", menuName = "Scriptable Objects/InteractableData")]
public class InteractableData : ScriptableObject
{
    [Header("UI & Task Info")]
    public string objectName;
    [TextArea]
    public string taskDescription;
    public bool requiresItem;            // Eşya gerektiriyor mu? (Örn: Electrical Tape)
    public string requiredItemName;      // Gereken eşyanın adı

    [Header("Stress Mechanics")]
    public bool isNoiseMaker;

    public AudioClip interactionSound;
}