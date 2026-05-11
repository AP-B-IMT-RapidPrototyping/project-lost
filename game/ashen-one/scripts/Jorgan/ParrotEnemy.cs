using Godot;
using System;

public partial class ParrotEnemy : CharacterBody3D
{
	[Export] public float Speed = 3.0f;
	[Export] public Node3D _player;
	[Export] public float DetectionDistance = 10.0f;
	[Export] public float AttackDistance = 3.6f;
	[Export] public AnimationPlayer _animationPlayer;
	[Export] public Timer _timer;
	[Export] public float health = 10.0f;

	public void TakeHit()
	{
		health -= 5.0f;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Vector3.Zero;

		_animationPlayer.Play("run");

		Vector3 lookTarget = new Vector3(_player.GlobalPosition.X, _player.GlobalPosition.Y + 3, _player.GlobalPosition.Z);

		// Laat de vijand naar de speler kijken
		if (GlobalPosition.DistanceTo(lookTarget) > 0.1f) 
		{
			LookAt(lookTarget, Vector3.Up);
		}

		if (GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) < DetectionDistance * DetectionDistance && GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) > AttackDistance + 0.1f * AttackDistance + 0.1f)
		{
			// Bereken de richting-vector
			Vector3 direction = (lookTarget - GlobalPosition).Normalized();

			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
			velocity.Y = direction.Y * Speed;
		}

		if(GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) < AttackDistance * AttackDistance)
		{
			if (_timer.IsStopped())
			{
				GD.Print("Parrot attacked");
				_timer.Start();
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
