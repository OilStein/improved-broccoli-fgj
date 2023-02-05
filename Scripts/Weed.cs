using Godot;
using System;

public class Weed : Area2D
{
	[Signal]
	public delegate void Killed();

	private Sprite sprite => GetNode<Sprite>("SpriteRoot/Sprite");
	private CPUParticles2D particles => GetNode<CPUParticles2D>("DeathEffect");

	public override void _Ready()
	{
		GetNode<AnimationPlayer>("SpriteAnimator").Play("WeedSpawn", 0);
	}

	public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
	{
		if (@event is InputEventScreenTouch)
		{
			var touchEvent = @event as InputEventScreenTouch;
			if (touchEvent.Pressed)
			{
				Die();
			}
		}
	}

	public void OnDeathAnimationTimeout()
	{
		sprite.Hide();
		particles.Emitting = true;
		Input.VibrateHandheld(100);
		var deathTimer = GetNode<Timer>("DeathTimer");
		deathTimer.Connect("timeout", this, "OnDeathParticleTimeout");
		deathTimer.Start();
	}

	public void OnDeathParticleTimeout()
	{
		QueueFree();
	}

	private void Die()
	{
		EmitSignal(nameof(Killed));

		GetNode<AnimationPlayer>("SpriteAnimator").Play("WeedDeath");
		var deathAnimTimer = GetNode<Timer>("DeathAnimationTimer");
		deathAnimTimer.Connect("timeout", this, "OnDeathAnimationTimeout");
		deathAnimTimer.Start();
	}
}
