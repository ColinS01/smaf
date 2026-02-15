using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class CharacterSelect : Control
{
    private enum PlayerTurn
    {
        Player1,
        Player2
    }

    private PlayerTurn _currentTurn = PlayerTurn.Player1;

    // 0 = first controller, 1 = second controller
    private int _activeDeviceIndex = 0;

    private FighterData _player1Fighter;
    private FighterData _player2Fighter;

    // UI references
    [Export] private Texture2D _defaultPortrait;
    [Export] private TextureRect Player1Icon { get; set; }
    [Export] private TextureRect Player2Icon { get; set; }
    [Export] private Label StatusLabel { get; set; }
    [Export] private Button MenuButton { get; set; }
    [Export] private Button PlayButton { get; set; }

    // Grid + button prefab + data
    [Export] private GridContainer FighterGrid { get; set; }
    [Export] public PackedScene FighterButtonScene { get; set; }
    [Export] public FighterData[] Fighters { get; set; }

    private readonly List<FighterButton> _fighterButtons = new();

    public override void _Ready()
    {
        SetupInitialUI();
        BuildFighterGrid();
    }

    private void SetupInitialUI()
    {
        if (Player1Icon != null && _defaultPortrait != null)
            Player1Icon.Texture = _defaultPortrait;
        if (Player2Icon != null && _defaultPortrait != null)
            Player2Icon.Texture = _defaultPortrait;

        if (StatusLabel != null)
            StatusLabel.Text = "Player 1: Select your fighter";

        if (MenuButton != null)
            MenuButton.Pressed += OnMenuButtonPressed;

        if (PlayButton != null)
            PlayButton.Pressed += OnPlayButtonPressed;
    }

    private void BuildFighterGrid()
    {
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
            _fighterButtons.Add(button);
        }

        if (_fighterButtons.Count > 0)
            _fighterButtons[0].GrabFocus();
    }

    private void OnFighterChosen(FighterData fighter)
    {
        if (fighter == null)
            return;

        if (_currentTurn == PlayerTurn.Player1)
        {
            _player1Fighter = fighter;
            if (Player1Icon != null)
                Player1Icon.Texture = fighter.Portrait ?? _defaultPortrait;

            _currentTurn = PlayerTurn.Player2;
            _activeDeviceIndex = 1; // Now only controller 1 is allowed
            if (StatusLabel != null)
                StatusLabel.Text = "Player 2: Select your fighter";

            if (_fighterButtons.Count > 0)
                _fighterButtons[0].GrabFocus();
        }
        else
        {
            _player2Fighter = fighter;
            if (Player2Icon != null)
                Player2Icon.Texture = fighter.Portrait ?? _defaultPortrait;

            if (StatusLabel != null)
                StatusLabel.Text = "Ready to fight!";

            if (PlayButton != null)
                PlayButton.GrabFocus();

            GameStateManager.Instance.Player1Fighter = _player1Fighter;
            GameStateManager.Instance.Player2Fighter = _player2Fighter;

            _activeDeviceIndex = 0; // Now only controller 1 is allowed
        }
    }

    private void OnMenuButtonPressed()
    {
        GameStateManager.Instance.ChangeState(GameState.MainMenu);
    }

    private void OnPlayButtonPressed()
    {
        if (GameStateManager.Instance.Player1Fighter != null &&
            GameStateManager.Instance.Player2Fighter != null)
        {
            GameStateManager.Instance.ChangeState(GameState.Match);
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Block all gamepad input from non‑active devices
        if (@event is InputEventJoypadButton btn && btn.Device != _activeDeviceIndex)
        {
            GetViewport().SetInputAsHandled(); // <- instead of GetTree().SetInputAsHandled()
            return;
        }

        if (@event is InputEventJoypadMotion motion && motion.Device != _activeDeviceIndex)
        {
            GetViewport().SetInputAsHandled();
            return;
        }

        // Active controller + keyboard/mouse still go through to UI
    }

}
