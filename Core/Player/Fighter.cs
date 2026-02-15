using Godot;

public partial class Fighter : CharacterBody2D
{
    public int DeviceId { get; set; } = 0; // set from outside when spawning

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventJoypadButton btn && btn.Device == DeviceId)
        {
            if (btn.Pressed && btn.ButtonIndex == JoyButton.A)
            {
                
            }
                
        }

        if (@event is InputEventJoypadMotion motion && motion.Device == DeviceId)
        {
            if (motion.Axis == JoyAxis.LeftX)
            {
                
            }
        }
    }
}
