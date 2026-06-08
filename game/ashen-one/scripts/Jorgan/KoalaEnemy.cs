using Godot;
using System;

public partial class KoalaEnemy : CharacterBody3D
{
	[Export] public float Speed = 3.0f;
	[Export] public Node3D _player;
	[Export] public float DetectionDistance = 10.0f;
	[Export] public float AttackDistance = 1.4f;
	[Export] public AnimationPlayer _animationPlayer;
	[Export] public Timer _timer;
	[Export] public int health = 10;

	// De fysieke attackCollision hebben we via deze methode niet eens meer nodig voor schade,
	// maar we laten hem staan als je teamgenoot hem ergens anders voor gebruikt.
	[Export] public CollisionShape3D attackCollision;

	public void TakeHit()
	{
		health -= 5;
	}

	public override void _Ready()
	{
		if (attackCollision != null) attackCollision.Disabled = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Vector3.Zero;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		Vector3 lookTarget = new Vector3(_player.GlobalPosition.X, GlobalPosition.Y, _player.GlobalPosition.Z);

		if (GlobalPosition.DistanceTo(lookTarget) > 0.1f)
		{
			LookAt(lookTarget, Vector3.Up);
		}

		// Bewegen naar de speler
		float distanceToPlayerSq = GlobalPosition.DistanceSquaredTo(_player.GlobalPosition);
		float attackDistSq = AttackDistance * AttackDistance;

		if (distanceToPlayerSq < DetectionDistance * DetectionDistance && distanceToPlayerSq > attackDistSq)
		{
			Vector3 direction = (lookTarget - GlobalPosition).Normalized();
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
			_animationPlayer.Play("run");
		}

		// Aanvallen triggeren via de timer
		if (distanceToPlayerSq <= attackDistSq)
		{
			if (_timer.IsStopped())
			{
				Attack();
			}
		}

		if (health <= 0)
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

	// De vernieuwde actieve Attack functie
	public async void Attack()
	{
		// Start de timer (cooldown) direct zodat hij niet elke frame een aanval start
		_timer.Start();

		GD.Print("Koala start animatie...");
		_animationPlayer.PlaySection("eat", 0, 0.5);

		// Optioneel: Wacht een fractie van een seconde tot de animatie op het "raak-moment" is
		await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);

		// HIER IS DE FIX: Controleer direct of de speler nog steeds binnen bereik staat
		float currentDistanceSq = GlobalPosition.DistanceSquaredTo(_player.GlobalPosition);
		if (currentDistanceSq <= AttackDistance * AttackDistance)
		{
			// Als je speler het PlayerMovement script heeft, delen we DIRECT schade uit
			if (_player.HasMethod("takehit"))
			{
				GD.Print("Koala deelt schade uit!");
				_player.Call("takehit"); // Verandert de HP van de speler met 10
			}
		}
	}
}