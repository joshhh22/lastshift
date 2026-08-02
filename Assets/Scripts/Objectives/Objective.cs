using UnityEngine;

[System.Serializable]
public class Objective
{
    [TextArea]
    public string title;

    public int targetAmount = 0;

    [HideInInspector]
    public int currentAmount = 0;

    [HideInInspector]
    public bool completed;
}