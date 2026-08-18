using UnityEngine;

public class GateManager : MonoBehaviour
{
    public static GateManager Instance { get; private set; }

    [SerializeField] private TicketGate gate;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OpenGate()
    {
        AudioManager.Instance.PlayGateOpen();
        gate.Open();
    }
}