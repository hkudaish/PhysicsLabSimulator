namespace VirtualStem.Science
{
    // Contract scaffold only: implementations belong to approved Phase 1.
    // TState must be immutable; rejection must preserve the input state.
    public interface IScientificSolver<TState, in TCommand, out TResult>
    {
        string ModelVersion { get; }
        TResult Execute(TState state, TCommand command);
    }
}
