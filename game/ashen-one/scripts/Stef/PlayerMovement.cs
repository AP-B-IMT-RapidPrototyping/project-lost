using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	[Export] public Node3D CameraPivot;
	[Export] Camera3D Camera;
	[Export] public float MouseSensitivity = 0.002f;
	[Export] Label hpLabel;

	[Export] public Node3D MeleeMesh;
	[Export] public AnimationPlayer anMelee;
	[Export] public AnimationPlayer anweapon;
	[Export] public Node3D RangeMesh;
	[Export] public AnimationPlayer anRange;

	[Export] private Marker3D _muzzle;
	[Export] public RayCast3D RayCast;
	int Ammo = 10;
	bool GunEmpty = false;
	bool GunCouldown = false;

	Node3D Player;
	bool ActivePlayer = true;
	bool PlayerSwitch = true;
	bool CanSwitch = true;


	public float Speed = 3.25f;
	public const float JumpVelocity = 2.5f;
	bool Forced = false;
	bool canDodge = true;
	int HP = 100;


	public override void _Ready()
	{
		MeleeMesh.Visible = true;
		RangeMesh.Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		if (ActivePlayer == true) { Player = MeleeMesh; RangeMesh.Visible = false; }
		else { Player = RangeMesh; MeleeMesh.Visible = false; }
		RayCast.Visible = false;
	}


	// movementcode

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			CameraPivot.RotateY(-mouseMotion.Relative.X * MouseSensitivity);

			Vector3 currentRot = CameraPivot.Rotation;
			currentRot.X -= mouseMotion.Relative.Y * MouseSensitivity;
			currentRot.X = Mathf.Clamp(currentRot.X, Mathf.DegToRad(-80), Mathf.DegToRad(80));
			CameraPivot.Rotation = currentRot;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (HP == 0)
		{
			GD.Print("Player died");
			GetTree().ChangeSceneToFile("res://scenes/Stef/death screen.tscn");
		}
		if (Input.IsActionJustPressed("switch_character") && PlayerSwitch == true)
		{
			SwitchCharacter();
			takehit();
		}

		Vector3 velocity = Velocity;
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		if (Input.IsActionJustPressed("jump") && IsOnFloor() && !Forced)
		{
			velocity.Y = JumpVelocity;
		}

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");

		Vector3 forward = CameraPivot.GlobalTransform.Basis.Z;
		Vector3 right = CameraPivot.GlobalTransform.Basis.X;
		forward.Y = 0;
		right.Y = 0;
		forward = forward.Normalized();
		right = right.Normalized();

		Vector3 direction = (forward * inputDir.Y + right * inputDir.X).Normalized();

		if (Input.IsActionJustPressed("dodge") && !Forced)
		{
			PerformDodge(direction);
		}

		if (!Forced)
		{
			if (Input.IsActionJustPressed("attack"))
			{
				attack();
			}
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
				velocity.Z = Mathf.MoveToward(velocity.Z, 0, Speed);
			}
			Velocity = velocity;
		}
		MoveAndSlide();
		Vector3 playerRot = Rotation;

		playerRot.Y = CameraPivot.Rotation.Y;
		Player.Rotation = playerRot;

	}
	private async void PerformDodge(Vector3 direction)
	{
		if (!canDodge) return;

		canDodge = false;
		Forced = true;
		SetCollisionMaskValue(2, false);

		Vector3 dodgeDirection = direction;
		if (dodgeDirection == Vector3.Zero)
		{
			dodgeDirection = -CameraPivot.GlobalTransform.Basis.Z;
			dodgeDirection.Y = 0;
			dodgeDirection = dodgeDirection.Normalized();
		}

		float dodgeSpeed = 20.0f;
		Velocity = dodgeDirection * dodgeSpeed;

		await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);
		Forced = false;

		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		SetCollisionMaskValue(2, true);
		Velocity = Vector3.Zero;

		await ToSignal(GetTree().CreateTimer(2.5f), SceneTreeTimer.SignalName.Timeout);
		canDodge = true;
		GD.Print("Dodge weer beschikbaar!");
	}
	async void ForcedMovement(Vector3 velocity)
	{
		Forced = true;
		Velocity = 5 * velocity;
		await ToSignal(GetTree().CreateTimer(5.0), SceneTreeTimer.SignalName.Timeout);
		Forced = false;
	}
	async void SwitchCharacter()
	{
		if (CanSwitch)
		{
			CanSwitch = false;
			ActivePlayer = !ActivePlayer;

			if (ActivePlayer)
			{
				Player = MeleeMesh;
				MeleeMesh.Visible = true;
				RangeMesh.Visible = false;
			}
			else
			{
				Player = RangeMesh;
				RangeMesh.Visible = true;
				MeleeMesh.Visible = false;
			}

			Player.Rotation = CameraPivot.Rotation;

			GD.Print("Geswitcht naar: " + Player.Name);
			await ToSignal(GetTree().CreateTimer(5.0), SceneTreeTimer.SignalName.Timeout);
			CanSwitch = true;
		}
	}

	//weapon and damage code

	async void attack()
	{
		if (ActivePlayer)
		{
			GD.Print("slash");
			anMelee.Play("attack-melee-right");
			anweapon.Play("slash");
			await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);
		}
		else
		{
			GD.Print("shoot");
			Shoot(RayCast);
		}
	}

	//hitmarkers
	public void takehit()
	{
		HP -= 10;
		HP = Mathf.Max(HP, 0);
		hpLabel.Text = $"HP: {HP}";
	}



	//playerswitchsignal
	public void OnPlayerSwitchActive()
	{
		PlayerSwitch = true;
	}

	public async void Shoot(RayCast3D raycast)
	{
		// 1. Check of we mogen schieten
		if (GunEmpty || GunCouldown) return;

		// 2. Start de cooldown en update de raycast
		GunCouldown = true;
		raycast.ForceRaycastUpdate();

		// 3. Verwerk de collision (Logica)
		if (raycast.IsColliding())
		{
			var collider = raycast.GetCollider();

			if (collider is Testenemy enemy)
			{
				enemy.TakeDamage(10);
			}
			else if (collider is Node3D node && node.GetParent() is Testenemy parentEnemy)
			{
				parentEnemy.TakeDamage(10);
			}
		}

		// 4. Visuele effecten en Ammo
		raycast.Visible = true; // Toont de debug lijn (als Visible Collision Shapes aan staat)
		Ammo--;
		if (Ammo <= 0)
		{
			GunEmpty = true;
		}

		// 5. Wacht op de cooldown/flash tijd
		await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);

		// 6. Reset voor het volgende schot
		raycast.Visible = false;
		GunCouldown = false;
	}
}