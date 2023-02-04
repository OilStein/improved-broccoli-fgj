using Godot;
using System;

public class Weed : Area2D
{
	[Signal]
	public delegate void Killed();

	private CPUParticles2D particles => GetNode<CPUParticles2D>("DeathEffect");

	public override void _Ready()
	{
	}

	public override void _Process(float delta)
	{
		
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

	private void Die()
	{
		var particles = this.particles;
		var globalPos = particles.GlobalPosition;
		RemoveChild(particles);
		GetParent().AddChild(particles);
		particles.GlobalPosition = globalPos;
		particles.Emitting = true;

		EmitSignal(nameof(Killed));
		QueueFree();
	}
}
