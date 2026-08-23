using Godot;
using System;

public partial class EndGameScreenManager : Control
{
    [Export] private RichTextLabel statsText;
    [Export] private TextureRect graph;
    [Export] private Color graphColor;
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

    private void UpdateText()
    {
        statsText.Text = durationStat + "\n" + userCountStat + "\n" + tripsStat;
    }
}
