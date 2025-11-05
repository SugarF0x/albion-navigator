using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace AlbionNavigator.Utils.ForceDirectedGraph;

public partial class Simulation
{
    private void Tick(int iterations = 1)
    {
        for (var k = 0; k < iterations; k++)
        {
            Alpha += (AlphaTarget - Alpha) * AlphaDecay;
            foreach (var force in Forces) force.Apply(Alpha);

            foreach (var node in Nodes.Where(node => !node.IsFrozen))
            {
                node.Velocity *= VelocityDecay;
                node.Position += node.Velocity;
            }
        }
    }

    public bool IsSimulationRunning;

    public delegate void OnSimulationStartedHandler();
    public event OnSimulationStartedHandler OnSimulationStarted;
    public delegate void OnSimulationFinishedHandler();
    public event OnSimulationFinishedHandler OnSimulationFinished;

    public void Start()
    {
        if (!(Alpha >= AlphaMin)) return;
        
        StartSimulation();
        while (Alpha >= AlphaMin) Tick();
        FinishSimulation();
    }

    public bool Step()
    {
        if (Alpha < AlphaMin) return false;
        Tick();
        return true;
    }

    public async void StartAsync()
    {
        try
        {
            if (!(Alpha >= AlphaMin)) return;
        
            StartSimulation();
            await Task.Run(() =>
            {
                while (Alpha >= AlphaMin) Tick();
            });
            FinishSimulation();
        }
        catch (Exception e)
        {
            GD.PrintErr("Simulation failed: " + e);
        }
    }

    private void StartSimulation()
    {
        IsSimulationRunning = true;
        OnSimulationStarted?.Invoke();
    }

    private void FinishSimulation()
    {
        IsSimulationRunning = false;
        OnSimulationFinished?.Invoke();
    }
}