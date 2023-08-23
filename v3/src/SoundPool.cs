using Godot;
using System;
using System.Collections.Generic;

public partial class SoundPool : Node2D
{
    [Export]
    public float GlobalInitialVolume = 0f;

    public float GlobalVolume;
    private Dictionary<string, SoundPlayer> _players = new Dictionary<string, SoundPlayer>();

    public static SoundPool Instance { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }

        this.GlobalVolume = GlobalInitialVolume;
        for (int i = 0; i < GetChildCount(); ++i)
        {
            var child = GetChild(i) as SoundPlayer;
            if (child != null)
            {
                _players[child.Alias] = child;

                child.SetVolume(this.GlobalVolume);
            }
        }
    }

    public void Play(string alias, float delay = 0)
    {
        if (_players.ContainsKey(alias))
        {
            _players[alias].Play(delay);
        }
    }

    public bool IsPlaying(string alias)
    {
        if (_players.ContainsKey(alias))
        {
            return _players[alias].IsPlaying();
        }
        return false;
    }

    public void SetVolume(float volume)
    {
        GlobalVolume = volume;
        foreach (var player in _players.Values)
        {
            player.SetVolume(volume);
        }
    }

    public void SetSpecificVolume(string alias, float volume)
    {
        if (_players.ContainsKey(alias))
        {
            _players[alias].SetVolume(volume);
        }
    }

    public void Stop(string alias = null)
    {
        if (string.IsNullOrEmpty(alias))
        {
            foreach (var player in _players.Values)
            {
                GD.Print($"SoundPool: stopping: {player.Alias}");
                player.Stop();
            }
            return;
        }
        if (_players.ContainsKey(alias))
        {
            _players[alias].Stop();
        }
        else
        {
            GD.PushWarning("SoundPool: No sound player with alias " + alias);
        }
    }
}
