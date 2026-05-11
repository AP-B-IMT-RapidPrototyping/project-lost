using Godot;

public partial class SpawnManager : Node3D
{
    [Export] public Node3D PortalLevel1;
    [Export] public Node3D PortalLevel2;
    [Export] public Node3D PortalLevel3;
    [Export] public Node3D PortalLevel4;

    public override void _Ready()
    {
        // ocultar todos
        PortalLevel1.Visible = false;
        PortalLevel2.Visible = false;
        PortalLevel3.Visible = false;
        PortalLevel4.Visible = false;

        // mostrar solo el nivel actual
        switch (GameManager.CurrentLevel)
        {
            case 1:
                PortalLevel1.Visible = true;
                break;

            case 2:
                PortalLevel2.Visible = true;
                break;

            case 3:
                PortalLevel3.Visible = true;
                break;

            case 4:
                PortalLevel4.Visible = true;
                break;
        }
    }
}