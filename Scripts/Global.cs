using Godot;
using System;

public class Global : Node
{
    public int Score { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Init();
    }

    public void Init()
    {
        Score = 0;
    }

    public void AddScore(int score)
    {
        Score += score;
    }
}
