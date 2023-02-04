using Godot;
using System;

public class CameraMovement : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private Viewport viewport;
	
	private Camera2D camera;
	private Vector2 yLocation = new Vector2();
	
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Error here
		viewport.SetSizeOverride(true, Vector2(720, 2680));
		viewport.SetSizeOverrideStretch(true);
		
		camera = GetNode<Camera2D>("Camera2D");
		var cPosition = camera.GetCameraPosition();
		GD.Print(cPosition);
	}
	
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		// Impl. state boolean day
		if (Input.IsActionPressed("ui_down"))
		{
			yLocation.y += 10;
			camera.SetGlobalPosition(yLocation);
		}
		
		// Impl. state boolean night
		if (Input.IsActionPressed("ui_up"))
		{
			yLocation.y -= 10;
			camera.SetGlobalPosition(yLocation);
		}
		
 	}
}
