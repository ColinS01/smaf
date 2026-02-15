using Godot;

public partial class FightScene : Node2D
{
	// Drag your Fighter.tscn into this export slot in the editor.[web:25][web:107]
	[Export] public PackedScene FighterScene { get; set; }

	private Marker2D _p1Spawn;
	private Marker2D _p2Spawn;

	public override void _Ready()
	{
		_p1Spawn = GetNode<Marker2D>("P1Spawn");
		_p2Spawn = GetNode<Marker2D>("P2Spawn");

		SpawnFighters();
	}

	private void SpawnFighters()
	{
		// Player 1
		var p1 = FighterScene.Instantiate<BaseFighter>();
		p1.ControllerIdx = 0; // first controller
		p1.Position = _p1Spawn.GlobalPosition;
		AddChild(p1);

		// Player 2
		var p2 = FighterScene.Instantiate<BaseFighter>();
		p2.ControllerIdx = 1; // second controller
		p2.Position = _p2Spawn.GlobalPosition;
		// Face left if you want them to face each other
		p2.Facing = -1;
		AddChild(p2);
	}
}
