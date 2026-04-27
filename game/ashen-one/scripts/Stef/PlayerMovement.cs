using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	[Export] public Node3D CameraPivot;
	[Export] Camera3D Camera;
	[Export] public float MouseSensitivity = 0.002f;

	[Export] public Node3D MeleeMesh;
	[Export] public Node3D RangeMesh;
	Node3D Player;
	bool ActivePlayer = false;
	bool PlayerSwitch = true;


	public float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	bool Forced = false;
	bool canDodge = true;


	public override void _Ready()
	{
		MeleeMesh.Visible = true;
		RangeMesh.Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		if (ActivePlayer == true) { Player = MeleeMesh; RangeMesh.Visible = false; }
		else { Player = RangeMesh; MeleeMesh.Visible = false; }
		GD.Print(Player);
	}
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
		if (Input.IsActionJustPressed("switch_character"))
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

		if (Input.IsActionJustPressed("dodge") && !Forced)
		{
			PerformDodge(direction);
		}

		if (!Forced)
		{
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
		if (!canDodge) return; // Stop als de cooldown nog loopt

		GD.Print("Dodge start");
		canDodge = false; // Start cooldown
		Forced = true;
		Player.Visible = false;

		// 1. Richting bepalen: als direction Zero is, pak de 'vooruit' kant van de camera
		Vector3 dodgeDirection = direction;
		GD.Print(direction);
		if (dodgeDirection == Vector3.Zero)
		{
			dodgeDirection = -CameraPivot.GlobalTransform.Basis.Z;
			dodgeDirection.Y = 0;
			dodgeDirection = dodgeDirection.Normalized();
		}

		// 2. Snelheid instellen
		float dodgeSpeed = 50.0f;
		Velocity = dodgeDirection * dodgeSpeed;

		// 3. Wacht 0.5 seconde voor de beweging/zichtbaarheid
		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		Forced = false;

		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		Player.Visible = true;
		Velocity = Vector3.Zero; // Stop de snelle beweging
		GD.Print("Dodge beweging klaar, cooldown loopt nog...");

		// 4. Cooldown timer: Wacht nog 2.5 seconden (totaal 3 seconden sinds begin)
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
	void SwitchCharacter()
	{
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
	}
}