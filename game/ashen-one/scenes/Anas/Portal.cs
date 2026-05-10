using Godot;
using System;

public partial class Portal : Node3D
{
    [Export] public string TargetScene;

    private bool used = false;

    private void _on_area_3d_body_entered(Node body)
    {
        if (used) return;

        if (body is PlayerMovement)
        {
            used = true;
            GD.Print("Teleporting...");
            GetTree().ChangeSceneToFile(TargetScene);
        }
    }
}