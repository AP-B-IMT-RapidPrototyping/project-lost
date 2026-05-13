using Godot;

public partial class Portal : Area3D
{
    [Export(PropertyHint.File, "*.tscn,*.scn")] 
    public string TargetScenePath;
    [Export] public string SpawnName;

    [Export] public bool StartAsMelee = true;

    [Export] public bool UnlockNextLevel = false;

       private bool used = false;

    private void _on_body_entered(Node body)
    {
        GD.Print("Someone went in");

        if (body is PlayerMovement)
        {
            PlayerMovement.OnPlayerSwitchActive();

            used = true;

            GameManager.NextSpawn = SpawnName;

            GameManager.StartAsMelee = StartAsMelee;

            if (TargetScenePath.Contains("Spawn"))
{
                GameManager.CurrentLevel++;

}               GameManager.CurrentLevel =
                    Mathf.Clamp(GameManager.CurrentLevel, 1, 4);

                GD.Print("Nivel actual: " + GameManager.CurrentLevel);

			GD.Print(TargetScenePath);

            var sceneToLoad = GD.Load<PackedScene>(TargetScenePath);
            GetTree().ChangeSceneToPacked(sceneToLoad);       
		}

        if (UnlockNextLevel)
        {
            GameManager.CurrentLevel++;

            GD.Print("NIVEL ACTUAL: " + GameManager.CurrentLevel);
        }   
    }



    
}