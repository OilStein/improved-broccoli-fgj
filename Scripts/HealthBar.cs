using Godot;
using System;

namespace Beatroot
{
	public class HealthBar : Node
	{
		[Export]
		public float MaxHealth = 100;

		private ProgressBar _bar;

		public override void _Ready()
		{
			_bar = GetNode<ProgressBar>("ProgressBar");
			_bar.MaxValue = MaxHealth;
		}

		public void SetHealth(int health)
		{
			_bar.Value = health;
		}
		public void SetHealth(int health, int delta)
		{
			SetHealth(health);
		}
	}
}
