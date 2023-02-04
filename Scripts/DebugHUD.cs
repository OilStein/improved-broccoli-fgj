using Godot;
using System;

public class DebugHUD : CanvasLayer
{
	[Signal]
	public delegate void SwitchDayNightPressed();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}
	
	private void OnButtonSwitchDayNightPressed()
	{
		EmitSignal(nameof(SwitchDayNightPressed));
	}
	
	public void UpdateTime(float remaining)
	{
		var label = GetNode<Label>("LabelTime");
		label.Text = $"Time: {remaining}";
	}
	
	public void UpdateTransition(float transitioning)
	{
		var label = GetNode<Label>("LabelTransition");
		label.Text = $"Transition: {100.0f * transitioning}";
	}
	
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}



