using Godot;

public partial class Portal : Area3D
{
    [Export]
    public Area3D ConnectPortal;
	
	private void OnArea3DBodyEntered(Node3D body)
{
    if (body.Name == "player")
    {
        var destination = ConnectPortal.GlobalTransform.Origin;

        var t = body.GlobalTransform;
        t.Origin = destination;
        body.GlobalTransform = t;
    }
}
}

