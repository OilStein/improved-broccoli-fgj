using Godot;
using System;

public class Mob : Area2D
{
	private bool isMoving = true;
	private bool isEating = false;
	private bool isSplat = false;
	private bool isFading = false;
	
	private float crawlSpeed = 5.0f;
	private float rotateSpeed = 0.6f;
	private Vector2 targetPosition;
	private Vector2 currentDirection = new Vector2(0, -1);
	private float targetDesiredAngleOffset = 0.0f;
	
	private float minDesiredAngleOffsetDegrees = 15.0f;
	private float maxDesiredAngleOffsetDegrees = 30.0f;
	private float minDirectionChangeInterval = 2.0f;
	private float maxDirectionChangeInterval = 6.0f;
	
	private float fadeRatio = 0.0f;
	
	public bool IsMoving {
		get => isMoving;
		set => isMoving = value;
	}
	
	public bool IsEating {
		get => isEating;
	}
	
	public bool IsSplat {
		get => isSplat;
	}
	
	public bool IsFading {
		get => isFading;
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
		var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
		int index = 1 + (int)(GD.Randi() % 3);
		sprite.Animation = "Bug" + index.ToString();
		StartAnimation();
	}
	
	// Called when there's an input event on this CollisionObject2D
	private void OnInputEvent(object viewport, object @event, int shape_idx)
	{
		if (!isSplat && @event is InputEventMouseButton) {
			Splat();
		}
	}

	// Called when another Area2D enters this area
	private void OnAreaEntered(object area)
	{
		if (area is Node n) {
			if (n.Name == "Beetroot") {
				// Colliding with the beetroot
				StopMoving();
				StartEating();
			}
		}
	}
	
	// Squish the mob
	public void Splat()
	{
		StopMoving();
		StopEating();
		StopAnimation();
		// TODO: Increment resources / points
		// TODO: Emit particles
		// TODO: Play splat sound
		isSplat = true;
		GetNode<Timer>("SplatTimer").Start();
	}
	
	// Fade the mob away
	public void Fadeout()
	{
		StopMoving();
		StopEating();
		StopAnimation();
		isFading = true;
		GetNode<Timer>("FadeoutTimer").Start();	
	}
	
	// Mob stops moving
	public void StopMoving()
	{
		isMoving = false;
	}
	
	// Mob starts eating the Beetroot
	public void StartEating()
	{
		isEating = true;
		// Start the eating timer
		GetNode<Timer>("EatingTimer").Start();
	}
	
	// Mob stops eating the Beetroot
	public void StopEating()
	{
		isEating = false;
		GetNode<Timer>("EatingTimer").Stop();
	}
	
	private void StartAnimation()
	{
		GetNode<AnimatedSprite>("AnimatedSprite").Playing = true;
	}
	
	// Stops the mob's current animation from playing
	private void StopAnimation()
	{
		GetNode<AnimatedSprite>("AnimatedSprite").Playing = false;
	}
	
	// Called periodically when the mob wants to change direction
	private void OnDirectionChangeTimerTimeout()
	{
		// Choose a new direction offset
		float offset = (float)GD.RandRange(minDesiredAngleOffsetDegrees, maxDesiredAngleOffsetDegrees);
		int offsetSign = (int)(GD.Randi() % 2) * 2 - 1;
		offset *= offsetSign;
		targetDesiredAngleOffset = Mathf.Deg2Rad(offset); // Mathf.Deg2Rad((float)GD.RandRange(-maxDesiredAngleOffsetDegrees, maxDesiredAngleOffsetDegrees));
		// Reset this timer with a random interval
		GetNode<Timer>("DirectionChangeTimer").Start((float)GD.RandRange(minDirectionChangeInterval, maxDirectionChangeInterval));
	}

	// Called periodically when the mob is eating the Beetroot
	private void OnEatingTimerTimeout()
	{
		// TODO: Damage the Beetroot.
		// TODO: Play hit sound
	}

	// Called some time after a splat
	private void OnSplatTimerTimeout()
	{
		GetNode<Timer>("FadeoutTimer").Start();
	}
	
	// Called when the mob has faded out
	private void OnFadeoutTimerTimeout()
	{
		QueueFree();
	}
	
	// Set the mob direction
	private void SetDirection(Vector2 direction) {
		currentDirection = direction;
		// Update the collision shape
		// Maybe this needs to be deferred?
		GetNode<CollisionShape2D>("CollisionShape2D").Rotation = direction.AngleTo(Vector2.Up);
	}

	// Called on physics update.
	public override void _PhysicsProcess(float delta)
	{
		if (isMoving) {
			// Delta to target
			Vector2 toTarget = TargetPosition - Position;
			// Angle between current direction and direction to target
			float angleToTarget = currentDirection.AngleTo(toTarget);
			// Angle difference from desired
			float angleDelta = targetDesiredAngleOffset - angleToTarget;
			// Decrease the angle delta
			angleDelta *= Mathf.Exp(-rotateSpeed * delta);
			// Rotate current direction towards desired
			SetDirection(currentDirection.Rotated(-angleDelta));
			// Move towards the desired direction
			Vector2 moveDelta = currentDirection.LimitLength(CrawlSpeed * delta);
			//Vector2 moveDelta = toTarget.LimitLength(CrawlSpeed * delta);
			Position += moveDelta;
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (isFading) {
			var timer = GetNode<Timer>("FadeoutTimer");
			fadeRatio = timer.TimeLeft / timer.WaitTime;
			// Set sprite alpha
			var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
			var color = sprite.Modulate;
			color = new Color(color.r, color.g, color.b, fadeRatio);
			sprite.Modulate = color;
		}
	}
}
