using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	[Export] public Node3D CameraPivot;
	[Export] Camera3D Camera;
	[Export] public float MouseSensitivity = 0.002f;
	[Export] Label hpLabel;
	[Export] Label ammolabel;

	[Export] public Node3D MeleeMesh;
	[Export] public AnimationPlayer anMelee;
	[Export] public AnimationPlayer anweapon;
	[Export] public Node3D RangeMesh;
	[Export] public AnimationPlayer anRange;
	[Export] public CollisionShape3D MeleeCollisionShape;
	[Export] Marker3D _muzzle;
	[Export] RayCast3D RayCast;
	[Export] MeshInstance3D bulletmesh;
	int Ammo = 10;
	bool GunEmpty = false;
	bool GunCouldown = false;

	Node3D Player;
	bool ActivePlayer = true;
	bool PlayerSwitch = true;
	bool CanSwitch = true;
	bool canAttack = true;


	public float Speed = 4f;
	public const float JumpVelocity = 2.5f;
	bool Forced = false;
	bool canDodge = true;
	int HP = 100;
	float regentimer = 0.0f;
	float LastHitTime = 0.0f;

	bool canTakeDamage = true;
	[Export] public float DamageCooldown = 1.0f;

	public override void _Ready()
	{
		MeleeMesh.Visible = true;
		RangeMesh.Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		if (ActivePlayer == true) { Player = MeleeMesh; RangeMesh.Visible = false; }
		else { Player = RangeMesh; MeleeMesh.Visible = false; }
		RayCast.Visible = false;
		ammolabel.Visible = false;
		if (MeleeCollisionShape != null) MeleeCollisionShape.Disabled = true;
		bulletmesh.Visible = false;
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
		LastHitTime += (float)delta;

		if (LastHitTime >= 20.0f && HP < 100)
		{
			regentimer += (float)delta;
			if (regentimer >= 1.0f)
			{
				HP += 1;
				HP = Mathf.Min(HP, 100);
				hpLabel.Text = $"HP: {HP}";

				regentimer = 0.0f;
				GD.Print($"HP passief hersteld! Huidig HP: {HP}");
			}
		}
		else
		{
			regentimer = 0.0f;
		}


		if (Input.IsActionJustPressed("switch_character") && PlayerSwitch == true)
		{
			SwitchCharacter();
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

		if (Input.IsActionJustPressed("dodge") && !Forced && ActivePlayer)
		{
			PerformDodge(direction);
		}

		if (!Forced)
		{
			if (Input.IsActionJustPressed("reload") && !ActivePlayer)
			{
				reload();
			}
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

		CheckEnemyCollisions();

		if (direction != Vector3.Zero)
		{
			Vector3 globalCamRot = CameraPivot.GlobalTransform.Basis.GetEuler();

			Vector3 globalPlayerRot = Player.GlobalTransform.Basis.GetEuler();

			globalPlayerRot.Y = globalCamRot.Y;
			globalPlayerRot.X = 0;
			globalPlayerRot.Z = 0;

			Player.GlobalTransform = new Transform3D(new Basis(Quaternion.FromEuler(globalPlayerRot)), Player.GlobalPosition);
		}

		Vector3 raycastRot = RayCast.Rotation;
		raycastRot.X = CameraPivot.Rotation.X - 30;
		RayCast.Rotation = raycastRot;

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
				ammolabel.Visible = false;
			}
			else
			{
				Player = RangeMesh;
				RangeMesh.Visible = true;
				MeleeMesh.Visible = false;
				ammolabel.Visible = true;
			}

			Player.Rotation = CameraPivot.Rotation;

			GD.Print("Geswitcht naar: " + Player.Name);
			await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
			CanSwitch = true;
		}
	}

	//weapon and damage code

	async void attack()
	{
		if (ActivePlayer && canAttack)
		{
			canAttack = false;
			anMelee.Play("attack-melee-right");
			anweapon.Play("slash");
			MeleeCollisionShape.Disabled = false;
			await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);
			MeleeCollisionShape.Disabled = true;
			await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
			canAttack = true;
		}
		else if (!ActivePlayer)
		{
			Shoot(RayCast);
		}
	}

	public void Ammolabel(bool reloading)
	{
		if (reloading == true) { ammolabel.Text = "Reloading..."; }
		else { ammolabel.Text = $"{Ammo} / 10"; }
	}
	public async void reload()
	{
		CanSwitch = false;
		Ammo = 0;
		Ammolabel(true);
		await ToSignal(GetTree().CreateTimer(5f), SceneTreeTimer.SignalName.Timeout);
		Ammo = 10;
		GunEmpty = false;
		Ammolabel(false);
		CanSwitch = true;
	}

	public async void Shoot(RayCast3D raycast)
	{
		if (GunEmpty || GunCouldown) return;
		GunCouldown = true;
		raycast.ForceRaycastUpdate();
		showmesh();

		if (raycast.IsColliding())
		{
			var collider = raycast.GetCollider();


			if (collider is Node3D hitNode)
			{
				if (hitNode.IsInGroup("Enemy"))
				{
					GD.Print($"Raycast raakte direct vijand: {hitNode.Name}");
					hitNode.Call("TakeHit");
				}
				else if (hitNode.GetParent() is Node3D parentNode && parentNode.IsInGroup("Enemy"))
				{
					GD.Print($"Raycast raakte child van vijand: {hitNode.Name}, parent is {parentNode.Name}");
					parentNode.Call("TakeHit");
				}
			}

		}
		async void showmesh()
		{
			bulletmesh.Visible = true;
			await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
			bulletmesh.Visible = false;
		}

		raycast.Visible = true;
		--Ammo;
		Ammolabel(false);
		if (Ammo <= 0)
		{
			GunEmpty = true;
		}

		await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);

		raycast.Visible = false;
		GunCouldown = false;
	}


	//playerdamage
	public void takehit()
	{
		HP -= 10;
		HP = Mathf.Max(HP, 0);
		hpLabel.Text = $"HP: {HP}";
	}
	private void CheckEnemyCollisions()
	{
		if (!canTakeDamage) return;
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision = GetSlideCollision(i);
			GodotObject collider = collision.GetCollider();

			if (collider is Node3D other)
			{
				if (other.IsInGroup("Enemy"))
				{
					GD.Print($"player hit by enemy: {other}");
					TriggerDamageCooldown();
					break;
				}
			}
		}
	}

	private async void TriggerDamageCooldown()
	{
		canTakeDamage = false;

		takehit();

		await ToSignal(GetTree().CreateTimer(DamageCooldown), SceneTreeTimer.SignalName.Timeout);

		canTakeDamage = true;
		GD.Print("Speler kan weer schade oplopen!");
	}

	//playerswitchsignal
	public void OnPlayerSwitchActive()
	{
		PlayerSwitch = true;
	}
}