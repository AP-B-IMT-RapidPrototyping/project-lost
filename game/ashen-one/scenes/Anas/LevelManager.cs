using Godot;

public partial class LevelManager : Node3D
{
    [Export] public bool AllowCharacterSwitch = true;

    [Export] public bool StartAsMelee = true;
    

    public override void _Ready()
    {
        var player = GetNode<Node3D>("PlayerMovement");

        var spawnPoints = GetNode<Node3D>("SpawnPoints");

            if (GameManager.StartAsMelee)
            {
                player.GetNode<Node3D>("MeleeMesh").Visible = true;
                player.GetNode<Node3D>("RangeMesh").Visible = false;
            }
            else
            {
                player.GetNode<Node3D>("MeleeMesh").Visible = false;
                player.GetNode<Node3D>("RangeMesh").Visible = true;
            }   

        // configurar personaje permitido


        // mover jugador al spawn correcto
        foreach (Node child in spawnPoints.GetChildren())
        {
            if (child.Name == GameManager.NextSpawn)
            {
                player.GlobalPosition =
                    ((Node3D)child).GlobalPosition;
            }
        }
    }
}