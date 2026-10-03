using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class EndGameScreenManager : Control
{
    [Export] private RichTextLabel statsText;
    [Export] private RichTextLabel explanationText;
    [Export] private TextureRect graph;
    [Export] private Color graphColor;
    [Export] private ProgressionDisplayModule progressionModule;
    private GameManager gameManager;

    public enum EndScreenStat { duration, userCount, trips };

    private string durationStat = "";
    private string userCountStat = "";
    private string tripsStat = "";

    public void RegisterManager(GameManager manager) { gameManager = manager; }

    public void OnRestartButtonPressed() { gameManager.OnRestartGameButtonClick(); }

    public void SetStat(EndScreenStat stat, string text)
    {
        switch(stat)
        {
            case EndScreenStat.duration: durationStat = text; break;
            case EndScreenStat.userCount: userCountStat = text; break;
            case EndScreenStat.trips: tripsStat = text; break;
        }

        UpdateText();
    }

    public void GenerateEndGameGraph(int width, int height)
    {
        graph.Texture = StatisticsManager.GetFrameUsersGraph(width, height, graphColor);
    }

    public void StartProgressionAnimation(float daysTarget)
    {
        progressionModule.AnimateTo(daysTarget);
    }

    public void UpdateLossExplanation()
    {
        /*
        User x waited X HOURS to get home and needs to leave without any rest
        User x waited X HOURS to go to work and missed their entire planning
        */
        UserSchedule schedule = StatisticsManager.userLostSchedule;
        explanationText.Text = "M.Bean" + StatisticsManager.userLostID + " waited ";
        bool userMissedResting = schedule.ShouldLeave();
        if(userMissedResting)
        {
            int waitedHours = schedule.leaveHour - schedule.backHour;
            if(waitedHours < 0)
                waitedHours += 24;
            explanationText.Text += waitedHours + " HOURS to get home and needs to leave without any rest.";
        }
        else
        {
            int waitedHours = schedule.backHour - schedule.leaveHour;
            if(waitedHours < 0)
                waitedHours += 24;
            explanationText.Text += waitedHours + " HOURS to go outside and missed their entire planning.";
        }
    }

    private void UpdateText()
    {
        statsText.Text = durationStat + "\n" + userCountStat + "\n" + tripsStat;
    }
}
