using Godot;
using System;

public class GameOverMenu : Node
{
    [Signal]
    public delegate void Close();

    public void ShowMenu(int score)
    {
        GetNode<Label>("UI/Score").Text = score.ToString();
    }

    public void CloseMenu() => EmitSignal(nameof(Close));
}
