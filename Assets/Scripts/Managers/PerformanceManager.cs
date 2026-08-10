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
    }

    public void EvaluateDecision(bool accepted, PassengerData data)
    {
        // =====================================
        // PENGECEKAN KHUSUS MONSTER ANOMALI
        // =====================================
        if (data.isMonster)
        {
            if (accepted)
            {
                // Katastrofi jika membiarkan Anomali masuk!
                AddPerformance(-50);
                AddHumanity(-10); // Menyelundupkan monster membahayakan penghuni kereta
                AddWrongDecision();
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
                result = DecisionResult.Merciful;
            else
                result = DecisionResult.Gullible;
        }
        else
        {
            if (data.isReasonTrue)
                result = DecisionResult.Heartless;
            else
                result = DecisionResult.Correct;
        }

        ApplyResult(result);

        AddPassengerServed();
    }

    void ApplyResult(DecisionResult result)
    {
        switch (result)
        {
            case DecisionResult.Correct:
                // Kerja bagus = performance naik tipis. Bekerja benar itu sebuah kewajiban.
                AddPerformance(+2); 
                AddCorrectDecision();
                break;

            case DecisionResult.Merciful:
                // Kasihan pada penumpang = Melanggar aturan. Humanity naik, Performance turun.
                AddPerformance(-5);
                AddHumanity(+5);
                AddWrongDecision();
                break;

            case DecisionResult.Gullible:
                // Ketipu penumpang = Performance anjlok.
                AddPerformance(-10);
                AddWrongDecision();
                break;

            case DecisionResult.Heartless:
                // Menolak penumpang yang jujur dengan kasar = Humanity turun.
                // Tapi secara teknis kamu ngikutin alat (tiket salah wajar ditolak).
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