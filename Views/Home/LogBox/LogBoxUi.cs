using System;
using System.Collections.Generic;
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
		LinkService.Instance.ExpiredLinkRemoved += EnqueueExpirationLog;
		LinkService.Instance.NewLinkAdded += LogNewLinkAdded;
	}

	public override void _PhysicsProcess(double delta)
	{
		ProcessLogExpiredLinkRemovalQueue();
	}

	private void AddLog(Log log)
	{
		if (SampleLog.Duplicate() is not Label newLog) throw new Exception("Sample log is not a Label");
		newLog.Text = log.ToString();

		var scrollBar = GetVScrollBar();
		var shouldScrollToEnd = Math.Abs(scrollBar.MaxValue - scrollBar.Value) < 4;
		LogsContainer.AddChild(newLog);
		if (shouldScrollToEnd) CallDeferred(nameof(ScrollToEnd));
	}

	private void ScrollToEnd()
	{
		var scrollBar = GetVScrollBar();
		scrollBar.Value = scrollBar.MaxValue;
	}

	private void LogLinkExpirationUpdate(ZoneLink link, int from, int to)
	{
		var source = ZoneService.Instance.Zones[link.Source];
		var target = ZoneService.Instance.Zones[link.Target];
		Services.LogBox.Instance.Add($"Link expiration updated: {source.DisplayName} -> {target.DisplayName} : {link.Expiration}");
	}

	private readonly Queue<ZoneLink> ExpirationQueue = new ();
	private void EnqueueExpirationLog(ZoneLink link, int _) { lock (ExpirationQueue) ExpirationQueue.Enqueue(link); }
	private void ProcessLogExpiredLinkRemovalQueue()
	{
		lock (ExpirationQueue)
		{
			if (ExpirationQueue.Count == 0) return;
			while (ExpirationQueue.Count > 0)
			{
				var link = ExpirationQueue.Dequeue();
				var source = ZoneService.Instance.Zones[link.Source];
				var target = ZoneService.Instance.Zones[link.Target];
				Services.LogBox.Instance.Add($"Link expired: {source.DisplayName} -> {target.DisplayName}");				
			}
		}
	}

	private void LogNewLinkAdded(ZoneLink link, int _)
	{
		var source = ZoneService.Instance.Zones[link.Source];
		var target = ZoneService.Instance.Zones[link.Target];
		Services.LogBox.Instance.Add($"Link added: {source.DisplayName} -> {target.DisplayName} : {link.Expiration}");
	}
}