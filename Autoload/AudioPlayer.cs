using System;
using System.Collections.Generic;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Autoload;

public partial class AudioPlayer : Node
{
	[Export] public AudioStreamPlayer StreamPlayer;
	[ExportGroup("Sounds")] 
	[Export] public AudioStream ShutterSound;
	[Export] public AudioStream TrashSound;
	[Export] public AudioStream PortalOpenSound;
	[Export] public AudioStream PortalCloseSound;
	[Export] public AudioStream ClockRetractSound;
	
	public static AudioPlayer Instance { get; private set; }
	
	public override void _Ready()
	{
		Instance = this;

		SyncVolumeSettings(SettingsService.Instance.Volume.Value);
		SettingsService.Instance.Volume.ChangedTo += SyncVolumeSettings;
		
		LinkService.Instance.NewLinkAdded += (_, _) => QueuePlay(SoundId.PortalOpen);
		LinkService.Instance.ExpiredLinkRemoved += (_, _) => QueuePlay(SoundId.PortalClose);
		LinkService.Instance.LinkExpirationUpdated += (_, _, _) => QueuePlay(SoundId.ClockRetract);
	}

	public override void _PhysicsProcess(double delta)
	{
		while (PlayQueue.Count > 0) Play(PlayQueue.Dequeue());
	}

	private void SyncVolumeSettings(float value) { StreamPlayer.VolumeLinear = value; }

	private readonly Queue<SoundId> PlayQueue = [];
	public void QueuePlay(SoundId id) => PlayQueue.Enqueue(id);
	
	private void Play(SoundId id)
	{
		Dictionary<SoundId, AudioStream> idToStreamMap = new()
		{
			{ SoundId.CameraShutter, ShutterSound },
			{ SoundId.PaperTrash, TrashSound },
			{ SoundId.PortalOpen, PortalOpenSound },
			{ SoundId.PortalClose, PortalCloseSound },
			{ SoundId.ClockRetract, ClockRetractSound },
		};
		
		StreamPlayer.Stream = idToStreamMap[id];
		StreamPlayer.PitchScale = 1.0f + (new Random().NextSingle() * 0.2f - 0.1f);
		StreamPlayer.Play();
	}
	
	public enum SoundId
	{
		CameraShutter,
		PaperTrash,
		PortalOpen,
		PortalClose,
		ClockRetract,
	}
}