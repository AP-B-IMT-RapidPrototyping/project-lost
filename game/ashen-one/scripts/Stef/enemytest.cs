using Godot;
using System;

public partial class enemytest : Area3D
{
	public void _on_body_entered(Node3D body)
	{
		GD.Print("body entered");
		if (body is PlayerMovement p)
		{
			GD.Print("De speler loopt door de hitbox!");
			p.takehit();
		}
	}
}
