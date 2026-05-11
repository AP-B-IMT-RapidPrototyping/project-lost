using Godot;

public partial class Portal : Area3D
{
    [Export(PropertyHint.File, "*.tscn,*.scn")] 
    public string TargetScenePath;
    [Export] public string SpawnName;

    private void _on_body_entered(Node body)
    {
        GD.Print("Someone went in");

        if (body is PlayerMovement)
        {
            GD.Print("Teleporting");

            GameManager.NextSpawn = SpawnName;

			GD.Print(TargetScenePath);

            var sceneToLoad = GD.Load<PackedScene>(TargetScenePath);
            GetTree().ChangeSceneToPacked(sceneToLoad);       
		}
    }
}