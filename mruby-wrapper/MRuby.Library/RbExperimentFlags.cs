using System;
using System.Diagnostics.CodeAnalysis;

namespace MRuby.Library
{
    // EXPERIMENT-ONLY toggles for the macOS-crash root-cause investigation.
    //
    // These flags let a controlled experiment REVERT the shipped thread-safety fix at
    // runtime (disable the process-global map locks) WITHOUT changing default behavior:
    // every flag defaults to FALSE, so an unset environment leaves the shipped locking
    // byte-identical to production. They exist purely to test hypothesis H1 - that the
    // original macOS CI hard-crash was caused by concurrent corruption of the two
    // process-global dictionaries (StateMapper, RbDataClassMapping), not by a generic
    // reverse-P/Invoke GC-suspension window (which a separate mruby-free experiment
    // already falsified, 360/360 clean).
    //
    // Values are read ONCE at process start (static initialization) so the hot lock
    // paths never touch the environment. Each lock is independently toggleable so the
    // experiment can attribute a failure to a SPECIFIC map, not just "some removed lock".
    //
    //   MRUBY_DISABLE_STATEMAPPER_LOCK   = "1" -> StateMapperLock becomes a no-op
    //   MRUBY_DISABLE_DATACLASS_LOCK     = "1" -> RbDataClassMappingLock becomes a no-op
    //
    // VmLifecycleLock is deliberately NOT toggleable here: disabling it would test a
    // different hypothesis (native open/close global races), confounding H1.
    [ExcludeFromCodeCoverage]
    internal static class RbExperimentFlags
    {
        internal static readonly bool DisableStateMapperLock =
            Environment.GetEnvironmentVariable("MRUBY_DISABLE_STATEMAPPER_LOCK") == "1";

        internal static readonly bool DisableDataClassLock =
            Environment.GetEnvironmentVariable("MRUBY_DISABLE_DATACLASS_LOCK") == "1";

        // True if any experimental lock bypass is active. Used only for diagnostic banner
        // printing in the test harness; never gates production behavior.
        internal static bool AnyLockDisabled => DisableStateMapperLock || DisableDataClassLock;
    }
}
