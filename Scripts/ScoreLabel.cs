using Godot;
using System;

namespace Beatroot
{
    public class ScoreLabel : Node2D
    {
        private Label _scoreLabel;

        public override void _Ready()
        {
            _scoreLabel = GetChild<Label>(0);
            _scoreLabel.Text = "Score: 0";

            var global = GetNode<Global>("/root/Global");
            global.Connect("ScoreChanged", this, "UpdateLabel");
        }

        public void UpdateLabel(int score)
        {
            _scoreLabel.Text = $"Score: {score}";
        }
    }
}