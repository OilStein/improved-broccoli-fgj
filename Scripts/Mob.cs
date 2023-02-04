using Godot;
using System;

public class Mob : Area2D
{
	private bool moving = true;
	private float crawlSpeed = 5.0f;
	private Vector2 targetPosition;
	
	public bool Moving {
		get => moving;
		set => moving = value;
	}
	
	public float CrawlSpeed {
		get => crawlSpeed;
		set => crawlSpeed = value;
	}
	
	public Vector2 TargetPosition {
		get => targetPosition;
		set => targetPosition = value;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}
	
	// Called when there's an input event on this CollisionObject2D
	private void OnInputEvent(object viewport, object @event, int shape_idx)
	{
		// Kill
	}

	// Called when another Area2D enters this area
	private void OnAreaEntered(object area)
	{
		//if (area is BeetRoot) {
		//	disable moving
		//  start damaging
		//}
	}
	
	public void StopMoving()
	{
		moving = false;
	}
	
	// Called on physics update.
	public override void _PhysicsProcess(float delta)
	{
		if (moving) {
			// Move towards the target
			Vector2 toTarget = TargetPosition - Position;
			Vector2 moveDelta = toTarget.LimitLength(CrawlSpeed * delta);
			Position += moveDelta;
		}
	}
	
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
