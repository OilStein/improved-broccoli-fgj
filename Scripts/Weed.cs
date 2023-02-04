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
	public float MinDragThreshold = 0.01f; // Drag length below this is ignored
	
	private static readonly string mouseClickAction = "mouse_left_click";

	private bool dragging = false;

	private Vector2 relativeClickPosition;
	private Vector2 stretchPosition;

	private float targetRotation;
	private Vector2 targetScale;

	private Node2D startPosition => GetNode<Node2D>("StartPosition");
	private Node2D sprite => GetNode<Node2D>("SpriteRoot");

	public override void _Ready()
	{
		Release();
	}

	public override void _Process(float delta)
	{
		ProcessDragging();
		ProcessSpritePosition();
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
		targetRotation = startPos.Rotation;
		targetScale = startPos.Scale;
	}

	private void ProcessDragging()
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
			EmitSignal(nameof(Killed));
			QueueFree();
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
