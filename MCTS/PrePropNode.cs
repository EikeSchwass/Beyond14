using System.Collections.Generic;

namespace MCTS
{
    internal class PrePropNode<TState, TMove, TPrePropState>
    {
        internal List<KeyValuePair<StateNode<TState, TMove, TPrePropState>, double>> Children { get; } = new List<KeyValuePair<StateNode<TState, TMove, TPrePropState>, double>>(16);
        internal StateNode<TState, TMove, TPrePropState> Parent { get; }
        internal TPrePropState State { get; }
        internal TMove MoveToReach { get; }
        internal int Visits { get; set; }
        internal double Rating { get; set; }
        public bool GotExpanded { get; set; }

        public PrePropNode(StateNode<TState, TMove, TPrePropState> parent, TPrePropState state, TMove moveToReach)
        {
            Parent = parent;
            State = state;
            MoveToReach = moveToReach;
        }

        public override string ToString()
        {
            return $"{Rating / Visits:F4} | {Rating:F4} | {Visits}";
        }
    }
}