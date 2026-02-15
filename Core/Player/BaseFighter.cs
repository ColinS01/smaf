using Godot;

public partial class BaseFighter : CharacterBody2D
{
    public enum FighterState { Idle, Walk, Jump, Attack, Hitstun }

    public FighterState State { get; protected set; } = FighterState.Idle;
    public int StateFrame { get; protected set; } = 0;

    // Controller device ID for this fighter (0, 1, 2, ...).
    // Set this when you spawn the fighter.
    [Export] public int ControllerIdx { get; set; } = 0;

    // Horizontal traits
    [Export] public float WalkSpeed = 100f;
    [Export] public float RunSpeedMultiplier = 1.5f;
    [Export] public float MaxAccel = 1000f;
    [Export] public float FloorFriction = 2500f;

    // Vertical traits
    [Export] public float Gravity = 2000f;
    [Export] public float JumpVelocity = -800f;
    [Export] public float MaxFallSpeed = 2500f;

    // General traits
    public float DamageTaken = 0.0f;
    public int Facing = 1;

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        StateFrame++;

        switch (State)
        {
            case FighterState.Idle:
            case FighterState.Walk:
                StepGroundNeutral(dt);
                break;
            case FighterState.Jump:
                StepJump(dt);
                break;
        }

        ApplyGravity(dt);
        ApplyHorizontalFriction(dt);

        MoveAndSlide();
    }

    // ------------ INPUT FROM SPECIFIC CONTROLLER ------------

    protected virtual float GetInputHorizontal()
    {
        // Left stick X axis from this controller only.[web:72][web:93]
        float axis = Input.GetJoyAxis(ControllerIdx, JoyAxis.LeftX);

        // Optional deadzone
        if (Mathf.Abs(axis) < 0.2f)
            axis = 0f;

        // Optional: also allow keyboard for this fighter
        axis += Input.GetAxis("p1_left", "p1_right");

        return Mathf.Clamp(axis, -1f, 1f);
    }

    protected virtual bool GetInputJump()
    {
        // Gamepad A button for this controller only.[web:72][web:93]
        bool padJump = Input.IsJoyButtonPressed(ControllerIdx, JoyButton.A);

        // Optional: also allow keyboard jump
        bool keyJump = Input.IsActionJustPressed("p1_jump");

        return padJump || keyJump;
    }

    // ------------ STATES ------------

    private void StepGroundNeutral(float dt)
    {
        float inputX = GetInputHorizontal();

        if (Mathf.Abs(inputX) > 0.01f)
            State = FighterState.Walk;
        else
            State = FighterState.Idle;

        ApplyGroundMovement(dt, inputX);

        if (GetInputJump() && IsOnFloor())
        {
            Velocity = new Vector2(Velocity.X, JumpVelocity);
            State = FighterState.Jump;
            StateFrame = 0;
        }
    }

    private void StepJump(float dt)
    {
        float inputX = GetInputHorizontal();

        // Optional: weaker air control later
        ApplyGroundMovement(dt, inputX);

        if (IsOnFloor())
        {
            State = FighterState.Idle;
            StateFrame = 0;
        }
    }

    // ------------ PHYSICS HELPERS ------------

    private void ApplyGravity(float dt)
    {
        if (!IsOnFloor())
        {
            Velocity = new Vector2(
                Velocity.X,
                Mathf.Clamp(Velocity.Y + Gravity * dt, -Mathf.Inf, MaxFallSpeed)
            );
        }
        else if (Velocity.Y > 0)
        {
            Velocity = new Vector2(Velocity.X, 0);
        }
    }

    private void ApplyHorizontalFriction(float dt)
    {
        float sign = Mathf.Sign(Velocity.X);
        float mag = Mathf.Abs(Velocity.X);
        float newMag = Mathf.Max(0, mag - FloorFriction * dt);
        Velocity = new Vector2(newMag * sign, Velocity.Y);
    }

    protected void ApplyGroundMovement(float dt, float inputX, bool allowDash = false)
    {
        float targetSpeed = 0f;

        if (Mathf.Abs(inputX) > 0.01f)
        {
            float speed = WalkSpeed;
            if (allowDash)
                speed *= RunSpeedMultiplier;

            targetSpeed = speed * Mathf.Sign(inputX);
            Facing = inputX > 0 ? 1 : -1;
        }

        float newSpeed = Mathf.MoveToward(Velocity.X, targetSpeed, MaxAccel * dt);
        Velocity = new Vector2(newSpeed, Velocity.Y);
    }
}
