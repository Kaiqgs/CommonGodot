using Godot;
using System;
using System.Collections.Generic;

public partial class SoundPlayer : Node2D
{
    [Export]
    public string Alias;

    [Export]
    public bool Randomize = false;

    [Export]
    public bool Repeat = false;

    [Export]
    public bool RandomPitch = false;

    private List<AudioStreamPlayer> _players = new List<AudioStreamPlayer>();
    private List<float> _volumes = new List<float>();
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private int _idx = 0;

    public override void _Ready()
    {
        for (int i = 0; i < GetChildCount(); ++i)
        {
            var child = GetChild(i) as AudioStreamPlayer;
            if (child != null)
            {
                _players.Add(child);
                _volumes.Add(child.VolumeDb);
            }
        }
    }

    private void _onTimerTimeout()
    {
        this.Play();
    }

    public int GetPlayIndex()
    {
        var lastIdx = _players.Count - 1;
        var idx = Randomize ? this._rng.RandiRange(0, lastIdx) : 0;
        while (!Repeat && Randomize && _players.Count > 1 && idx == lastIdx)
        {
            idx = this._rng.RandiRange(0, lastIdx);
        }
        return idx;
    }

    public bool IsPlaying()
    {
        foreach (var player in _players)
        {
            if (player.Playing)
            {
                return true;
            }
        }
        return false;
    }

    public void Play(float delay = 0)
    {
        if (_players.Count <= 0)
        {
            return;
        }

        var lastIdx = _players.Count - 1;
        var lastPlayer = _players[lastIdx];
        if (lastPlayer.Playing)
        {
            return;
        }

        if (delay != 0)
        {
            var timer = GetTree().CreateTimer(delay);
            timer.Connect("timeout", this, nameof(_onTimerTimeout));
            return;
        }

        var idx = this._idx = this.GetPlayIndex();
        var player = _players[idx];
        if (this.RandomPitch)
        {
            player.PitchScale = this._rng.RandfRange(0.9f, 1.1f);
        }
        player.Play();
        _players.RemoveAt(idx);
        _players.Add(player);
    }

    public float MaxVolume = 0f; // 72f;
    public float MinVolume = -80f;

    public void SetVolume(float volume)
    {
        // float unitVolume = (volume) / (72f + 80f);
        for (int i = 0; i < _players.Count; ++i)
        {
            var originalVolume = _volumes[i];
            var unitVolume = MathUtil.Map(volume, MinVolume, MaxVolume, 0f, 1f);
            var originalUnit = MathUtil.Map(originalVolume, MinVolume, MaxVolume, 0f, 1f);

            var resultingVolume = unitVolume * originalUnit;
            var mappedVolume = MathUtil.Map(resultingVolume, 0f, 1f, MinVolume, MaxVolume);

            // GD.Print($"originalVolume: {originalVolume}");
            // GD.Print($"volume: {unitVolume}");
            // GD.Print($"resultingVolume: {unitVolume}");
            // GD.Print($"mappedVolume: {mappedVolume}");
            var player = _players[i];
            player.VolumeDb = mappedVolume;
        }
    }

    public void Stop()
    {
        foreach (var player in _players)
        {
            player.Stop();
        }
    }
}
