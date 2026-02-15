using System.Xml.Serialization;
using Godot;

public partial class CharacterSelect : Control
{
    private enum PlayerTurn
    {
        Player1,
        Player2
    }

    private PlayerTurn _currentTurn = PlayerTurn.Player1;

    private FighterData _player1Fighter;
    private FighterData _player2Fighter;

    // UI references
    [Export] private Texture2D _defaultPortrait;        // optional: placeholder image
    [Export] private TextureRect Player1Icon { get; set; }
    [Export] private TextureRect Player2Icon { get; set; }
    [Export] private Label StatusLabel { get; set; }
    [Export] private Button MenuButton { get; set; }

    [Export] private Button PlayButton { get; set; }

    // Grid + button prefab + data
    [Export] private GridContainer FighterGrid { get; set; }
    [Export] public PackedScene FighterButtonScene { get; set; }
    [Export] public FighterData[] Fighters { get; set; }

    public override void _Ready()
    {
        // Initial UI state
        if (Player1Icon != null && _defaultPortrait != null)
            Player1Icon.Texture = _defaultPortrait;
        if (Player2Icon != null && _defaultPortrait != null)
            Player2Icon.Texture = _defaultPortrait;

        if (StatusLabel != null)
            StatusLabel.Text = "Player 1: Select your fighter";

        if (MenuButton != null)
        {
            MenuButton.Pressed += OnMenuButtonPressed;
        }
        if (PlayButton != null)
        {
            PlayButton.Pressed += OnPlayButtonPressed;
        }

        // Build grid: one button per fighter
        if (FighterGrid == null || FighterButtonScene == null || Fighters == null)
        {
            GD.PushError("CharacterSelect is missing FighterGrid, FighterButtonScene, or Fighters.");
            return;
        }

        foreach (var fighter in Fighters)
        {
            if (fighter == null)
                continue;

            var button = FighterButtonScene.Instantiate<FighterButton>();
            button.FighterData = fighter;
            button.FighterSelected += OnFighterChosen;

            FighterGrid.AddChild(button);
        }


        
    }

    private void OnFighterChosen(FighterData fighter)
    {
        if (fighter == null)
            return;

        if (_currentTurn == PlayerTurn.Player1)
        {
            _player1Fighter = fighter;
            Player1Icon.Texture = fighter.Portrait ?? _defaultPortrait;
            StatusLabel.Text = "Player 2: Select your fighter";
            _currentTurn = PlayerTurn.Player2;
        }
        else
        {
            _player2Fighter = fighter;
            Player2Icon.Texture = fighter.Portrait ?? _defaultPortrait;
            StatusLabel.Text = "Ready to fight!";

            // Save into GameStateManager
            GameStateManager.Instance.Player1Fighter = _player1Fighter;
            GameStateManager.Instance.Player2Fighter = _player2Fighter;

        }
    }

    private void OnMenuButtonPressed()
    {
        GameStateManager.Instance.ChangeState(GameState.MainMenu);
    }

    private void OnPlayButtonPressed()
    {
        if (GameStateManager.Instance.Player1Fighter != null && GameStateManager.Instance.Player2Fighter != null)
        {
             GameStateManager.Instance.ChangeState(GameState.Match);
        }
    }

}
