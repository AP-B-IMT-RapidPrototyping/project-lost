using Godot;
using System;

public partial class Testspeler : CharacterBody3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	public void _on_area_3d_area_entered()
	{
		GD.Print("object in hitbox");
	}
}
