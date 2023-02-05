using Godot;
using System;

public class Mob : Area2D
{
	[Signal]
	public delegate void EatingBeetroot();

    [Signal]
    public delegate void Killed();

    [Export]
	public float[] ColliderRadiuses;
	[Export]
	public float[] ColliderHeights;

	[Export]
	public AudioStreamMP3[] BugClip;

	//[Export]
	//public AudioStreamMP3[] SlapClip;

	[Export]
	public AudioStreamMP3[] SquishClip;

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
		var collision = GetNode<CollisionShape2D>("CollisionShape2D");
		var shape = collision.Shape as CapsuleShape2D;
		shape.Radius = ColliderRadiuses[index - 1];
		shape.Height = ColliderHeights[index - 1];
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
			if (n.Name == "BeetrootEatingZone") {
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
		GetNode<AnimationPlayer>("SplashAnimator").Play("Splash");
		// TODO: Increment resources / points
		EmitSignal("Killed");
		// TODO: Emit particles
		SplatSound();
		isSplat = true;
		GetNode<Timer>("SplatTimer").Start();
	}

	private void SplatSound()
	{
        var squishSound = GetNode<AudioStreamPlayer2D>("Squish");
		squishSound.VolumeDb = 15;
        squishSound.Stream = SquishClip[GetRand(SquishClip.Length)];
        squishSound.Play();

		if (new Random().Next(0, 100) > 60)
		{
            var bugSound = GetNode<AudioStreamPlayer2D>("Bug");
			bugSound.VolumeDb = 5;
            bugSound.Stream = BugClip[GetRand(BugClip.Length)];
            bugSound.Play();
        }
		//var slapSound = GetNode<AudioStreamPlayer2D>("Slap");
		//slapSound.Stream = SlapClip[GetRand(SlapClip.Length)];
		//slapSound.Play();
	}
	
	private int GetRand(int l){
		return new Random().Next(0, l);
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
		GD.Print("Send eating signal");
		EmitSignal(nameof(EatingBeetroot));
		// TODO: Play hit sound
	}

	// Called some time after a splat
	private void OnSplatTimerTimeout()
	{
		GetNode<CPUParticles2D>("BloodParticle").Emitting = true;
		Input.VibrateHandheld(100);
		GetNode<Timer>("FadeoutTimer").Start();
	}
	
	// Called when the mob has faded out
	private void OnFadeoutTimerTimeout()
	{
		isFading = false;
		QueueFree();
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
			// BUG: Rotating the collision shape here results in a crash
			// when the objects are freed.
			currentDirection = currentDirection.Rotated(-angleDelta);
			// Move towards the desired direction
			Vector2 moveDelta = currentDirection.Normalized() * CrawlSpeed * delta;
			//Vector2 moveDelta = toTarget.LimitLength(CrawlSpeed * delta);
			Position += moveDelta;
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}
}
