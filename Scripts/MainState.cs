using Godot;
using System;

public class MainState : Node
{
	[Signal]
	public delegate void GameOver(int score);

	[Signal]
	public delegate void DayNightCycle(bool isDay);

#pragma warning disable 649
	// We assign this in the editor, so we don't need the warning about not being assigned.
	[Export]
	public PackedScene mobScene;
#pragma warning restore 649
	
	private Vector2 nightCameraPosition = new Vector2(0, 768);
	private Vector2 dayCameraPosition = new Vector2(0, 0);
	
	private bool isNight = false;
	private bool isTransitioning = false;
	private bool isSpawningMobs = false;
	// 1 during day, 0 during night, interpolated while transitioning.
	private float transitioningRatio = 1.0f;

	private int score = 0;

	private int round = 0;
	
	public bool IsNight
	{
		get => isNight;
	}
	
	public bool IsTransitioning
	{
		get => isTransitioning;
	}

	public void OnBeetrootHealthChange(int health, int delta)
	{
		if (health <= 0)
		{
			EmitSignal(nameof(GameOver), score);
		}
	}
	
	private void SetBeetrootFace(bool nighty)
	{
		var beetroot  = GetNode<Node2D>("Beetroot");
		var face = beetroot.GetNode<AnimatedSprite>("SpriteFace");
		face.Frame = (nighty ? 1 : 0);
	}
	
	private void SetSkyBackground(bool nighty)
	{
		var sky = GetNode<AnimatedSprite>("SpriteSky");
		sky.Frame = (nighty ? 1 : 0);
	}
	
	private void StartMobSpawning()
	{
		isSpawningMobs = true;
		GetNode<Timer>("MobSpawnerTimer").Start();
		round++;
	}
	
	private void StopMobSpawning()
	{
		isSpawningMobs = false;
		// Also stop mobs from moving
		GetTree().CallGroup("mobs", "StopMoving");
		GetNode<Timer>("MobSpawnerTimer").Stop();
	}
	
	private void ClearOutMobs()
	{
		// Note that for calling Godot-provided methods with strings,
		// we have to use the original Godot snake_case name.
		GetTree().CallGroup("mobs", "Fadeout");
		GD.Print("Fading out all mobs");
	}
	
	private void StartTransitionToNight()
	{
		isNight = true;
		isTransitioning = true;
		// Update sky
		SetSkyBackground(isNight);
		EmitSignal("DayNightCycle", !isNight);
		// Update the camera target
		GetNode<Node2D>("CameraTarget").Position = nightCameraPosition;
		// Start the transition timer which does the state switching
		GetNode<Timer>("TransitionToNightTimer").Start();
    }
	
	private void StartTransitionToDay() 
	{
		isNight = false;
		isTransitioning = true;
		// Update sky
		SetSkyBackground(isNight);
        StopMobSpawning();
        EmitSignal("DayNightCycle", !isNight);
        // Update the camera target
        GetNode<Node2D>("CameraTarget").Position = dayCameraPosition;
		// Start the transition timer which does the state switching
		GetNode<Timer>("TransitionToDayTimer").Start();
	}
	
	// Called when the transition to night is complete
	private void OnTransitionToNightTimerTimeout()
	{
		isTransitioning = false;
		// Update the Beetroot face to night mode
		SetBeetrootFace(true);
		// Start mob spawning and stop weed spawning
		StartMobSpawning();
		GetNode<WeedSpawner>("WeedSpawner").Stop();
		// Restart the cycle timer
		GetNode<Timer>("DayNightCycleTimer").Start();
	}
	
	// Called a when the transition to day is complete
	private void OnTransitionToDayTimerTimeout()
	{
		isTransitioning = false;
		// Update the Beetroot face to day mode
		SetBeetrootFace(false);
		// Mobs are no longer needed, remove them. Start weed spawning.
		ClearOutMobs();
		GetNode<WeedSpawner>("WeedSpawner").Start();
		// Restart the cycle timer
		GetNode<Timer>("DayNightCycleTimer").Start();
	}
	
	private void SwitchDayNight()
	{
		// Start transitioning from night to day or vice versa.
		if (!isTransitioning) {
			if (isNight) {
				StartTransitionToDay();
			} else {
				StartTransitionToNight();
			}
		}
	}
	
	private void OnDayNightCycleTimerTimeout()
	{
		SwitchDayNight();
	}
	
	// Called when a mob should spawn
	private void OnMobSpawnerTimerTimeout()
	{
		// Get a random position on the lower half of the mob spawn circle
		var spawnCircle = GetNode<Position2D>("MobSpawnCircle");
		float angle = (float)GD.RandRange(0, Mathf.Pi);
		var mobSpawnPosition = spawnCircle.Position + new Vector2(
			Mathf.Cos(angle) * spawnCircle.GizmoExtents,
			Mathf.Sin(angle) * spawnCircle.GizmoExtents
		);
		var beetroot = GetNode<Node2D>("Beetroot");
		// Create a new mob
		var mob = (Mob)mobScene.Instance();
		AddChild(mob);
		ConnectMobEatingSignal(mob);
		mob.Connect("Killed", this, "AddScore");
		mob.CrawlSpeed = 50.0f + (25f * (float)round);
		mob.Position = mobSpawnPosition;
		mob.TargetPosition = beetroot.Position;
		GD.Print("Created new mob with speed: " + mob.CrawlSpeed);
	}

	private void OnDebugHUDSwitchDayNightPressed()
	{
		SwitchDayNight();
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Beatroot.Beetroot>("Beetroot").Connect("HealthChanged", this, "OnBeetrootHealthChange");
		GetNode<WeedSpawner>("WeedSpawner").Start();
	}
	
	private void UpdateDebugHUD()
	{
		var timer = GetNode<Timer>("DayNightCycleTimer");
		float remaining = 0.0f;
		if (!timer.IsStopped()) {
			remaining = timer.TimeLeft;
		}
		var hud = GetNode<DebugHUD>("DebugHUD");
		hud.UpdateTime(remaining);
		hud.UpdateTransition(transitioningRatio);
	}
	
	private void UpdateTransitioning()
	{
		if (isTransitioning) {
			Timer timer = null;
			if (isNight) {
				timer = GetNode<Timer>("TransitionToNightTimer");
				transitioningRatio = timer.TimeLeft / timer.WaitTime;
			} else {
				timer = GetNode<Timer>("TransitionToDayTimer");
				transitioningRatio = 1.0f - timer.TimeLeft / timer.WaitTime;
			}
		} else {
			transitioningRatio = (isNight ? 0.0f : 1.0f);
		}
		
		// Hide the ground sprite during night
		var spriteGround = GetNode<Sprite>("SpriteGround");
		// Set sprite alpha
		var color = spriteGround.Modulate;
		color = new Color(color.r, color.g, color.b, transitioningRatio);
		spriteGround.Modulate = color;
	}

	private void ConnectMobEatingSignal(Mob mob)
	{
		var beetroot = GetNode<Beatroot.Beetroot>("Beetroot");
		mob.Connect("EatingBeetroot", beetroot, "TakeDamage");
		GD.Print("Connect eating signal");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		UpdateDebugHUD();
		UpdateTransitioning();
	}

	public void AddScore()
	{
		score++;
	}
}
