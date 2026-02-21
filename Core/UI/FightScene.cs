using Godot;

public partial class FightScene : Control
{
    // Drag your generic BaseFighter scene here in the editor.
    [Export] public PackedScene FighterScene { get; set; }

    private Marker2D _p1Spawn;
    private Marker2D _p2Spawn;

    public override void _Ready()
    {
        _p1Spawn = GetNode<Marker2D>("P1Spawn");
        _p2Spawn = GetNode<Marker2D>("P2Spawn");

        var p1Data = GameStateManager.Instance.Player1Fighter;
        var p2Data = GameStateManager.Instance.Player2Fighter;

        if (p1Data == null || p2Data == null)
        {
            GD.PushError("FightScene: Missing FighterData for one or both players.");
            return;
        }

        SpawnFighters(p1Data, p2Data);
    }

    private void SpawnFighters(FighterData p1Data, FighterData p2Data)
    {
        if (FighterScene == null)
        {
            GD.PushError("FightScene: FighterScene is not assigned.");
            return;
        }

        // Player 1
        var p1 = FighterScene.Instantiate<BaseFighter>(); // typed instantiate [web:91][web:94]
        p1.ControllerIdx = 0; // first controller
        p1.Position = _p1Spawn.GlobalPosition;
        p1.Facing = 1;
        p1.Data = p1Data;     // <-- tell the fighter which config to use
        AddChild(p1);

        // Player 2
        var p2 = FighterScene.Instantiate<BaseFighter>();
        p2.ControllerIdx = 1; // second controller
        p2.Position = _p2Spawn.GlobalPosition;
        p2.Facing = -1;       // face left
        p2.Data = p2Data;
        AddChild(p2);
    }
}
