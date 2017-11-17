using System.Collections.Generic;

namespace MCTS
{
    public delegate KeyValuePair<TMove, TPrePropState>[] AllowedMovesDelegate<in TState, TMove, TPrePropState>(TState state);

    public delegate KeyValuePair<TState, double>[] PossibleNextStatesDelegate<TState, in TPrePropState>(TPrePropState prePropabilityState);

    public delegate bool IsFiniteStateDelegate<in TState>(TState state);

    public delegate double EvaluateOutcomeDelegate<in TState>(TState state);
}