/*
 * nativeshim.c - a deliberately trivial native shim with ZERO dependencies.
 *
 * It does exactly one thing: call a function pointer (a CLR delegate) in a
 * tight loop. There is NO mruby, NO Lua, NO library here at all -- just the
 * bare native->managed (reverse-P/Invoke) boundary, crossed millions of times.
 *
 * The whole point: prove that the macOS crash lives in the CoreCLR reverse-
 * P/Invoke + GC-suspension machinery, independent of mruby.
 */
#include <stdint.h>

typedef void (*callback_t)(void);

#if defined(_WIN32)
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif

/*
 * One managed->native P/Invoke gets us in here; then every cb() is a
 * native->managed reverse-P/Invoke. The worker thread therefore oscillates
 * preemptive<->cooperative GC mode `iterations` times per call. Each
 * oscillation pushes/pops the explicit transition frame the GC stack-walker
 * depends on -- i.e. each one is a sample of the suspension window.
 */
EXPORT void run_callback_loop(callback_t cb, int64_t iterations)
{
    for (int64_t i = 0; i < iterations; i++)
    {
        cb();
    }
}
