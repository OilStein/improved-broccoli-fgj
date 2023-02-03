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
    
    private static readonly string mouseClickAction = "mouse_left_click";

    private bool dragging = false;

    public override void _Process(float delta)
    {
        if (!dragging)
        {
            return;
        }

        var sprite = GetNode<Sprite>("Sprite");

        if (Input.IsActionJustReleased(mouseClickAction))
        {
            dragging = false;
            sprite.Position = Vector2.Zero;
            return;
        }

        var relativeMousePosition = GetLocalMousePosition();
        var dragDirection = relativeMousePosition.Normalized();
        var dragLength = relativeMousePosition.Length();

        var stretchLength = Mathf.Min(dragLength, StretchDistance);
        sprite.Position = dragDirection * stretchLength;

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
            dragging = true;
        }
    }
}
