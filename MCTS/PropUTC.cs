using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Math;

namespace MCTS
{
    public class PropUTC<TState, TMove, TPrePropState>
    {
        public IsFiniteStateDelegate<TState> IsFiniteState { get; }
        public EvaluateOutcomeDelegate<TState> EvaluateOutcome { get; }
        public double ExplorationParameter { get; }
        internal double ExplorationParameterBase { get; }
        internal AllowedMovesDelegate<TState, TMove, TPrePropState> AllowedMoves { get; }
        internal PossibleNextStatesDelegate<TState, TPrePropState> PossibleNextStates { get; }
        internal StateNode<TState, TMove, TPrePropState> Root { get; private set; }

        private double CurrentBestRating { get; set; }

        public PropUTC(TState startState, AllowedMovesDelegate<TState, TMove, TPrePropState> allowedMoves, PossibleNextStatesDelegate<TState, TPrePropState> possibleNextStates, IsFiniteStateDelegate<TState> isFiniteState, EvaluateOutcomeDelegate<TState> evaluateOutcome, double explorationParameter = 2.1, double explorationParameterBase = 0.07)
        {
            AllowedMoves = allowedMoves;
            PossibleNextStates = possibleNextStates;
            IsFiniteState = isFiniteState;
            EvaluateOutcome = evaluateOutcome;
            ExplorationParameter = explorationParameter;
            ExplorationParameterBase = explorationParameterBase;
            Root = new StateNode<TState, TMove, TPrePropState>(null, startState) { IsLeaf = true };
            Expansion(Root, Root);
        }

        public TMove GetCurrentBestMove()
        {
            ImproveSubtree(Root);
            ImproveSubtree(Root);
            ImproveSubtree(Root);
            ImproveSubtree(Root);
            ImproveSubtree(Root);
            var bestChild = Root.Children.First();
            foreach (var node in Root.Children)
            {
                if (node.Rating > bestChild.Rating)
                {
                    bestChild = node;
                }
            }
            return bestChild.MoveToReach;
        }

        public void ImproveTree(CancellationToken cancellationToken)
        {
            var followupsFromRoot = GetFollowupsFromRoot();
            Parallel.ForEach(followupsFromRoot, ImproveSubtree);
            /*ImproveSubtree(Root);
            /*ImproveSubtree(Root);
            ImproveSubtree(Root);
            ImproveSubtree(Root);
            ImproveSubtree(Root);*/
        }

        private void Backpropagation(StateNode<TState, TMove, TPrePropState> expandedNode, double simulationOutcome, StateNode<TState, TMove, TPrePropState> state)
        {
            while (expandedNode != Root)
            {
                StateNode<TState, TMove, TPrePropState> next;
                lock (expandedNode)
                {
                    lock (expandedNode.Parent)
                    {
                        expandedNode.Parent.Visits++;
                        expandedNode.Parent.Rating += simulationOutcome;
                        next = expandedNode.Parent?.Parent;
                    }
                }
                expandedNode = next;
            }
        }

        private StateNode<TState, TMove, TPrePropState> Expansion(StateNode<TState, TMove, TPrePropState> node, StateNode<TState, TMove, TPrePropState> state)
        {
            if (node.IsFiniteState || IsFiniteState(node.State))
            {
                node.IsLeaf = true;
                node.IsFiniteState = true;
                return node;
            }
            var allowedMoves = AllowedMoves(node.State);
            foreach (var allowedMove in allowedMoves)
            {
                var propNode = new PrePropNode<TState, TMove, TPrePropState>(node, allowedMove.Value, allowedMove.Key);
                node.Children.Add(propNode);
            }
            node.IsLeaf = false;
            node.IsFiniteState = false;
            var index = ThreadStaticRandom.Next(0, node.Children.Count);
            var randomNode = node.Children[index];
            if (!randomNode.GotExpanded)
            {
                randomNode.GotExpanded = true;
                var possibleNextStates = PossibleNextStates(randomNode.State);
                foreach (var possibleNextState in possibleNextStates)
                {
                    var kvp = new KeyValuePair<StateNode<TState, TMove, TPrePropState>, double>(new StateNode<TState, TMove, TPrePropState>(randomNode, possibleNextState.Key), possibleNextState.Value);
                    randomNode.Children.Add(kvp);
                }
            }
            return randomNode.Children.RandomlySelect();
        }

