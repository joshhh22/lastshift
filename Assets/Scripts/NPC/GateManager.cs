using UnityEngine;

public class GateManager : MonoBehaviour
{
    public static GateManager Instance;

    [SerializeField] private TicketGate gate;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenGate()
    {
        gate.Open();
    }
}