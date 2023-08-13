using Godot;
using System;

public partial class FollowTarget : Node2D
{
    [Export]
    public string Target;

    [Export]
    public float LerpSpeed = 0.1f;

    [Export]
    public Vector2 Shift = new Vector2(0, 0);

    private Node2D _parent;

    private Vector2 _targetPosition;
    private KinematicBody2D _target;

    public override void _Ready()
    {
        _parent = GetParent<Node2D>();
        _target = GetNode<KinematicBody2D>(Target);
        _targetPosition = GetViewportRect().Size / 2f;

        GD.Print("FollowTarget: ", this._target.Name);
        GD.Print("FollowTarget: ", this._parent.Name);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Target != null)
        {
            this._targetPosition =
                new Vector2(this._target.GlobalPosition.x, this._target.GlobalPosition.y)
                + this.Shift;
        }

        var newpos = this._parent.Position.LinearInterpolate(
            this._targetPosition,
            LerpSpeed * delta
        );
        this._parent.Position = newpos;
    }
}
