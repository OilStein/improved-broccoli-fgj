using Godot;
using System;

public class GameStateManager : Node
{
    [Export]
    public PackedScene MenuScene;
    [Export]
    public PackedScene GameScene;
    [Export]
    public PackedScene EndScene;

    public override void _Ready()
    {
        ToStartMenu();
    }

    public void StartGame()
    {
        GetNode<Node>("MainMenu").QueueFree();
        var game = GameScene.Instance();
        AddChild(game);
        game.Connect("GameOver", this, "EndGame");
    }

    public void EndGame(int score)
    {
        GetNode<Node>("MainState").QueueFree();
        var endScreen = EndScene.Instance<GameOverMenu>();
        endScreen.ShowMenu(score);
        AddChild(endScreen);
        endScreen.Connect("Close", this, "BackToStartMenu");
    }

    public void BackToStartMenu()
    {
        GetNode<Node>("GameOver").QueueFree();
        ToStartMenu();
    }

    private void ToStartMenu()
    {
        var start = MenuScene.Instance();
        AddChild(start);
        start.Connect("StartGame", this, "StartGame");
    }
}
