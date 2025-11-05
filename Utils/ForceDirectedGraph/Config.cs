using System;

namespace AlbionNavigator.Utils.ForceDirectedGraph;

public partial class Simulation
{
    public float InitialRadius = 10;
    public float InitialAngle = (float)Math.PI * (3 - float.Sqrt(5));
    
    public float Alpha = 1;
    public float AlphaTarget;

    public float AlphaDecay = 0.02276277904418933f;
    public float VelocityDecay = 0.6f;
    
    private float _alphaMin = 0.001f;
    public float AlphaMin
    {
        get => _alphaMin;
        set
        {
            _alphaMin = value;
            AdjustAlphaDecayToAlphaMin();
        }
    }

    public void AdjustAlphaDecayToAlphaMin()
    {
        AlphaDecay = 1f - float.Pow(AlphaMin, 1f / 300f);
    }
}