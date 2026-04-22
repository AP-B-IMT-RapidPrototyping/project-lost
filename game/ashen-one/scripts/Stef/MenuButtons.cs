using Godot;
using System;

public partial class MenuButtons : Control
{
	[Export] public Button NewGame;
	[Export] PackedScene World;
	public override void _Ready()
	{
		NewGame.Pressed += OnNewGamePressed;
	}
	public void OnNewGamePressed()
	{
		GD.Print("scene veranderd");
		GetTree().ChangeSceneToPacked(World);
	}
}