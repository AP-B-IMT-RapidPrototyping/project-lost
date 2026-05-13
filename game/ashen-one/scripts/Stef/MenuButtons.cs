using Godot;
using System;

public partial class MenuButtons : Control
{
	[Export] public Button NewGame;
	[Export] public Button Quit;
	[Export] PackedScene World;
	public override void _Ready()
	{
		NewGame.Pressed += OnNewGamePressed;
		Quit.Pressed += OnQuitPressed;
	}
	public void OnNewGamePressed()
	{
		GD.Print("scene veranderd");
		GetTree().ChangeSceneToPacked(World);
	}
	public void OnQuitPressed()
	{
		GetTree().Quit();
	}
}