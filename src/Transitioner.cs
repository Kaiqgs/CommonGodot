using Godot;
using System;
using System.Collections.Generic;

public class CircleAnimation
{
    CircleAnimation next;

    public float InitialRadius;
    public float FinalRadius;
    public double Duration;

    public Func<float, float> EaseFunction;

    public double GetElapsedTime(double timeLeft)
    {
        return Duration - timeLeft;
    }

    public double GetElapsedScaled(double timeLeft)
    {
        return GetElapsedTime(timeLeft) / Duration;
    }

    public float GetDelta()
    {
        return FinalRadius - InitialRadius;
    }
}

public partial class Transitioner : Timer
{
    public KinematicBody2D Target { get; set; }

    [Export]
    public ShaderMaterial _shaderMaterial;

    [Signal]
    public delegate void FinishedRoutine();

    private List<CircleAnimation> _routines;
    private CircleAnimation _currentRoutine;

    public override void _Ready()
    {
        this._routines = new List<CircleAnimation>
        {
            new CircleAnimation
            {
                InitialRadius = 0f,
                FinalRadius = .56f,
                Duration = 2f,
                EaseFunction = Kryz.Tweening.EasingFunctions.InOutQuad
            },
        };

        this.Connect("timeout", this, nameof(_onTimerTimeout));
    }

    public override void _Process(float delta)
    {
        // GD.Print(this._routines.Count);
        if (_currentRoutine == null && _routines.Count <= 0)
        {
            return;
        }
        else if (_currentRoutine == null && _routines.Count > 0 && this.IsStopped())
        {
            this._currentRoutine = this._routines[0];
            this._routines.RemoveAt(0);
            this.WaitTime = (float)this._currentRoutine.Duration;
            this.Start();
        }

        if (this.IsStopped())
        {
            return;
        }

        var elapsedTime = this._currentRoutine.GetElapsedScaled(this.TimeLeft);
        var interpolated = Mathf.Lerp(
            this._currentRoutine.InitialRadius,
            this._currentRoutine.FinalRadius,
	    this._currentRoutine.EaseFunction((float)elapsedTime)
        );
        // var interpolated = Tween.InterpolateValue(
        //     this._currentRoutine.InitialRadius,
        //     this._currentRoutine.GetDelta(),
        //     elapsedTime,
        //     this._currentRoutine.Duration,
        //     this._currentRoutine.TransitionType,
        //     this._currentRoutine.EaseType
        // );
        float _value = (float)interpolated;
        //Good combination:
        //Back
        //Out
        //2ndHalf:
        //Elastic
        //In
        // GD.Print("elapsedTime: ", elapsedTime, " value: ", _value);

        Vector2 targetPos;
        if (this.Target != null)
        {
            var transf = this.Target.GetGlobalTransformWithCanvas();
            targetPos = transf.origin / GetViewport().Size;
            // targetPos = transf.Origin / rect.Size;
            // GD.Print(transf);
            targetPos.y -= .175f;
        }
        else
        {
            targetPos = Vector2.One / 2;
        }
        this.SetState((float)_value, targetPos);
    }

    public void SetState(float radius, Vector2? position)
    {
        if (!position.HasValue)
        {
            position = (Vector2)this._shaderMaterial.GetShaderParam("position");
        }
        this._shaderMaterial.SetShaderParam("position", position.Value);
        this._shaderMaterial.SetShaderParam("radius", radius);
    }

    public void OnCompletion(string name)
    {
        GD.Print("OnCompletion transition");
        this._routines.Add(
            new CircleAnimation
            {
                Duration = .5f,
                InitialRadius = 1f,
                FinalRadius = .1f,
                EaseFunction = Kryz.Tweening.EasingFunctions.OutQuint
            }
        );
        this._routines.Add(
            new CircleAnimation
            {
                Duration = 1f,
                InitialRadius = .1f,
                FinalRadius = 0f,
                EaseFunction = Kryz.Tweening.EasingFunctions.InElastic
            }
        );
        this._routines.Add(
            new CircleAnimation
            {
                Duration = 1.5f,
                InitialRadius = 0f,
                FinalRadius = 0f,
                EaseFunction = Kryz.Tweening.EasingFunctions.Linear,
            }
        );
    }

    private void _onTimerTimeout()
    {
        this.SetState((float)_currentRoutine.FinalRadius, null);
        _currentRoutine = null;
        if (_routines.Count <= 0)
        {
            EmitSignal("FinishedRoutine");
        }
    }
}
