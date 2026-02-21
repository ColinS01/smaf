using Godot;

[GlobalClass]
public partial class FighterData : Resource
{
    [Export] public string Id { get; set; }
    [Export] public string DisplayName { get; set; }
    [Export] public Texture2D Portrait { get; set; }

    // Scene / animation setup
    [Export] public PackedScene FighterScene { get; set; } // optional: a prefab override
    [Export] public SpriteFrames SpriteFrames { get; set; } // if using AnimatedSprite2D [web:16][web:15]
    [Export] public AnimationLibrary AnimLibrary { get; set; } // if you centralize animations [web:49][web:53]
    [Export] public Vector2 CollisionSize { get; set; }
    [Export] public Vector2 CollisionOffset {get; set; }

    // Core movement stats
    [Export] public float WalkSpeed { get; set; } = 100f;
    [Export] public float RunSpeedMultiplier { get; set; } = 1.5f;
    [Export] public float MaxAccel { get; set; } = 1000f;
    [Export] public float FloorFriction { get; set; } = 2500f;

    // Vertical
    [Export] public float Gravity { get; set; } = 2000f;
    [Export] public float JumpVelocity { get; set; } = -800f;
    [Export] public float MaxFallSpeed { get; set; } = 2500f;

    // Combat‑style flags
    // lets fix this to add a flag to all moves because some moves
    // for a character may be ranged and other may not be
    [Export] public bool IsRanged { get; set; } = false;

    public FighterData() {}
}
