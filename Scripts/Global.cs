using Godot;
using System;

namespace Beatroot
{
    /// <summary>
    /// Global holds data needed to be carried over scenes.
    /// This script is autoloaded and can be accessed with GetNode<Global>("/root/Global")
    /// </summary>
    public class Global : Node
    {
        public int Score { get; private set; }

        [Signal] public delegate void ScoreChanged(int score);

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

            EmitSignal("ScoreChanged", Score);
        }
    }
}