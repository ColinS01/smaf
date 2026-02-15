using Godot;

public partial class Fighter : CharacterBody2D
{
    public int DeviceId { get; set; } = 0;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
    }
}