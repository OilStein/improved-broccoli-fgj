using Godot;
using System;

public class MainState : Node
{
#pragma warning disable 649
	// We assign this in the editor, so we don't need the warning about not being assigned.
	[Export]
	public PackedScene mobScene;
#pragma warning restore 649
	
	private Vector2 nightCameraPosition = new Vector2(0, 300);
	private Vector2 dayCameraPosition = new Vector2(0, 0);
	
	private bool isNight = false;
	private bool isTransitioning = false;
	private bool isSpawningMobs = false;
	
	public bool IsNight
	{
		get => isNight;
	}
	
	public bool IsTransitioning
	{
		get => isTransitioning;
	}
	
	private void StartMobSpawning()
	{
		isSpawningMobs = true;
		GetNode<Timer>("MobSpawnerTimer").Start();
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
		// TODO: Kill all mobs
		// Note that for calling Godot-provided methods with strings,
		// we have to use the original Godot snake_case name.
		GetTree().CallGroup("mobs", "queue_free");
		GD.Print("Cleared out mobs");
	}
	
	private void StartTransitionToNight()
	{
		isNight = true;
		isTransitioning = true;
		// Update the camera target
		GetNode<Node2D>("CameraTarget").Position = nightCameraPosition;
		// Start the transition timer which does the state switching
		GetNode<Timer>("TransitionToNightTimer").Start();
	}
	
	private void StartTransitionToDay() 
	{
		isNight = false;
		isTransitioning = true;
		StopMobSpawning();
		// Update the camera target
		GetNode<Node2D>("CameraTarget").Position = dayCameraPosition;
		// Start the transition timer which does the state switching
		GetNode<Timer>("TransitionToDayTimer").Start();
	}
	
	private void OnTransitionToNightTimerTimeout()
	{
		isTransitioning = false;
		StartMobSpawning();
		// Restart the cycle timer
		GetNode<Timer>("DayNightCycleTimer").Start();
	}
	
	private void OnTransitionToDayTimerTimeout()
	{
		isTransitioning = false;
		// Mobs are no longer needed, remove them
		ClearOutMobs();
		// Restart the cycle timer
		GetNode<Timer>("DayNightCycleTimer").Start();
	}
	
	private void OnDayNightCycleTimerTimeout()
	{
		// Start transitioning from night to day or vice versa.
		if (isNight) {
			StartTransitionToDay();
		} else {
			StartTransitionToNight();
		}
	}
	
	private void OnMobSpawnerTimerTimeout()
	{
		// TODO: Spawn in randomized locations and give random pathing
		var beetroot = GetNode<Node2D>("Beetroot");
		// Create a new mob
		var mobSpawnLocation = beetroot.Position + new Vector2(0, 400);
		var mob = (Mob)mobScene.Instance();
		AddChild(mob);
		mob.Position = beetroot.Position + new Vector2(0, 400);
		mob.TargetPosition = beetroot.Position;
		GD.Print("Created new mob");
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}
}



