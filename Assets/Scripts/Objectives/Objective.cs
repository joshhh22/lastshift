using UnityEngine;

[System.Serializable]
public class Objective
{
    [TextArea]
    public string title;

    [HideInInspector]
    public bool completed;
}