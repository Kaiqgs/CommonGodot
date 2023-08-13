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

    private List<AudioStreamPlayer> _players = new List<AudioStreamPlayer>();
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
        player.Play();
        _players.RemoveAt(idx);
        _players.Add(player);
    }

    public void SetVolume(float volume)
    {
        foreach (var player in _players)
        {
            player.VolumeDb = volume;
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
