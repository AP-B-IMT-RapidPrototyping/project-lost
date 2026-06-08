using Godot;
using System;

public partial class EggDrop : CharacterBody3D
{
	[Export] public CollisionShape3D MeleeCollisionshape;
	[Export] public int DamageValue = 10; // Hoeveel schade het ei doet bij een voltreffer

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Voeg zwaartekracht toe
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		Velocity = velocity;

		// 1. Voer de beweging uit
		MoveAndSlide();

		// 2. Check DIRECT na de beweging of we iets hebben geraakt tijdens het vallen
		CheckProjectileCollisions();

		// 3. Check of we de normale grond hebben geraakt
		GroundDetection();
	}

	private void CheckProjectileCollisions()
	{
		// Loop door alle botsingen van deze frame
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision = GetSlideCollision(i);
			GodotObject collider = collision.GetCollider();

			// Check of het object dat we raken een Node3D is
			if (collider is Node3D hitNode)
			{
				// Check of dit de speler is (of dat hij de 'takehit' methode heeft)
				if (hitNode.HasMethod("takehit"))
				{
					GD.Print($"EggDrop heeft de speler ({hitNode.Name}) geraakt!");

					// Deel direct schade uit aan de speler
					hitNode.Call("takehit");

					// Vernietig het ei meteen na de impact zodat het niet door de speler heen blijft vallen
					QueueFree();
					return;
				}
			}
		}
	}

	public void GroundDetection()
	{
		// Als het ei de grond raakt (en niet de speler), verdwijnt het alsnog netjes
		if (IsOnFloor())
		{
			GD.Print("EggDrop raakte de grond en is kapot gevallen.");
			QueueFree();
		}
	}
}

