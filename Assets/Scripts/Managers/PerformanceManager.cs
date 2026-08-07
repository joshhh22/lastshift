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

            AddPerformance(+5);
            AddCorrectDecision();

            break;

        case DecisionResult.Merciful:

            AddPerformance(-2);
            AddHumanity(+3);

            AddWrongDecision();

            break;

        case DecisionResult.Gullible:

            AddPerformance(-8);

            AddWrongDecision();

            break;

        case DecisionResult.Heartless:

            AddPerformance(+2);
            AddHumanity(-5);

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