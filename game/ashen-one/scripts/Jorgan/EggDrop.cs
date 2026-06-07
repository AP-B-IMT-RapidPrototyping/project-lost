using Godot;
using System;

public partial class EggDrop : CharacterBody3D
{
	[Export] public CollisionShape3D MeleeCollisionshape;
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
}


