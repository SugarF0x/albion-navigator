using Godot;
using System;
using System.Collections.Generic;
using AlbionNavigator.Autoload;

public partial class AudioPlayer : Node
{
	[Export] public AudioStreamPlayer StreamPlayer;
	[ExportGroup("Sounds")] 
	[Export] public AudioStream ShutterSound;
	[Export] public AudioStream TrashSound;
	[Export] public AudioStream PortalOpenSound;
	[Export] public AudioStream PortalCloseSound;
	
	public enum SoundId
	{
		CameraShutter,
		PaperTrash,
		PortalOpen,
		PortalClose,
	}

	public void Play(string name)
	{
		Play((SoundId)Enum.Parse(typeof(SoundId), name));
	}
	
	public void Play(SoundId id)
	{
		Dictionary<SoundId, AudioStream> idToStreamMap = new()
		{
			{ SoundId.CameraShutter, ShutterSound },
			{ SoundId.PaperTrash, TrashSound },
			{ SoundId.PortalOpen, PortalOpenSound },
			{ SoundId.PortalClose, PortalCloseSound },
		};
		
		StreamPlayer.Stream = idToStreamMap[id];
		StreamPlayer.PitchScale = 1.0f + (new Random().NextSingle() * 0.2f - 0.1f);
		StreamPlayer.Play();
	}

	public override void _Ready()
	{
		var screenCapture = GetNode<ScreenCapture>("/root/ScreenCapture");
		screenCapture.ScreenCaptured += _ => Play(SoundId.CameraShutter);
	}
}