        private StateNode<TState, TMove, TPrePropState>[] GetFollowupsFromRoot()
        {
            List<StateNode<TState, TMove, TPrePropState>> result = new List<StateNode<TState, TMove, TPrePropState>>(32);

            foreach (var child in Root.Children)
            {
                if (!child.GotExpanded)
                {
                    child.GotExpanded = true;
                    var possibleNextStates = PossibleNextStates(child.State);
                    foreach (var possibleNextState in possibleNextStates)
                    {
                        var kvp = new KeyValuePair<StateNode<TState, TMove, TPrePropState>, double>(new StateNode<TState, TMove, TPrePropState>(child, possibleNextState.Key), possibleNextState.Value);
                        child.Children.Add(kvp);
                    }
                }
            }
            foreach (var rootChild in Root.Children)
            {
                var rootGrandChild = rootChild.Children.RandomlySelect();
                result.Add(rootGrandChild);
            }
            return result.ToArray();
        }
        private void ImproveSubtree(StateNode<TState, TMove, TPrePropState> state)
        {
            var selectedNode = Selection(state);
            var expandedNode = Expansion(selectedNode, state);
            var simulationOutcome = Simulation(expandedNode, state);
            Backpropagation(expandedNode, simulationOutcome, state);
        }

        private int RootVisits()
        {
            lock (Root)
            {
                if (Root.IsLeaf)
                    return 0;
                int sum = 0;
                foreach (var child in Root.Children)
                {
                    lock (child)
                        sum += child.Visits;
                }
                return sum;
            }
        }

        private StateNode<TState, TMove, TPrePropState> Selection(StateNode<TState, TMove, TPrePropState> start)
        {
            var current = start;
            while (!current.IsLeaf)
            {
                var bestChild = current.Children.MaxElement(UTC);
                if (!bestChild.GotExpanded)
                {
                    bestChild.GotExpanded = true;
                    var possibleNextStates = PossibleNextStates(bestChild.State);
                    foreach (var possibleNextState in possibleNextStates)
                    {
                        var kvp = new KeyValuePair<StateNode<TState, TMove, TPrePropState>, double>(new StateNode<TState, TMove, TPrePropState>(bestChild, possibleNextState.Key), possibleNextState.Value);
                        bestChild.Children.Add(kvp);
                    }
                }
                var propNode = bestChild.Children.RandomlySelect();
                current = propNode;
            }
            return current;
        }

        private double Simulation(StateNode<TState, TMove, TPrePropState> expandedNode, StateNode<TState, TMove, TPrePropState> stateNode)
        {
            TState state = expandedNode.State;
            int depth = 0;
            while (!IsFiniteState(state) && depth <= 2)
            {
                //DebugCallback(state);
                var allowedMoves = AllowedMoves(state);
                var random = ThreadStaticRandom.Next(0, allowedMoves.Length);
                var allowedMove = allowedMoves[random];
                state = PossibleNextStates(allowedMove.Value).RandomlySelect();
                depth++;
            }
            var outcome = EvaluateOutcome(state);
            return outcome;
        }

        private double UTC(PrePropNode<TState, TMove, TPrePropState> node)
        {
            var parent = node.Parent;
            double sum = 0;
            foreach (var sibling in parent.Children)
            {
                sum += sibling.Rating / (sibling.Visits + 1);
            }
            sum /= parent.Children.Count;
            double explorationParameter = sum * ExplorationParameter + ExplorationParameterBase;
            double rating = node.Rating / (node.Visits + 1);
            double explore = explorationParameter * Sqrt(Log(RootVisits() + 1) / (node.Visits + 1));
            double result = rating + explore;
            if (double.IsNaN(result)) { }
            return result;
        }

        public void MoveRoot(TState board)
        {
            foreach (var rootChild in Root.Children)
            {
                foreach (var grandChild in rootChild.Children)
                {
                    var newRoot = grandChild.Key;
                    if (Equals(newRoot.State, board))
                    {
                        newRoot.Parent = null;
                        Root = newRoot;
                        if (Root.IsLeaf)
                            Expansion(Root, Root);
                        return;
                    }
                }
            }
            throw new InvalidOperationException("No matching child found");
        }
    }
}