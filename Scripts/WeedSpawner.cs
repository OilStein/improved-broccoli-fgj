using Godot;
using System;
using System.Collections.Generic;

public class WeedSpawner : Node
{
	[Export]
	public PackedScene WeedInstanceScene;

    private RandomNumberGenerator rng;

    public override void _Ready()
    {
        GD.Randomize();
        rng = new RandomNumberGenerator();
    }

    public void Start()
    {
        GetNode<Timer>("Timer").Start();
    }

    public void Stop()
    {
        GetNode<Timer>("Timer").Stop();
    }

    public void Spawn()
    {
        var spawnPoints = GetNode<Node2D>("SpawnPoints").GetChildren();
        var spawnPointNodes = new List<Node2D>();
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint is Node2D)
            {
                spawnPointNodes.Add((Node2D)spawnPoint);
            }
        }

        if (spawnPointNodes.Count == 0)
        {
            return;
        }

        int index = rng.RandiRange(0, spawnPointNodes.Count - 1);
        var spawnNode = spawnPointNodes[index];

        var weed = WeedInstanceScene.Instance<Weed>();
        AddChild(weed);
        weed.GlobalPosition = spawnNode.GlobalPosition;

        GD.Print("Spawned a weed at: " + weed.Position);
    }
}