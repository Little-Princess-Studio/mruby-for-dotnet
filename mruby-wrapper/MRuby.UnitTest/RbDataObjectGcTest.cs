namespace MRuby.UnitTest;

using System;
using System.Threading;
using Library;
using Library.Language;

// Single-threaded GC-stress probes for the mruby-4.0 data-object free path.
//
// When a C# object is embedded in an mruby data object, its native `dfree` callback
// runs during mruby GC / mrb_close and crosses back into managed GCHandle.Free. These
// tests force an aggressive GC while a data object is live, then close the state, to
// check that path survives collection. (Investigation note: the free delegate IS rooted
// via the static data-class mapping, so these pass single-threaded; kept as guards.)
//
// They are [WindowsOnlyFact]: a forced GC.Collect while other test classes run inside
// native mruby hard-exits the macOS/Linux host (signal-based GC thread-suspension),
// independent of any library bug. Own non-parallel collection so the GC is isolated.
[Collection("DataObjectGcSerial")]
public class RbDataObjectGcTest
{
    private sealed class Payload
    {
        public long Value { get; set; }
    }

    private static void ForceFullGc()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
        }
    }

    [Fact]
    public void TestDoubleCloseIsSafeNoOp()
    {
        var state = Ruby.Open();

        Ruby.Close(state);
        Assert.Equal(IntPtr.Zero, state.NativeHandler);

        Ruby.Close(state);
        Assert.Equal(IntPtr.Zero, state.NativeHandler);
    }

    [WindowsOnlyFact]
    public void TestCloseDisarmsLiveDataObjects()
    {
        var releasedCount = 0;

        var state = Ruby.Open();
        var cls = state.DefineClass("DisarmProbe", null);
        cls.DefineMethod("initialize", (_, self, _) => self, RbHelper.MRB_ARGS_NONE(), out _);

        for (var i = 0; i < 5; i++)
        {
            cls.NewObjectWithCSharpDataObject(
                "DisarmProbe",
                new Payload(),
                (_, _) => releasedCount++);
        }

        Ruby.Close(state);

        Assert.Equal(5, releasedCount);
        Assert.Equal(IntPtr.Zero, state.NativeHandler);
    }

    [WindowsOnlyFact]
    public void TestMidProgramGcCallsReleaseFnExactlyOnce()
    {
        var releasedCount = 0;
        var state = Ruby.Open();
        var cls = state.DefineClass("MidGcProbe", null);
        cls.DefineMethod("initialize", (_, self, _) => self, RbHelper.MRB_ARGS_NONE(), out _);

        cls.NewObjectWithCSharpDataObject("MidGcProbe", new Payload(), (_, _) => releasedCount++);

        ForceFullGc();
        using var compiler = state.NewCompiler();
        compiler.LoadString("GC.start");

        Ruby.Close(state);

        Assert.Equal(1, releasedCount);
    }

    [WindowsOnlyFact]
    public void TestReleaseFnCanOpenAndCloseAnotherStateWithoutDeadlock()
    {
        var state = Ruby.Open();
        var cls = state.DefineClass("DeadlockProbe", null);
        cls.DefineMethod("initialize", (_, self, _) => self, RbHelper.MRB_ARGS_NONE(), out _);

        cls.NewObjectWithCSharpDataObject(
            "DeadlockProbe",
            new Payload(),
            (_, _) =>
            {
                var inner = Ruby.Open();
                Ruby.Close(inner);
            });

        var completed = false;
        Exception? closeError = null;
        var thread = new Thread(() =>
        {
            try
            {
                Ruby.Close(state);
                completed = true;
            }
            catch (Exception e)
            {
                closeError = e;
            }
        })
        {
            IsBackground = true,
            Name = "release-fn-close-deadlock-probe",
        };

        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "Ruby.Close deadlocked (releaseFn held VmLifecycleLock).");
        Assert.Null(closeError);
        Assert.True(completed, "Ruby.Close did not complete successfully.");
        Assert.Equal(IntPtr.Zero, state.NativeHandler);
    }

    // The releaseFn (lambda) path: the free callback is a captured anonymous delegate
    // stored only via the marshaled data-type struct. Force GC while the object is live,
    // then close -> mrb_close calls dfree -> must not jump through a freed delegate.
    [WindowsOnlyFact]
    public void TestDataObjectWithReleaseFnSurvivesGcThenClose()
    {
        var released = 0L;

        var state = Ruby.Open();
        var cls = state.DefineClass("GcDataReleaseProbe", null);
        cls.DefineMethod("initialize", (stat, self, args) =>
        {
            var v = stat.UnboxInt(args[0]);
            var typeName = self.GetDataObjectType().Name;
            self.GetDataObject<Payload>(typeName)!.Value = v;
            return self;
        }, RbHelper.MRB_ARGS_REQ(1), out _);

        var payload = new Payload();
        cls.NewObjectWithCSharpDataObject(
            "GcDataReleaseProbe",
            payload,
            (_, obj) => released = ((Payload)obj!).Value,
            state.BoxInt(98765));

        // Drop our managed references and force GC while the data object is alive in the
        // VM. If the captured release delegate is not rooted, its thunk can be collected.
        payload = null;
        ForceFullGc();

        // mrb_close runs the final GC sweep, which (under 4.0 MRB_TT_CDATA) calls dfree.
        Ruby.Close(state);

        Assert.Equal(98765, released);
    }

    // The default-free path (static NativeDataObjectFreeFunc method group): create many
    // data objects, force GC, then close. Exercises the GCHandle.Free callback at
    // teardown for many live objects.
    [WindowsOnlyFact]
    public void TestManyDataObjectsSurviveGcThenClose()
    {
        var state = Ruby.Open();
        var cls = state.DefineClass("GcDataManyProbe", null);
        cls.DefineMethod("initialize", (stat, self, args) =>
        {
            var typeName = self.GetDataObjectType().Name;
            self.GetDataObject<Payload>(typeName)!.Value = stat.UnboxInt(args[0]);
            return self;
        }, RbHelper.MRB_ARGS_REQ(1), out _);

        for (var i = 0; i < 100; i++)
        {
            cls.NewObjectWithCSharpDataObject("GcDataManyProbe", new Payload(), state.BoxInt(i));
        }

        ForceFullGc();

        // Must not crash invoking the free callback for every live data object.
        Ruby.Close(state);
    }
}

[CollectionDefinition("DataObjectGcSerial", DisableParallelization = true)]
public sealed class DataObjectGcSerialCollection
{
}
