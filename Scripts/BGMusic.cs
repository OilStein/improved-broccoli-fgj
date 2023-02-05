using Godot;
using System;

public class BGMusic : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	private AudioStreamPlayer day;
	private AudioStreamPlayer night;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		day = GetNode<AudioStreamPlayer>("Day");
		GD.Print("Day soundtrack");

		night = GetNode<AudioStreamPlayer>("Night");
		GD.Print("BG audio script loaded");

		var mainState = GetParent<MainState>();
		mainState.Connect("DayNightCycle", this, "PlayTrack");

		PlayTrack(true);
	}

	public void PlayTrack(bool mode)
	{
		if (mode.Equals(true))
		{	
			GD.Print("Playing day soundtrack");
			night.Stop();
			day.Play();
		}
		else
		{

			GD.Print("Playing night soundtrack");
			day.Stop();
			night.Play();
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
// public override void _Process(float delta)
//  {     
//
//	}
}
