using Godot;
using System;

public partial class ElephantEnemy : CharacterBody3D
{
	[Export] public float Speed = 3.0f;
	[Export] public Node3D _player;
	[Export] public float DetectionDistance = 10.0f;
	[Export] public float AttackDistance = 1.4f;
	[Export] public AnimationPlayer _animationPlayer;
	[Export] public Timer _timer;
	[Export] public int health = 25;

	public void TakeHit()
	{
		health -= 5;
	}

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

		if (GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) < DetectionDistance * DetectionDistance && GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) > AttackDistance * AttackDistance)
		{
			// Bereken de richting-vector
			Vector3 direction = (lookTarget - GlobalPosition).Normalized();

			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
			_animationPlayer.Play("run");
		}

		if (GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) < AttackDistance * AttackDistance)
		{
			if (_timer.IsStopped())
			{
				_animationPlayer.PlaySection("eat", 0, 0.5);
				_timer.Start();
				GD.Print("Elephant attacked");
			}
		}

		if (health == 0)
		{
			QueueFree();
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void _on_hurtbox_body_entered(Node3D other)
	{
		if (other.IsInGroup("Playerweapon"))
		{
				GD.Print("enemy hit by player");
				TakeHit();
		}
	}
}
