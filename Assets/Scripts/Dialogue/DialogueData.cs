using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Dialogue",
    menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines = new();
}