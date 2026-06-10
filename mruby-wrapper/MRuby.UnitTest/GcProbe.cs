namespace MRuby.UnitTest;

using System;

/// <summary>
/// GC instrumentation helpers for the macOS crash fix test suite.
/// Measures CLR gen0/gen1/gen2 collection counts across a code body and
/// provides a safe NoGCRegion wrapper that never throws to the caller.
/// </summary>
public sealed class GcProbe
{
    private int _gen0Before;
    private int _gen1Before;
    private int _gen2Before;

    /// <summary>
    /// Record collection counts before the body under test.
    /// </summary>
    public void RecordBefore()
    {
        _gen0Before = GC.CollectionCount(0);
        _gen1Before = GC.CollectionCount(1);
        _gen2Before = GC.CollectionCount(2);
    }

    /// <summary>
    /// Returns (gen0Delta, gen1Delta, gen2Delta) since RecordBefore().
    /// </summary>
    public (int gen0, int gen1, int gen2) Delta()
    {
        return (
            GC.CollectionCount(0) - _gen0Before,
            GC.CollectionCount(1) - _gen1Before,
            GC.CollectionCount(2) - _gen2Before
        );
    }

    /// <summary>
    /// Asserts delta >= min (used by the canary to guard against false greens
    /// where ~0 GCs fired - that would make a green run meaningless as a signal).
    /// </summary>
    public static void MinGcCountAssertion(long delta, long min, string description = "")
    {
        if (delta < min)
            throw new InvalidOperationException(
                $"MinGcCount assertion failed{(description.Length > 0 ? " (" + description + ")" : "")}: " +
                $"expected >= {min} collections but got {delta}. " +
                "A near-zero GC count may mean this was a false green with no stress.");
    }

    /// <summary>
    /// Runs <paramref name="body"/> inside a NoGCRegion of <paramref name="budgetBytes"/>.
    /// Returns true if the region was successfully started (suppression applied).
    /// Returns false (and still runs body) if:
    ///   - budget is too large (ArgumentOutOfRangeException from TryStartNoGCRegion)
    ///   - system is low on memory (returns false)
    /// Does NOT throw to the caller if EndNoGCRegion fails (budget blown).
    /// </summary>
    public static bool RunInNoGCRegion(Action body, long budgetBytes = 64 * 1024 * 1024)
    {
        bool started = false;
        try
        {
            started = GC.TryStartNoGCRegion(budgetBytes);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Budget too large for this runtime; run without suppression.
            started = false;
        }

        try
        {
            body();
        }
        finally
        {
            if (started)
            {
                try { GC.EndNoGCRegion(); }
                catch (InvalidOperationException) { /* budget blown; already ended */ }
            }
        }

        return started;
    }
}
