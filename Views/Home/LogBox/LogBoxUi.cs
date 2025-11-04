using System;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.LogBox;

public partial class LogBoxUi : ScrollContainer
{
	private Label SampleLog;
	private VBoxContainer LogsContainer;
	
	public override void _Ready()
	{
		SampleLog = GetNode<Label>("%SampleLog");
		LogsContainer = GetNode<VBoxContainer>("%LogsContainer");
		LogsContainer.RemoveChild(SampleLog);
		
		foreach (var log in Services.LogBox.Instance.Logs) AddLog(log);
		Services.LogBox.Instance.NewEntryAdded += AddLog;
		LinkService.Instance.LinkExpirationUpdated += LogLinkExpirationUpdate;
		LinkService.Instance.ExpiredLinkRemoved += LogExpiredLinkRemoval;
		LinkService.Instance.NewLinkAdded += LogNewLinkAdded;
	}

	private void AddLog(Log log)
	{
		if (SampleLog.Duplicate() is not Label newLog) throw new Exception("Sample log is not a Label");
		newLog.Text = log.ToString();
		LogsContainer.AddChild(newLog);
	}

	private void LogLinkExpirationUpdate(ZoneLink link)
	{
		var source = ZoneService.Instance.Zones[link.Source];
		var target = ZoneService.Instance.Zones[link.Target];
		Services.LogBox.Instance.Add($"Link expiration updated: {source.DisplayName} -> {target.DisplayName} : {link.Expiration}");
	}

	private void LogExpiredLinkRemoval(ZoneLink link)
	{
		var source = ZoneService.Instance.Zones[link.Source];
		var target = ZoneService.Instance.Zones[link.Target];
		Services.LogBox.Instance.Add($"Link expired: {source.DisplayName} -> {target.DisplayName}");
	}

	private void LogNewLinkAdded(ZoneLink link)
	{
		var source = ZoneService.Instance.Zones[link.Source];
		var target = ZoneService.Instance.Zones[link.Target];
		Services.LogBox.Instance.Add($"Link added: {source.DisplayName} -> {target.DisplayName} : {link.Expiration}");
	}
}