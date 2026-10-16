using UnityEngine;

[CreateAssetMenu(fileName = "InteractableData", menuName = "Scriptable Objects/InteractableData")]
public class InteractableData : ScriptableObject
{
    public string objectName;
    [TextArea] public string interactionPromt = "Press [E] to close";
    public AudioClip interactionSound;
}
