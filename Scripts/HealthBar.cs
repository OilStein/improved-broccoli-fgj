using Godot;
using System;

namespace Beatroot
{
    public class HealthBar : Node
    {
        private ProgressBar _bar;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _bar = GetChild<ProgressBar>(0);
            if (GetParent<BaseCharacter>() != null && GetParent<BaseCharacter>().Get("MaxHealth") != null)
            {
                _bar.MaxValue = (double)GetParent<BaseCharacter>().Get("MaxHealth");
            }
        }

        public override void _Process(float delta)
        {
            Update();
        }

        /// <summary>
        /// Updates the health bar according to the parent Node<BaseCharacter>
        /// </summary>
        public void Update()
        {
            if (GetParent<BaseCharacter>() != null && GetParent<BaseCharacter>().Get("CurrentHealth") != null)
            {
                _bar.Value = (double)GetParent<BaseCharacter>().Get("CurrentHealth");
            }
        }
    }
}