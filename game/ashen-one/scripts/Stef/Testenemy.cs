using Godot;
using System;

public partial class Testenemy : Node3D
{
	int HP = 100;

	// Variabelen om de spawn-status bij te houden
	private Vector3 _startPosition;
	private bool _isDead = false;

	public override void _Ready()
	{
		// Sla de beginpositie op zodat hij hier altijd naar terugkeert
		_startPosition = GlobalPosition;
	}

	public override void _Process(double delta)
	{
		// We controleren of HP 0 is én of hij niet al bezig is met respawnene
		if (HP <= 0 && !_isDead)
		{
			RespawnLogic();
		}
	}

	public void TakeDamage(int amount)
	{
		if (_isDead) return; // Geen schade incasseren als je al dood bent

		HP -= amount;
		HP = Mathf.Max(HP, 0); // Zorgt dat HP niet onder de 0 zakt
		GD.Print($"Enemy geraakt! HP is nu: {HP}");
	}

	private async void RespawnLogic()
	{
		_isDead = true;
		GD.Print("Enemy is dood, respawn start...");

		// 1. Maak de enemy onzichtbaar en verplaats hem tijdelijk ver weg (of zet collisions uit)
		Visible = false;
		GlobalPosition = new Vector3(9999, -9999, 9999); // Veilige manier om hem uit de actie te halen

		// 2. Wacht bijvoorbeeld 3 seconden (pas dit getal aan naar wens)
		await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout);

		// 3. Reset alle statistieken
		HP = 100;
		GlobalPosition = _startPosition;
		Visible = true;
		_isDead = false;

		GD.Print("Enemy is gerespawned!");
	}

	public void _on_hurtbox_body_entered(Node3D other)
	{
		if (other.IsInGroup("Playerweapon"))
		{
			TakeDamage(10);
		}
	}
}