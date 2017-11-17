using System.Collections.Generic;

namespace MCTS
{
    internal class StateNode<TState, TMove, TPrePropState>
    {
        internal List<PrePropNode<TState, TMove, TPrePropState>> Children { get; } = new List<PrePropNode<TState, TMove, TPrePropState>>(16);
        internal bool IsLeaf { get; set; } = true;
        internal bool IsFiniteState { get; set; }
        internal PrePropNode<TState, TMove, TPrePropState> Parent { get; set; }
        internal TState State { get; }

        internal StateNode(PrePropNode<TState, TMove, TPrePropState> parent, TState state)
        {
            Parent = parent;
            State = state;
        }
    }
}