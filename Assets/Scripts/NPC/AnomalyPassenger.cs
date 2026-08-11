using UnityEngine;

[RequireComponent(typeof(NPCController))]
public class AnomalyPassenger : MonoBehaviour
{
    private NPCController controller;

    public void ApplyAnomalyData()
    {
        controller = GetComponent<NPCController>();
        
        // Ganti data tiketnya jadi kacau balau biar kelihatan seperti anomali/hantu
        controller.passengerData.passengerName = "???";
        controller.passengerData.ticket.ticketID = "ERROR-666";
        controller.passengerData.ticket.originStation = "???";
        controller.passengerData.ticket.destinationStation = "???";
        controller.passengerData.ticket.status = TicketStatus.Fake; 
        
        // Set flag khusus monster biar PerformanceManager bisa ngurangin poin kalau diterima
        controller.passengerData.isMonster = true;

        // Bikin dialognya aneh dan creepy
        string[] creepyReasons = {
            "D__I__N__G__I__N...",
            "B___I___A___R___K___A___N . A___K___U . M___A___S___U___K",
            "M___E___R___E___K___A . D___A___T___A___N___G",
            ". . . . . . . . . .",
            "T__A__K . A__D__A . Y__A__N__G . S__E__L__A__M__A__T"
        };
        controller.passengerData.reason = creepyReasons[Random.Range(0, creepyReasons.Length)];
    }
}
