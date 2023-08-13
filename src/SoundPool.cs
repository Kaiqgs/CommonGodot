using Godot;
using System;
using System.Collections.Generic;

public partial class SoundPool : Node2D
{
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
        for (int i = 0; i < GetChildCount(); ++i)
        {
            var child = GetChild(i) as SoundPlayer;
            if (child != null)
            {
                _players[child.Alias] = child;
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

    public void SetVolume(float volume)
    {
        foreach (var player in _players.Values)
        {
            player.SetVolume(volume);
        }
    }

    public void SetSpecificVolume(string alias, float volume) { 

        if (_players.ContainsKey(alias))
        {
            _players[alias].SetVolume(volume);
        }
    }

    public void Stop(string alias)
    {
        if (_players.ContainsKey(alias))
        {
            _players[alias].Stop();
        }
        else {

            foreach (var player in _players.Values)
            {
                player.Stop();
            }


            GD.PushWarning("SoundPool: No sound player with alias " + alias);

        }
    }
}
