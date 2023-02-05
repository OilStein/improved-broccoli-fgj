using Godot;
using System;

public class MainMenu : Node
{
    [Signal]
    public delegate void StartGame();

    public void StartGamePressed()
    {
        EmitSignal(nameof(StartGame));
    }
}
