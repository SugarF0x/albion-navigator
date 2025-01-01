using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace AlbionNavigator;

public class QuadTreeNode
{
    public enum Type {
        Void,
        Branch,
        Leaf,
    }

    public List<ForceGraphNode> Leaves { get; } = [];
    public QuadTreeNode[] Branches { get; set; } = [];
    public float Charge { get; set; } = 0.0f;
    public Vector2 CenterOfMass { get; set; } = Vector2.Zero;

    public bool IsLeaf => Leaves.Count > 0;
    public bool IsBranch => Leaves.Count  == 0 && Branches.Length > 0;
    public bool IsVoid => Leaves.Count == 0 && Branches.Length == 0;

    public void AttachLeaf(ForceGraphNode leaf)
    {
        if (IsBranch) throw new Exception("Can't attach leaf: not a branch");
        Leaves.Add(leaf);
    }

    public void BranchOut(Quad.Quadrant quadrant)
    {
        if (IsBranch) throw new Exception("Can't branch out: already a branch");

        CreateEmptyBranches();
        if (!IsLeaf) return;
        
        foreach (var leaf in Leaves)
        {
            Branches[(int)quadrant].AttachLeaf(leaf);
        }
            
        Leaves.Clear();
    }

    public void CreateEmptyBranches()
    {
        Branches = Enumerable.Range(0, Quad.QuadCount).Select(_ => new QuadTreeNode()).ToArray();
    }

    public void TrimBranches()
    {
        if (!IsBranch) throw new Exception("Can't trim branches: not a branch");
        
        var populatedBranches = Branches.Where(branch => branch.IsLeaf).ToArray();
        
        switch (populatedBranches.Length)
        {
            case > 1: return;
            case < 1:
                Branches = [];
                return;
            default:
            {
                var branch = populatedBranches.First();
                if (!branch.IsLeaf) return;

                Branches = [];
                foreach (var leaf in branch.Leaves) AttachLeaf(leaf);
                return;
            }
        }
    }
}