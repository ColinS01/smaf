using Godot;

public partial class BaseFighter : CharacterBody2D
{
    public enum FighterState
    {
        Idle,
        Walk,
        Jump,
        Attack,
        Hitstun
    }

    public FighterState State { get; protected set; } = FighterState.Idle;
    public int StateFrame { get; protected set; } = 0;

    // Per‑fighter config
    [Export] public FighterData Data { get; set; }

    [Export] public int ControllerIdx { get; set; } = 0;

    // Horizontal traits (defaults, overridden by Data if present)
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
    public int Facing = 1; // 1 = right, -1 = left

    // Hooks for animation / VFX
    public AnimatedSprite2D Sprite { get; protected set; }
    protected AnimationTree AnimationTree;
    protected AnimationNodeStateMachinePlayback StateMachine;
    private CollisionShape2D _collisionShape;

    public override void _Ready()
    {
        // Grab nodes
        Sprite = GetNode<AnimatedSprite2D>("Sprite");
        AnimationTree = GetNode<AnimationTree>("AnimationTree");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        AnimationTree.Active = true;

        // Get state machine playback from the tree
        StateMachine = AnimationTree.Get("parameters/playback")
                        .As<AnimationNodeStateMachinePlayback>();

        ApplyData();
        ChangeState(FighterState.Idle);
        TravelAnimationState("idle");
    }

    private void ApplyData()
    {
        if (Data == null)
        {
            GD.PushWarning("BaseFighter has no FighterData assigned.");
            return;
        }

        // Movement traits from data
        WalkSpeed          = Data.WalkSpeed;
        RunSpeedMultiplier = Data.RunSpeedMultiplier;
        MaxAccel           = Data.MaxAccel;
        FloorFriction      = Data.FloorFriction;
        Gravity            = Data.Gravity;
        JumpVelocity       = Data.JumpVelocity;
        MaxFallSpeed       = Data.MaxFallSpeed;

        // Visuals (optional)
        if (Data.SpriteFrames != null && Sprite != null)
        {
            Sprite.SpriteFrames = Data.SpriteFrames;
        }

        // Collision sizing
        if (_collisionShape?.Shape is RectangleShape2D rect)
        {
            rect.Size = Data.CollisionSize;          // Godot 4 API [web:150][web:160]
            _collisionShape.Position = Data.CollisionOffset;
        }
        else
        {
            AutoSizeCollisionFromSprite();
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        StateFrame++;

        UpdateState(dt);
        UpdateAnimation(dt);

        ApplyGravity(dt);
        ApplyHorizontalFriction(dt);

        MoveAndSlide();
    }

    // ------------------
    //   INPUT
    // ------------------

    protected virtual float GetInputHorizontal()
    {
        // Gamepad left stick X for this controller
        float axis = Input.GetJoyAxis(ControllerIdx, JoyAxis.LeftX); // [web:62][web:65]

        // Deadzone
        if (Mathf.Abs(axis) < 0.2f)
            axis = 0f;

        // Keyboard axis (e.g., p1_left / p1_right in Input Map)
        axis += Input.GetAxis("p1_left", "p1_right"); // returns -1..1 [web:54][web:55][web:62]

        return Mathf.Clamp(axis, -1f, 1f);
    }

    protected virtual bool GetInputJump()
    {
        bool padJump = Input.IsJoyButtonPressed(ControllerIdx, JoyButton.A); // [web:62][web:65]
        bool keyJump = Input.IsActionJustPressed("p1_jump");

        return padJump || keyJump;
    }

    protected virtual bool GetInputAttack()
    {
        bool padAttack = Input.IsJoyButtonPressed(ControllerIdx, JoyButton.B);
        bool keyAttack = Input.IsActionJustPressed("p1_attack");

        return padAttack || keyAttack;
    }

    // ------------------
    //   STATE MACHINE
    // ------------------

    private void ChangeState(FighterState newState)
    {
        if (State == newState)
            return;

        State = newState;
        StateFrame = 0;
        PlayAnimationByState();
    }

    private void UpdateState(float dt)
    {
        switch (State)
        {
            case FighterState.Idle:
            case FighterState.Walk:
                StepGroundNeutral(dt);
                break;

            case FighterState.Jump:
                StepJump(dt);
                break;

            case FighterState.Attack:
                StepAttack(dt);
                break;

            case FighterState.Hitstun:
                StepHitstun(dt);
                break;
        }
    }

    private void StepGroundNeutral(float dt)
    {
        float inputX = GetInputHorizontal();

        if (Mathf.Abs(inputX) > 0.01f)
            ChangeState(FighterState.Walk);
        else
            ChangeState(FighterState.Idle);

        ApplyGroundMovement(dt, inputX);

        if (GetInputJump() && IsOnFloor())
        {
            Velocity = new Vector2(Velocity.X, JumpVelocity);
            ChangeState(FighterState.Jump);
        }
        else if (GetInputAttack() && IsOnFloor())
        {
            ChangeState(FighterState.Attack);
            // later you can call PerformAttack() here
        }
    }

    private void StepJump(float dt)
    {
        float inputX = GetInputHorizontal();
        ApplyGroundMovement(dt, inputX);

        if (IsOnFloor())
        {
            ChangeState(FighterState.Idle);
        }
    }

    private void StepAttack(float dt)
    {
        // Placeholder attack duration; later tie to Data or animation length
        if (StateFrame >= 15)
        {
            ChangeState(FighterState.Idle);
        }
    }

    private void StepHitstun(float dt)
    {
        if (StateFrame >= 10)
        {
            ChangeState(FighterState.Idle);
        }
    }

    // ------------------
    //   PHYSICS HELPERS
    // ------------------

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

    // ------------------
    //   ANIMATION / VFX
    // ------------------

    protected virtual void UpdateAnimation(float dt)
    {
        if (Sprite == null)
            return;

        // Flip sprite based on facing
        Sprite.FlipH = Facing == -1;
    }

    private void TravelAnimationState(string nodeName)
    {
        if (StateMachine == null)
            return;

        StateMachine.Travel(nodeName);
    }

    protected virtual void PlayAnimationByState()
    {
        string name = "idle";

        switch (State)
        {
            case FighterState.Idle:    name = "idle";    break;
            case FighterState.Walk:    name = "walk";    break;
            case FighterState.Jump:    name = "jump";    break;
        }

        TravelAnimationState(name);
    }

    // Optional hook for subclasses / Data‑driven attacks
    protected virtual void PerformAttack()
    {
        // Melee or ranged behavior can be implemented or overridden here later.
    }

    private void AutoSizeCollisionFromSprite()
    {
        if (_collisionShape?.Shape is RectangleShape2D rect && Sprite?.SpriteFrames != null)
        {
            // Use current animation's first frame texture size as a rough body box
            var anim = "idle";
            var tex = Sprite.SpriteFrames.GetFrameTexture(anim, 0);
            if (tex != null)
            {
                var size = tex.GetSize();          // in pixels
                rect.Size = size;                  // or size * some scale
                _collisionShape.Position = new Vector2(0, -size.Y * 0.5f); // e.g. anchor at feet
            }
        }
    }

}
