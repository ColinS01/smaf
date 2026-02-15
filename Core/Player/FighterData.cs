using Godot;

[GlobalClass]
public partial class FighterData : Resource
{
    [Export]
    public string Id { get; set; }
    [Export]
    public string DisplayName { get; set; }
    [Export]
    public Texture2D Portrait { get; set; }

    public FighterData(){}
}
