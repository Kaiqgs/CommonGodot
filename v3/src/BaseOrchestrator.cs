using Godot;
using System;

public abstract class BaseOrchestrator : Node2D, IComplete
{
    [Export]
    public string TransitionerPath = "Transitions";

    [Export]
    public string AmbientMusicAlias = "music/stage";

    [Export]
    public string NextScene = "res://scenes/main.tscn";

    [Export]
    public string MenuScene = "res://scenes/menu.tscn";

    [Export]
    public string StoryScene = "res://scenes/stages/stage1.tscn";

    [Export]
    public string FreePlayScene = "res://scenes/freeplay.tscn";

    [Export]
    public ShaderMaterial PostProcessingMaterial;

    [Export]
    public float PostShiftSpeed = 10f;

    [Export]
    public Vector3 PostLowerClamp = new Vector3(.9f, .9f, .9f);

    [Export]
    public Vector3 PostUpperClamp = new Vector3(5f, 5f, 5f);

    [Export]
    public Color NextMainColor = Colors.Transparent;

    [Export]
    public Color NextSecondaryColor = Colors.Transparent;

    [Export]
    public bool StopAmbientOnTransition = false;

    private Transitioner _transitioner;
    public bool Completed;
    private Sprite _loseMessageSprite;
    private string _completeName;
    private bool _firstRoutineCompleted;
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        this._transitioner = GetNode<Transitioner>(TransitionerPath);
        this._transitioner.Connect("FinishedRoutine", this, nameof(FinishedRoutine));
        this._loseMessageSprite = GetNode<Sprite>("CanvasTransition/LoseMessage");

        if (
            this.NextMainColor != Colors.Transparent
            && this.NextSecondaryColor != Colors.Transparent
        )
        {
            this.PostProcessingMaterial.SetShaderParam("setMainColor", this.NextMainColor);
            this.PostProcessingMaterial.SetShaderParam(
                "setSecondaryColor",
                this.NextSecondaryColor
            );
        }
        else
        {
            this.PostProcessingMaterial.SetShaderParam("setMainColor", null);
            this.PostProcessingMaterial.SetShaderParam("setSecondaryColor", null);
        }
        // this.PostProcessingMaterial.SetShaderParam("SetMainColor", this.NextMainColor);
        // this.PostProcessingMaterial.SetShaderParam("SetSecondaryColor", this.NextSecondaryColor);
        //this._transitioner.Connect("timeout", this, nameof(FinishedRoutine));
    }

    public void Complete(string name)
    {
        if (name == "complete/fail")
        {
            this._loseMessageSprite.Frame = (int)GD.RandRange(0, _loseMessageSprite.Hframes - 1);
            this._loseMessageSprite.Visible = true;
            SoundPool.Instance.Play("sfx/fail");
        }
        else if (name == "complete")
        {
            SoundPool.Instance.Play("sfx/success");
        }

        this.Completed = true;
        this._transitioner.OnCompletion(name);
        this._completeName = name;
    }

    public void FinishedRoutine()
    {
        this._firstRoutineCompleted = true;
        if (!this.Completed)
        {
            return;
        }

        if (this._completeName == "complete")
        {
            if (this.StopAmbientOnTransition)
            {
                SoundPool.Instance.Stop();
            }
            CrossScene.Instance?.VisitedScenes.Add(GetTree().CurrentScene.Filename);
            GetTree().ChangeScene(NextScene);
        }
        else if (this._completeName == "complete/fail")
        {
            GD.Print("haha lose to a bot?");
            GetTree().ChangeScene(GetTree().CurrentScene.Filename);
        }
        else if (this._completeName == "goto/story")
        {
            GetTree().ChangeScene(StoryScene);
        }
        else if (this._completeName == "goto/freeplay")
        {
            GetTree().ChangeScene(FreePlayScene);
        }
        else if (this._completeName == "goto/menu")
        {
            SoundPool.Instance.Stop(AmbientMusicAlias);
            GetTree().ChangeScene(MenuScene);
        }
    }

    public static bool DEBUG = false;

    // private float centerRand => (this.rng.Randf() * 2 - 1);
    private float centerRand => (GD.Randf() * 2f - 1f);

    public void ToggleShaderMadness(bool madness)
    {
        CrossScene.Instance.ShaderMadness = madness;
        if (!CrossScene.Instance.ShaderMadness)
        {
            this.PostProcessingMaterial.SetShaderParam("brightness", 1f);
            this.PostProcessingMaterial.SetShaderParam("contrast", 1f);
            this.PostProcessingMaterial.SetShaderParam("saturation", 1f);
        }
    }

    public override void _Process(float delta)
    {
        if (CrossScene.Instance.ShaderMadness)
        {
            Vector3 updatePost = Vector3.One * PostShiftSpeed * delta;
            var newPost = CrossScene.Instance.PostProcessing + updatePost;
            CrossScene.Instance.PostProcessing = newPost;

            var brightnessNoise = CrossScene.Instance.Noise.GetNoise3d(newPost.x, 0f, 0f);
            var contrastNoise = CrossScene.Instance.Noise.GetNoise3d(0f, newPost.y, 0f);
            var saturationNoise = CrossScene.Instance.Noise.GetNoise3d(0f, 0f, newPost.z);
            var brightness = MathUtil.Map(
                brightnessNoise,
                -1,
                1,
                PostLowerClamp.x,
                PostUpperClamp.x
            );
            var contrast = MathUtil.Map(contrastNoise, -1, 1, PostLowerClamp.y, PostUpperClamp.y);
            var saturation = MathUtil.Map(
                saturationNoise,
                -1,
                1,
                PostLowerClamp.z,
                PostUpperClamp.z
            );

            this.PostProcessingMaterial.SetShaderParam("brightness", brightness);
            this.PostProcessingMaterial.SetShaderParam("contrast", contrast);
            this.PostProcessingMaterial.SetShaderParam("saturation", saturation);
            // GD.Print(
            //     $"Brightness: {brightness}, Contrast: {contrast}, Saturation: {saturation}"
            // );
        }

        if (Input.IsActionJustReleased("skip") && DEBUG)
        {
            // _loseMessageSprite.Visible = true;
            this._completeName = "complete";
            this.Completed = true;
            this.FinishedRoutine();
            // or
            // this.Complete("complete");
        }
        else if (Input.IsActionJustReleased("fail") && DEBUG)
        {
            this.Complete("complete/fail");
        }
        else if (Input.IsActionJustReleased("skip") && !DEBUG)
        {
            this.Complete("complete");
        }

        if (!SoundPool.Instance.IsPlaying(AmbientMusicAlias))
        {
            GD.Print("Started playing ambient music");
            SoundPool.Instance.Stop();
            SoundPool.Instance.Play(AmbientMusicAlias);
        }
        if (!this._firstRoutineCompleted)
        {
            return;
        }
        var viewsize = GetViewport().Size;
        this._transitioner.GoalPosition = GetViewport().GetMousePosition() / viewsize;
    }

    public void OnCompletion()
    {
        this.FinishedRoutine();
    }
}
