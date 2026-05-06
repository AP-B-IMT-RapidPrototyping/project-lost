using Godot;
using System;

public partial class DeathScreen : Control
{
	[Export] Button restart;
	[Export] Button quit;
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;

		restart.Pressed += OnRestartPressed;
		quit.Pressed += OnQuitPressed;
	}

	void OnRestartPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Anas/Spawn.tscn");
	}
	void OnQuitPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Stef/DeathScreen.tscn");
	}
}
