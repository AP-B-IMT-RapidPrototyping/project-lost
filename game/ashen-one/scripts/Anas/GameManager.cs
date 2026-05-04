using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static Dictionary<string, bool> PortalUnlocked = new Dictionary<string, bool>()
    {
        {"Level1", false},
        {"Level2", false},
        {"Level3", false},
        {"Level4", false}
    };
}