using UnityEngine;

public class NPCIdentity : MonoBehaviour
{
    [SerializeField] private NPCGender gender;

    public NPCGender Gender => gender;

    public string PassengerName => gameObject.name.Replace("(Clone)", "").Trim();
}