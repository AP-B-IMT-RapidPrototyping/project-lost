using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class KoalaEnemy : CharacterBody3D
{
	[Export] public float Speed = 3.0f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public Node3D _player;
	[Export] public float DetectionDistance = 10.0f;
	[Export] public AnimationPlayer _animationPlayer;


	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Vector3.Zero;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// 2. Bereken de richting op het horizontale vlak (X en Z)
		// We negeren de Y om te voorkomen dat de vijand de grond in kijkt/beweegt
		Vector3 lookTarget = new Vector3(_player.GlobalPosition.X, GlobalPosition.Y, _player.GlobalPosition.Z);

		// Laat de vijand naar de speler kijken
		if (GlobalPosition.DistanceTo(lookTarget) > 0.1f) 
		{
			LookAt(lookTarget, Vector3.Up);
		}

		if (GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) < DetectionDistance * DetectionDistance)
		{
			// Bereken de richting-vector
			Vector3 direction = (lookTarget - GlobalPosition).Normalized();

			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
			_animationPlayer.Play("run");
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
