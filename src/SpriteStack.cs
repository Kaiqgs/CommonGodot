using Godot;
using System;

[Tool]
public class SpriteStack : AnimatedSprite
{
    [Export]
    public int StackEnd = 999;

    private bool _showSprites = false;

    [Export]
    public bool ShowSprites
    {
        get => _showSprites;
        set
        {
            _showSprites = value;
            if (_showSprites)
            {
                this._createSprites();
            }
            else
            {
                this._clearSprites();
            }
        }
    }

    private float _subRotation = Mathf.Pi / 2f;

    [Export]
    public float SubRotation
    {
        get => _subRotation;
        set
        {
            _subRotation = value;
            this._updateRotation();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Stop();
        this._createSprites();
    }

    private void _createSprites()
    {
        var count = this.Frames.GetFrameCount(this.Animation);
        for (int i = 0; i < count && i < StackEnd; ++i)
        {
            var spr = new Sprite();
            var frame = this.Frames.GetFrame(this.Animation, i);
            spr.Texture = frame;
            spr.Position = new Vector2(spr.Position.x, -i);
            spr.Rotation = this._subRotation;
            AddChild(spr);
        }
        GD.PrintS("Added sprites: " + count);
    }

    private void _clearSprites()
    {
        foreach (Node child in GetChildren())
        {
            if (child is Sprite)
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }
    }

    private void _updateRotation()
    {
        // this.Rotation = this._subRotation;
        foreach (Node child in GetChildren())
        {
            if (child is Sprite)
            {
                var spr = (Sprite)child;
                spr.Rotation = this._subRotation;
            }
        }
    }
}
