using Godot;
using System;
using AlbionNavigator.Autoload;

public partial class AudioPlayer : Node
{
	[Export] public AudioStreamPlayer StreamPlayer;
	[ExportGroup("Sounds")] 
	[Export] public AudioStream ShutterSound;
	
	public enum SoundId
	{
		CameraShutter,
	}

	public void Play(SoundId id)
	{
		switch (id)
		{
			case SoundId.CameraShutter: StreamPlayer.Stream = ShutterSound; break;
			default: throw new ArgumentOutOfRangeException(nameof(id), id, null);
		}
		
		var random = new Random();
		StreamPlayer.PitchScale = 1.0f + (random.NextSingle() * 0.2f - 0.1f);
		StreamPlayer.Play();
	}

	public override void _Ready()
	{
		var screenCapture = GetNode<ScreenCapture>("/root/ScreenCapture");
		screenCapture.ScreenCaptured += _ => Play(SoundId.CameraShutter);
	}
}
