using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Runtime.CompilerServices;

public static class StatisticsManager
{
    public static int numberOfTravels { get; private set; } = 0;
    public static List<int> usersPerFrame = new();

    public static void Reset()
    {
        numberOfTravels = 0;
        usersPerFrame.Clear();
    }

    public static void IncrNumberOfTravels() { numberOfTravels++; }

    public static void registerFrameUsers(int users) { usersPerFrame.Add(users); }

    public static Texture2D GetFrameUsersGraph(int width, int height, Color color)
    {
        int maxUsers = GetMaxRegisteredNumberOfUsers();

        GD.Print("Generating " + width + " x " + height);

        Image img = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        img.Fill(new(0.0f, 0.0f, 0.0f, 0.0f));
        for(int x = 0; x < width; ++x)
        {
            int firstIndex = Mathf.FloorToInt(1.0f * x / width * usersPerFrame.Count);
            int secondIndex = firstIndex + 1;

            while(secondIndex >= usersPerFrame.Count)
            {
                firstIndex--;
                secondIndex--;
            }

            float currentPointValue = (usersPerFrame[firstIndex] + usersPerFrame[secondIndex]) * 0.5f;
            int pointHeight = Mathf.FloorToInt(height * currentPointValue / maxUsers);

            pointHeight = Mathf.Max(1, pointHeight);

            img.FillRect(new(x, height - pointHeight, 1, pointHeight), color);
        }

        return ImageTexture.CreateFromImage(img);
    }

    private static int GetMaxRegisteredNumberOfUsers()
    {
        int max = 0;
        usersPerFrame.ForEach((x) => { if(x > max) { max = x; } });
        return max;
    }
}
