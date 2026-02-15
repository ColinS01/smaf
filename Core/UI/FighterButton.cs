using Godot;

public partial class FighterButton : Button
{
    [Export] public FighterData FighterData;

    [Signal]
    public delegate void FighterSelectedEventHandler(FighterData fighter);

    public override void _Ready()
    {
        Pressed += () =>
        {
            EmitSignal(SignalName.FighterSelected, FighterData);
        };

        if (FighterData != null)
        {
            Icon = FighterData.Portrait;
            TooltipText = FighterData.DisplayName;
        }
    }
}
