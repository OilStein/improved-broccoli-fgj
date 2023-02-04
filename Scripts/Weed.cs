using Godot;
using System;

public class Weed : Area2D
{
	[Signal]
	public delegate void Killed();

	[Export]
	public float StretchDistance = 20.0f;
	[Export]
	public float BreakPointDistance = 40.0f;
	[Export]
	public float DownwardDragLimit = 5;
	[Export]
	public Vector2 StretchScaleModifier = new Vector2(0.01f, -0.01f);

	[Export]
	public string TouchInputNodePath = "/root/MainState/TouchInput";

	public float MinDragThreshold = 0.01f; // Drag length below this is ignored
	
	private static readonly string mouseClickAction = "mouse_left_click";

	private bool dragging = false;

	private Vector2 touchPosition;
	private Vector2 relativeClickPosition;

	private Vector2 stretchPosition;

	private float targetRotation;
	private Vector2 targetScale;

	private Node2D startPosition => GetNode<Node2D>("StartPosition");
	private Node2D sprite => GetNode<Node2D>("SpriteRoot");
	private CPUParticles2D particles => GetNode<CPUParticles2D>("DeathEffect");

	public override void _Ready()
	{
		var touchInput = GetNode<Area2D>(TouchInputNodePath);
		if (touchInput != null)
		{
			touchInput.Connect("input_event", this, "OnInputEvent");
		}
		Release();
	}

	public override void _Process(float delta)
	{
		ProcessDragging();
		ProcessSpritePosition();
	}

	public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
	{
		OnInputEvent(viewport, @event, shapeIdx, true);
	}

	// Receives input events from both this node and parent TouchInput node
	// This input node may not receive the touch release event, so that needs to be
	// received from parent node
	public void OnInputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
	{
		OnInputEvent(viewport, @event, shapeIdx, false);
	}
	public void OnInputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx, bool allowGrab)
	{
		if (@event is InputEventScreenTouch)
		{
			var touchEvent = @event as InputEventScreenTouch;
			if (touchEvent.Pressed && allowGrab)
			{
				Grab(touchEvent.Position - Position);
			}
			else if (!touchEvent.Pressed)
			{
				Release();
			}
		}
		else if (@event is InputEventScreenDrag)
		{
			var dragEvent = @event as InputEventScreenDrag;
			touchPosition = dragEvent.Position - Position;
			GD.Print("Drag " + touchPosition);
		}
	}

	private void Grab(Vector2 position)
	{
		GD.Print("Grab " + position);
		dragging = true;
		relativeClickPosition = position;
		touchPosition = Vector2.Zero;
		stretchPosition = Vector2.Zero;
	}

	private void Release()
	{
		GD.Print("Release");
		dragging = false;
		var startPos = startPosition;
		targetRotation = startPos.Rotation;
		targetScale = startPos.Scale;
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

	private void ProcessDragging()
	{
		if (!dragging)
		{
			return;
		}

		var relativeMousePosition = GetLocalMousePosition();
		var dragDirection = relativeMousePosition.Normalized();
		var dragLength = relativeMousePosition.Length();

		if (dragLength < MinDragThreshold)
		{
			return;
		}

		var stretchLength = Mathf.Min(dragLength, StretchDistance);
		stretchPosition = dragDirection * stretchLength;

		targetRotation = dragDirection.Angle();
		targetScale = new Vector2(
			1 + stretchLength * StretchScaleModifier.x,
			1 + stretchLength * StretchScaleModifier.y
		);

		if (stretchPosition.y >= DownwardDragLimit)
		{
			Release();
			return;
		}

		if (dragLength >= BreakPointDistance)
		{
			Die();
		}
	}

	private void ProcessSpritePosition()
	{
		var sprite = this.sprite;
		sprite.Rotation = Mathf.LerpAngle(sprite.Rotation, targetRotation, 1f / 4f);
		sprite.Scale = new Vector2(
			Mathf.Lerp(sprite.Scale.x, targetScale.x, 1f / 4f),
			Mathf.Lerp(sprite.Scale.y, targetScale.y, 1f / 4f)
		);
	}
}
