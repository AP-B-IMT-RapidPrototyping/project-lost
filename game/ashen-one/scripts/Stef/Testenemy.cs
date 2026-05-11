using Godot;
using System;
using System.Net.Http;

public partial class Testenemy : Node3D
{
	int HP = 100;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (HP == 0)
		{
			QueueFree();
		}
	}
	public void _on_hurtbox_body_entered(Node3D other)
	{
		if (other.IsInGroup("Playerweapon"))
		{
			GD.Print("enemy hit by player");
			HP -= 10;
		}
	}
}
