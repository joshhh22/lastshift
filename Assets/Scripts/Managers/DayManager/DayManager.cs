using UnityEngine;

public enum GameDay
{
    Day1 = 1,
    Day2,
    Day3,
    Day4,
    Day5,
    Day6,
    Day7
}

public class DayManager : MonoBehaviour
{
    public GameDay CurrentDay { get; private set; }

    public int CurrentDayNumber
    {
        get { return (int)CurrentDay; }
    }

    private void Awake()
    {
        SetDay(GameDay.Day1);
    }

    public void SetDay(GameDay day)
    {
        CurrentDay = day;
        Debug.Log($"Current Day : {CurrentDay}");
    }

    public void NextDay()
    {
        if (CurrentDay == GameDay.Day7)
        {
            Debug.Log("Last Day Reached");
            return;
        }

        SetDay(CurrentDay + 1);
    }

    public void ResetDay()
    {
        SetDay(GameDay.Day1);
    }
}