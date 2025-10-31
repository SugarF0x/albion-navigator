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
	}

	private void AddLog(Log log)
	{
		if (SampleLog.Duplicate() is not Label newLog) throw new Exception("Sample log is not a Label");
		newLog.Text = log.ToString();
		LogsContainer.AddChild(newLog);
	}
}