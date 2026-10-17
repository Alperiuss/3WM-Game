using UnityEngine;

[CreateAssetMenu(fileName = "InteractableData", menuName = "Scriptable Objects/InteractableData")]
public class InteractableData : ScriptableObject
{
    public string objectName;
    public AudioClip interactionSound;
}
