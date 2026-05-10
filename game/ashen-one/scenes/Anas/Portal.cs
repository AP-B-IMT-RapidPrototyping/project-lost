using Godot;

public partial class Portal : Area3D
{
	[Export] public PackedScene TargetScene;

	[Export] public string SpawnName;

	private bool used = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body)
	{
		if (used) return;

		if (body is PlayerMovement)
		{
			used = true;

			// guardar nombre del spawn
			GameManager.NextSpawn = SpawnName;

			// cambiar escena
			GetTree().ChangeSceneToPacked(TargetScene);
		}
	}
}
