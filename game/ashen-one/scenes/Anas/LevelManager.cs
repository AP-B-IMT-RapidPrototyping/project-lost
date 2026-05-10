using Godot;

public partial class LevelManager : Node3D
{
    public override void _Ready()
    {
        var player = GetNode<Node3D>("Player");

        var spawnPoints = GetNode<Node3D>("SpawnPoints");

        foreach (Node child in spawnPoints.GetChildren())
        {
            if (child.Name == GameManager.NextSpawn)
            {
                player.GlobalPosition = ((Node3D)child).GlobalPosition;
            }
        }
    }
}