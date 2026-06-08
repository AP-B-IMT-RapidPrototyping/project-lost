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

		// Bewegen naar de speler toe
		float distanceToPlayerSq = GlobalPosition.DistanceSquaredTo(_player.GlobalPosition);

		if (distanceToPlayerSq < DetectionDistance * DetectionDistance && distanceToPlayerSq > AttackDistance * AttackDistance)
		{
			Vector3 direction = (lookTarget - GlobalPosition).Normalized();

			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
			velocity.Y = direction.Y * Speed;
		}

		// Aanvallen
		if (distanceToPlayerSq < AttackDistance * AttackDistance)
		{
			if (_timer.IsStopped())
			{
				GD.Print("Parrot attacked");
				_animationPlayer.PlaySection("eat", 0, 0.5);
				SpawnAttackAsset();
				_timer.Start();
			}
		}

		// FIX: Controleer of de vogel dood is en verwijder hem uit de wereld!
		if (health <= 0.0f)
		{
			GD.Print("Parrot is verslagen!");
			QueueFree();
			return; // Stop de process hier direct
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void TakeHit()
	{
		health -= 5.0f;
		GD.Print($"Parrot hit! Resterende HP: {health}");
	}

	public void SpawnAttackAsset()
	{
		if (AttackEffectScene == null)
		{
			GD.PrintErr("AttackEffectScene not assigned in the Inspector!");
			return;
		}

		Node3D effect = AttackEffectScene.Instantiate<Node3D>();
		GetTree().Root.AddChild(effect);
		effect.GlobalTransform = _strikePoint.GlobalTransform;
	}

	public void _on_hurtbox_body_entered(Node3D other)
	{
		if (other.IsInGroup("Playerweapon"))
		{
			GD.Print("enemy hit by player melee");
			TakeHit();
		}
	}
}
