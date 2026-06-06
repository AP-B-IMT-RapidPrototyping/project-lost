using Godot;
using System;

public partial class ParrotEnemy : CharacterBody3D
{
	[Export] public PackedScene AttackEffectScene;
	[Export] public float Speed = 2.5f;
	[Export] public Node3D _player;
	[Export] public float DetectionDistance = 10.0f;
	[Export] public float AttackDistance = 3.6f;
	[Export] public AnimationPlayer _animationPlayer;
	[Export] public Timer _timer;
	[Export] public float health = 10.0f;

	private Marker3D _strikePoint;

	public override void _Ready()
	{
		_strikePoint = GetNode<Marker3D>("StrikePoint");
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

		if (GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) < AttackDistance * AttackDistance)
		{
			if (_timer.IsStopped())
			{
				GD.Print("Parrot attacked");
				_animationPlayer.PlaySection("eat", 0, 0.5);
				SpawnAttackAsset();
				_timer.Start();
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void TakeHit()
	{
		health -= 5.0f;
	}

	public void SpawnAttackAsset()
	{
		if (AttackEffectScene == null)
		{
			GD.PrintErr("AttackEffectScene not assigned in the Inspector!");
			return;
		}

		// 1. Instantiate the scene
		Node3D effect = AttackEffectScene.Instantiate<Node3D>();

		// 2. Add it to the root scene so it stays in the world space
		GetTree().Root.AddChild(effect);

		// 3. Match the Marker3D's global position and rotation
		effect.GlobalTransform = _strikePoint.GlobalTransform;

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
