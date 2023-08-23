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

    public Vector2? GoalPosition { get; set; }

    [Export]
    public ShaderMaterial _shaderMaterial;

    [Signal]
    public delegate void FinishedRoutine();

    [Export]
    public float CompleteDelayTime = 3f;

    [Export]
    public float CompleteFirstStepTime = 2f;

    [Export]
    public float CompleteSecondStepTime = 2f;

    private List<CircleAnimation> _routines;
    private CircleAnimation _currentRoutine;
    private bool _lastRoutine = false;

    public override void _Ready()
    {
        this._routines = new List<CircleAnimation>
        {
            new CircleAnimation
            {
                InitialRadius = 0f,
                FinalRadius = .56f,
                Duration = 2f,
                EaseFunction = EasingFunctions.InOutQuad
            },
        };

        GoalPosition = Vector2.One / 2;
        this.OneShot = true;
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

        if (this.Target != null)
        {
            var transf = this.Target.GetGlobalTransformWithCanvas();
            Vector2 pos = transf.origin / GetViewport().Size;
            pos.y -= .175f;
            GoalPosition = pos;
        }

        if (GoalPosition.HasValue)
        {
            var targetPos = (Vector2)GoalPosition;
            this.SetState((float)_value, targetPos);
        }
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
        if(this._lastRoutine){
            GD.Print("OnCompletion duplicate avoided");
            return;
        }

        GD.Print("OnCompletion transition");
        this._routines.Add(
            new CircleAnimation
            {
                Duration = CompleteDelayTime,
                InitialRadius = 1f,
                FinalRadius = .1f,
                EaseFunction = EasingFunctions.OutQuint
            }
        );
        this._routines.Add(
            new CircleAnimation
            {
                Duration = CompleteFirstStepTime,
                InitialRadius = .1f,
                FinalRadius = 0f,
                EaseFunction = EasingFunctions.InElastic
            }
        );
        this._routines.Add(
            new CircleAnimation
            {
                Duration = CompleteSecondStepTime,
                InitialRadius = 0f,
                FinalRadius = 0f,
                EaseFunction = EasingFunctions.Linear,
            }
        );
        this._lastRoutine = true;
    }

    private void _onTimerTimeout()
    {
        try
        {
            if (this._currentRoutine == null)
            {
                return;
            }
            this.SetState((float)_currentRoutine.FinalRadius, null);
            _currentRoutine = null;
            if (_routines.Count <= 0)
            {
                EmitSignal("FinishedRoutine");
            }
        }
        catch (Exception e)
        {
            GD.PushError(e.ToString());
        }
    }
}
