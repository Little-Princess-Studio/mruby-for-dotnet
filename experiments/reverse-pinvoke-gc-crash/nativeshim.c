/*
 * nativeshim.c - a deliberately trivial native shim with ZERO dependencies.
 *
 * There is NO mruby, NO Lua, NO library here -- just the bare native->managed
 * (reverse-P/Invoke) boundary, crossed under GC pressure. The point: prove
 * (or falsify) that the macOS CI crash lives in the CoreCLR reverse-P/Invoke +
 * GC-suspension machinery, independent of mruby.
 *
 * Two modes:
 *   (1) run_callback_loop  -- a tight idle loop calling a callback. Tests the
 *       PURE mode-flip/transition window. (v1: did NOT crash, 180/180 clean.)
 *   (2) fake_open/register/close -- a fake VM lifecycle whose `fake_close` fires
 *       managed callbacks NESTED DEEP inside native teardown work, mimicking
 *       mruby's mrb_close -> GC sweep -> dfree reverse-callback, driven under
 *       tight open/close churn. (v2: the structural shape the real storm has.)
 */
#include <stdint.h>
#include <stdlib.h>

typedef void (*callback_t)(void);
typedef void (*dfree_t)(intptr_t obj);

#if defined(_WIN32)
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif

/*
 * Mode 1 (v1): one managed->native P/Invoke gets us in here; then every cb() is
 * a native->managed reverse-P/Invoke. The worker thread oscillates preemptive
 * <->cooperative GC mode `iterations` times. Each oscillation pushes/pops the
 * explicit transition frame the GC stack-walker depends on -- a sample of the
 * pure suspension window.
 */
EXPORT void run_callback_loop(callback_t cb, int64_t iterations)
{
    for (int64_t i = 0; i < iterations; i++)
    {
        cb();
    }
}

/* ---- Mode 2 (v2): fake VM lifecycle with dfree nested inside close ---- */
typedef struct fake_state
{
    dfree_t  dfree;
    intptr_t *objs;
    int64_t  count;
    int64_t  cap;
} fake_state;

EXPORT void *fake_open(void)
{
    fake_state *st = (fake_state *)calloc(1, sizeof(fake_state));
    st->cap = 64;
    st->objs = (intptr_t *)calloc((size_t)st->cap, sizeof(intptr_t));
    return st;
}

EXPORT void fake_register(void *state, dfree_t dfree, intptr_t obj)
{
    fake_state *st = (fake_state *)state;
    st->dfree = dfree;
    if (st->count == st->cap)
    {
        st->cap *= 2;
        st->objs = (intptr_t *)realloc(st->objs, (size_t)st->cap * sizeof(intptr_t));
    }
    st->objs[st->count++] = obj;
}

/*
 * Mimic mrb_close: while tearing the VM down in NATIVE code, walk every live
 * "data object" and call its managed dfree (reverse-P/Invoke) -- so a GC
 * suspension signal that lands here hits a managed thread parked DEEP inside
 * native teardown, the exact shape of the real storm crash.
 */
EXPORT void fake_close(void *state)
{
    fake_state *st = (fake_state *)state;
    for (int64_t i = 0; i < st->count; i++)
    {
        if (st->dfree)
        {
            st->dfree(st->objs[i]);
        }
    }
    free(st->objs);
    free(st);
}
