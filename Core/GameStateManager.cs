using Godot;

public partial class GameStateManager : Node
{
    public static GameStateManager Instance { get; private set; }

    private Node _activeScene;
    public GameState CurrentState { get; private set; }

    public FighterData Player1Fighter { get; set; }
    public FighterData Player2Fighter { get; set; }
    public override void _Ready()
    {
        Instance = this;
        
        GD.Print("[GameStateManager] Ready");
        ChangeState(GameState.MainMenu);
    }


    public void ChangeState(GameState newState)
    {
        GD.Print($"[GameState] {CurrentState} → {newState}");

        if (_activeScene != null)
        {
            _activeScene.QueueFree();
            _activeScene = null;
        }

        CurrentState = newState;

        var scene = LoadSceneForState(newState);
        _activeScene = scene.Instantiate();
        
        GetTree().Root.CallDeferred(Node.MethodName.AddChild, _activeScene);

    }

    private PackedScene LoadSceneForState(GameState state)
    {
        return state switch
        {
            GameState.MainMenu =>
                GD.Load<PackedScene>("res://Scenes/Menus/MainMenu.tscn"),

            GameState.CharacterSelect =>
                GD.Load<PackedScene>("res://Scenes/Menus/CharacterSelect.tscn"),

            GameState.Match =>
                GD.Load<PackedScene>("res://Scenes/Fight/Match.tscn"),

            _ => throw new System.Exception($"Unhandled state: {state}")
        };
    }
}
