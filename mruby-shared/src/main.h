#include "mruby.h"

#include "mruby/array.h"
#include "mruby/class.h"
#include "mruby/data.h"
#include "mruby/hash.h"
#include "mruby/string.h"
#include "mruby/internal.h"
#include "mruby/proc.h"
#include "mruby/error.h"

MRB_API mrb_value mrb_float_value_boxing(struct mrb_state *mrb, mrb_float f);

MRB_API mrb_value mrb_int_value_boxing(mrb_int i);

MRB_API mrb_value mrb_symbol_value_boxing(mrb_sym i);

MRB_API mrb_value mrb_nil_value_boxing();

MRB_API mrb_value mrb_true_value_boxing();

MRB_API mrb_value mrb_false_value_boxing();

MRB_API mrb_value mrb_undef_value_boxing();

MRB_API mrb_int mrb_int_value_unboxing(mrb_value value);

MRB_API mrb_float mrb_float_value_unboxing(mrb_value value);

MRB_API mrb_sym mrb_symbol_value_unboxing(mrb_value value);

MRB_API const char *mrb_string_value_unboxing(struct mrb_state *mrb,
                                              mrb_value value);

MRB_API mrb_value mrb_ptr_to_mrb_value(void *p);

MRB_API mrb_value mrb_new_data_object(struct mrb_state *mrb, struct RClass *klass, void *datap, struct mrb_data_type *type);

MRB_API void *mrb_data_object_get_ptr(struct mrb_state *mrb, mrb_value obj,
                                      struct mrb_data_type *type);

MRB_API const mrb_data_type *mrb_data_object_get_type(mrb_value obj);

MRB_API struct RClass *mrb_get_class_ptr(mrb_value value);

MRB_API struct RObject *mrb_value_to_obj_ptr(mrb_value value);

MRB_API mrb_bool mrb_check_frozen_ex(mrb_value o);

MRB_API mrb_value mrb_get_block(struct mrb_state *mrb);

MRB_API mrb_noreturn void mrb_name_error_ex(mrb_state *mrb, mrb_sym id,
                                            const char *msg);

MRB_API void mrb_warn_ex(mrb_state *mrb, const char *msg);

MRB_API mrb_int mrb_array_len(mrb_value array);

MRB_API mrb_int mrb_obj_hash(mrb_state *mrb, mrb_value self);

MRB_API mrb_bool mrb_check_type_integer(mrb_value obj);
MRB_API mrb_bool mrb_check_type_symbol(mrb_value obj);
MRB_API mrb_bool mrb_check_type_float(mrb_value obj);
MRB_API mrb_bool mrb_check_type_array(mrb_value obj);
MRB_API mrb_bool mrb_check_type_string(mrb_value obj);
MRB_API mrb_bool mrb_check_type_hash(mrb_value obj);
MRB_API mrb_bool mrb_check_type_exception(mrb_value obj);
MRB_API mrb_bool mrb_check_type_object(mrb_value obj);
MRB_API mrb_bool mrb_check_type_class(mrb_value obj);
MRB_API mrb_bool mrb_check_type_moudle(mrb_value obj);
MRB_API mrb_bool mrb_check_type_sclass(mrb_value obj);
MRB_API mrb_bool mrb_check_type_proc(mrb_value obj);
MRB_API mrb_bool mrb_check_type_range(mrb_value obj);
MRB_API mrb_bool mrb_check_type_fiber(mrb_value obj);

MRB_API void mrb_get_raw_bytes_from_string(mrb_value value, char **bytes,
                                           size_t *len);

MRB_API mrb_bool mrb_open_failure_p(struct mrb_state *mrb);

MRB_API void mrb_data_disarm(struct mrb_state *mrb, mrb_value obj);

// ===========================================================================
// Native callback trampoline (macOS longjmp-over-managed-frame crash fix)
//
// Instead of handing mruby a managed (C#) delegate function pointer directly,
// every managed callback is registered as a proc-backed cfunc whose env[0] holds
// an integer callbackId, with a single STATIC native trampoline as the cfunc.
// When mruby invokes a method, it calls the native trampoline; the trampoline
// reads callbackId from the proc env, calls the one rooted managed dispatcher,
// lets it FULLY RETURN, and only THEN - with the managed frame already popped -
// calls mrb_exc_raise from native code. The longjmp therefore originates BELOW
// the managed frame and never crosses it (the dotnet/runtime#1445 crash class).
//
// The managed dispatcher never raises: on a C# exception it writes a UTF-8
// message into the native-provided buffer, sets *shouldRaise=1, and returns nil;
// the native side builds the Ruby exception and raises it.
// ===========================================================================

// Managed dispatcher function pointer type (implemented in C#). Returns the
// callback's mrb_value result. On error it sets *should_raise=1 and fills
// msg_buf (UTF-8, NUL-terminated within msg_buf_len); native then raises.
typedef uint64_t (*mrbdotnet_dispatch_fn)(
    struct mrb_state *mrb, uint64_t self, int64_t callback_id,
    int64_t argc, const uint64_t *argv,
    mrb_bool *should_raise, char *msg_buf, int32_t msg_buf_len);

// Register the single managed dispatcher (called once at process/state init).
MRB_API void mrbdotnet_set_dispatcher(mrbdotnet_dispatch_fn fn);

// Proc-backed method definitions carrying a callbackId in env. These replace the
// raw mrb_define_*_id calls for managed callbacks.
MRB_API void mrbdotnet_define_method_id(struct mrb_state *mrb, struct RClass *c, mrb_sym mid, int64_t callback_id, mrb_aspec aspec);
MRB_API void mrbdotnet_define_private_method_id(struct mrb_state *mrb, struct RClass *c, mrb_sym mid, int64_t callback_id, mrb_aspec aspec);
MRB_API void mrbdotnet_define_class_method_id(struct mrb_state *mrb, struct RClass *c, mrb_sym mid, int64_t callback_id, mrb_aspec aspec);
MRB_API void mrbdotnet_define_module_function_id(struct mrb_state *mrb, struct RClass *c, mrb_sym mid, int64_t callback_id, mrb_aspec aspec);
MRB_API void mrbdotnet_define_singleton_method_id(struct mrb_state *mrb, struct RObject *o, mrb_sym mid, int64_t callback_id, mrb_aspec aspec);

// Proc-backed cfunc proc carrying a callbackId (replaces mrb_proc_new_cfunc_with_env for NewProc).
MRB_API struct RProc *mrbdotnet_proc_new_with_callback_id(struct mrb_state *mrb, int64_t callback_id);

// Protect/Ensure/Rescue wrappers that take callbackId(s) and route the body through
// the trampoline + dispatcher (so a body raise originates in native code).
MRB_API mrb_value mrbdotnet_protect(struct mrb_state *mrb, int64_t body_id, mrb_value data, mrb_bool *error);
MRB_API mrb_value mrbdotnet_ensure(struct mrb_state *mrb, int64_t body_id, mrb_value b_data, int64_t ensure_id, mrb_value e_data);
MRB_API mrb_value mrbdotnet_rescue(struct mrb_state *mrb, int64_t body_id, mrb_value b_data, int64_t rescue_id, mrb_value r_data);
MRB_API mrb_value mrbdotnet_rescue_exceptions(struct mrb_state *mrb, int64_t body_id, mrb_value b_data, int64_t rescue_id, mrb_value r_data, struct RClass **classes, mrb_int len);
