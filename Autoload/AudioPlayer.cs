using Godot;
using System;
using AlbionNavigator.Autoload;

public partial class AudioPlayer : Node
{
	[Export] public AudioStreamPlayer StreamPlayer;
	[ExportGroup("Sounds")] 
	[Export] public AudioStream ShutterSound;
	[Export] public AudioStream TrashSound;
	
	public enum SoundId
	{
		CameraShutter,
		PaperTrash,
	}


	public void Play(string name)
	{
		Play((SoundId)Enum.Parse(typeof(SoundId), name));
	}
	
	public void Play(SoundId id)
	{
		AudioStream[] idToStreamMap =
		[
			ShutterSound,
			TrashSound,
		];

		if ((int)id >= idToStreamMap.Length) throw new ArgumentOutOfRangeException(nameof(id), id, null);
		
		StreamPlayer.Stream = idToStreamMap[(int)id];
		StreamPlayer.PitchScale = 1.0f + (new Random().NextSingle() * 0.2f - 0.1f);
		StreamPlayer.Play();
	}

	public override void _Ready()
	{
		
		
		var screenCapture = GetNode<ScreenCapture>("/root/ScreenCapture");
		screenCapture.ScreenCaptured += _ => Play(SoundId.CameraShutter);
	}
}
