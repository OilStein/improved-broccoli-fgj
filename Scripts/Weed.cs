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
	public float StretchScaleModifier = 0.01f;
	public float MinDragThreshold = 0.01f; // Drag length below this is ignored
	
	private static readonly string mouseClickAction = "mouse_left_click";

	private bool dragging = false;

	private Vector2 relativeClickPosition;
	private Vector2 stretchPosition;

	private Node2D startPosition => GetNode<Node2D>("StartPosition");
	private Node2D sprite => GetNode<Node2D>("SpriteRoot");

	public override void _Process(float delta)
	{
		if (!dragging)
		{
			return;
		}

		if (Input.IsActionJustReleased(mouseClickAction))
		{
			Release();
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

		var sprite = this.sprite;
		sprite.Rotation = dragDirection.Angle();
		sprite.Scale = new Vector2(1 + stretchLength * StretchScaleModifier, 1);

		if (stretchPosition.y >= DownwardDragLimit)
		{
			Release();
			return;
		}

		if (dragLength >= BreakPointDistance)
		{
			EmitSignal(nameof(Killed));
			QueueFree();
		}
	}

	public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
	{
		if (@event.IsActionPressed(mouseClickAction))
		{
			Grab();
		}
	}

	private void Grab()
	{
		dragging = true;
		relativeClickPosition = GetLocalMousePosition();
		stretchPosition = Vector2.Zero;
	}

	private void Release()
	{
		dragging = false;
		var startPos = startPosition;
		var sprite = this.sprite;
		sprite.Position = startPos.Position;
		sprite.Rotation = startPos.Rotation;
		sprite.Scale = startPos.Scale;
	}
}
