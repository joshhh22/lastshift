using System.Collections.Generic;
using UnityEngine;

public class PerformanceManager : MonoBehaviour
{
    public static PerformanceManager Instance { get; private set; }

    [Header("Scores")]
    public int Performance { get; private set; }
    public int Humanity { get; private set; }

    [Header("Statistics")]
    public int CorrectDecisions { get; private set; }
    public int WrongDecisions { get; private set; }
    public int PassengersServed { get; private set; }

    [Header("Failure / Violation Log")]
    private readonly List<string> dayViolations = new List<string>();
    public IReadOnlyList<string> DayViolations => dayViolations;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ResetDay();
    }

    public void ResetDay()
    {
        Performance = 50;
        Humanity = 50;

        CorrectDecisions = 0;
        WrongDecisions = 0;
        PassengersServed = 0;

        dayViolations.Clear();
    }

    public void AddViolation(string violation)
    {
        string timeStamp = GameTimeManager.Instance != null ? GameTimeManager.Instance.FormattedTime : "00:00";
        dayViolations.Add($"[{timeStamp}] {violation}");
    }

    public void RecordCCTVAnomalyFailure(string camName)
    {
        AddViolation($"🚨 KEGAGALAN SISTEM: Terlambat Mengunci / Mengabaikan Anomali di {camName}!");
        AddPerformance(-20);
        AddWrongDecision();
    }

    public void EvaluateDecision(bool accepted, PassengerData data)
    {
        string pName = !string.IsNullOrEmpty(data.passengerName) ? data.passengerName : "Penumpang Anonim";

        // =====================================
        // PENGECEKAN KHUSUS MONSTER ANOMALI
        // =====================================
        if (data.isMonster)
        {
            if (accepted)
            {
                // Katastrofi jika membiarkan Anomali masuk!
                AddPerformance(-50);
                AddHumanity(-10);
                AddWrongDecision();
                AddViolation($"🚨 KELALAIAN FATAL: Mengizinkan Entitas Monster / Anomali Masuk ke Stasiun! ({pName})");
            }
            else
            {
                // Bagus, mengusir Anomali dengan benar
                AddPerformance(+5); 
                AddCorrectDecision();
            }
            
            AddPassengerServed();
            return;
        }

        DecisionResult result;

        if (accepted)
        {
            if (data.isReasonTrue)
            {
                result = DecisionResult.Merciful;
                AddViolation($"⚠️ Meloloskan Penumpang Tanpa Prosedur Standar / Izin Khusus ({pName})");
            }
            else
            {
                result = DecisionResult.Gullible;
                AddViolation($"❌ Meloloskan Penumpang Berdokumen/Alasan Palsu ({pName})");
            }
        }
        else
        {
            if (data.isReasonTrue)
            {
                result = DecisionResult.Heartless;
                AddViolation($"❌ Menolak Penumpang Sah dengan Tiket Valid ({pName})");
            }
            else
            {
                result = DecisionResult.Correct;
            }
        }

        ApplyResult(result);
        AddPassengerServed();
    }

    void ApplyResult(DecisionResult result)
    {
        switch (result)
        {
            case DecisionResult.Correct:
                AddPerformance(+2); 
                AddCorrectDecision();
                break;

            case DecisionResult.Merciful:
                AddPerformance(-5);
                AddHumanity(+5);
                AddWrongDecision();
                break;

            case DecisionResult.Gullible:
                AddPerformance(-10);
                AddWrongDecision();
                break;

            case DecisionResult.Heartless:
                AddPerformance(+1);
                AddHumanity(-10);
                AddWrongDecision();
                break;
        }
    }

    public void AddPerformance(int amount)
    {
        Performance += amount;
        Performance = Mathf.Clamp(Performance, 0, 100);
    }

    public void AddHumanity(int amount)
    {
        Humanity += amount;
        Humanity = Mathf.Clamp(Humanity, 0, 100);
    }

    public void AddCorrectDecision()
    {
        CorrectDecisions++;
    }

    public void AddWrongDecision()
    {
        WrongDecisions++;
    }

    public void AddPassengerServed()
    {
        PassengersServed++;
    }
}