using Godot;
using System;

public partial class EggDrop : CharacterBody3D
{
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		GroundDetection();

		Velocity = velocity;
		MoveAndSlide();
	}

	public void GroundDetection()
	{
		if (IsOnFloor())
		{
			QueueFree();
		}
	}

	public void _on_hurtbox_body_entered(Node3D other)
	{
		if (other.IsInGroup("Player"))
		{
			QueueFree();
		}
	}
}


