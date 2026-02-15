using Godot;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        GD.Print("[MainMenu] Ready");

        GetNode<Button>("CenterContainer/VBoxContainer/Start")
            .Pressed += OnStartPressed;

        GetNode<Button>("CenterContainer/VBoxContainer/Quit")
            .Pressed += OnQuitPressed;

        GetNode<Button>("CenterContainer/VBoxContainer/Start").GrabFocus();
    }

    private void OnStartPressed()
    {
        GD.Print("[MainMenu] Start pressed");
        GameStateManager.Instance.ChangeState(GameState.CharacterSelect);
    }

    private void OnQuitPressed()
    {
        GD.Print("[MainMenu] Quit pressed");
        GetTree().Quit();
    }
}
