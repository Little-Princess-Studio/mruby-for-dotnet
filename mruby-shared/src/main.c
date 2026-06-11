#include "main.h"

// make linker happy
mrb_value mrb_bint_new_int64(struct mrb_state *mrb, int64_t x) {
  return mrb_nil_value();
}

int mrb_msvc_snprintf(char *s, size_t n, const char *format, ...) {
  return 0;
}

int mrb_msvc_vsnprintf(char *s, size_t n, const char *format, va_list arg) {
  return 0;
}

int64_t mrb_bint_as_int64(struct mrb_state *mrb, mrb_value x) { return 0; }

mrb_bool mrb_pool_can_realloc(struct mrb_pool * pool, void * p, size_t size) {
  return FALSE;
}

mrb_value mrb_float_value_boxing(struct mrb_state *mrb, mrb_float f) {
  return mrb_word_boxing_float_value(mrb, f);
}

mrb_value mrb_int_value_boxing(mrb_int i) {
  return mrb_fixnum_value(i);
}

mrb_value mrb_string_value_boxing(struct mrb_state *mrb, const char cstr[]) {
  return mrb_str_new_cstr(mrb, cstr);
}

mrb_value mrb_symbol_value_boxing(mrb_sym i) { return mrb_symbol_value(i); }

mrb_value mrb_nil_value_boxing() { return mrb_nil_value(); }

mrb_value mrb_true_value_boxing() { return mrb_true_value(); }

mrb_value mrb_false_value_boxing() { return mrb_false_value(); }

mrb_value mrb_undef_value_boxing() { return mrb_undef_value(); }

mrb_int mrb_int_value_unboxing(mrb_value value) { return mrb_fixnum(value); }

mrb_float mrb_float_value_unboxing(mrb_value value) { return mrb_float(value); }

mrb_sym mrb_symbol_value_unboxing(mrb_value value) { return mrb_symbol(value); }

const char *mrb_string_value_unboxing(struct mrb_state* mrb, mrb_value value) {
  return mrb_str_to_cstr(mrb, value);
}

mrb_value mrb_ptr_to_mrb_value(void *p) { return mrb_obj_value(p); }

struct RObject* mrb_value_to_obj_ptr(mrb_value value) { return mrb_obj_ptr(value); }

mrb_value mrb_new_data_object(struct mrb_state *mrb, struct RClass *klass, void *datap, struct mrb_data_type *type) {
  MRB_SET_INSTANCE_TT(klass, MRB_TT_CDATA);
  return mrb_obj_value(Data_Wrap_Struct(mrb, klass, type, datap));
}

void *mrb_data_object_get_ptr(struct mrb_state *mrb, mrb_value obj, struct mrb_data_type *type) {
  void *p;
  Data_Get_Struct(mrb, obj, type, p);
  return p;
}

const mrb_data_type *mrb_data_object_get_type(mrb_value obj) { return DATA_TYPE(obj); }

struct RClass *mrb_get_class_ptr(mrb_value value) { return mrb_class_ptr(value); }

mrb_bool mrb_check_frozen_ex(mrb_value o) {
  return mrb_frozen_p(mrb_basic_ptr(o)) ? TRUE : FALSE;
}

mrb_value mrb_get_block(struct mrb_state *mrb) {
  mrb_callinfo *ci = mrb->c->ci;
  mrb_value b = ci->stack[mrb_ci_bidx(ci)];
  return b;
}

void mrb_name_error_ex(mrb_state *mrb, mrb_sym id, const char *msg) {
  mrb_name_error(mrb, id, msg);
}

void mrb_warn_ex(mrb_state *mrb, const char *msg) {
  mrb_warn(mrb, msg);
}

mrb_int mrb_array_len(mrb_value array) { return RARRAY_LEN(array); }

mrb_int mrb_obj_hash(mrb_state *mrb, mrb_value self) {
  mrb_value hash_code = mrb_funcall(mrb, self, "hash", 0);
  return mrb_int(mrb, hash_code);
}

mrb_bool mrb_check_type_integer(mrb_value obj) {
  return mrb_integer_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_symbol(mrb_value obj) {
  return mrb_symbol_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_float(mrb_value obj) {
  return mrb_float_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_array(mrb_value obj) {
  return mrb_array_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_string(mrb_value obj) {
  return mrb_string_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_hash(mrb_value obj) {
  return mrb_hash_p(obj) ? TRUE : FALSE;
}
mrb_bool mrb_check_type_exception(mrb_value obj) {
  return mrb_exception_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_object(mrb_value obj) {
  return mrb_object_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_class(mrb_value obj) {
  return mrb_class_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_moudle(mrb_value obj) {
  return mrb_module_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_sclass(mrb_value obj) {
  return mrb_sclass_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_proc(mrb_value obj) {
  return mrb_proc_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_range(mrb_value obj) {
  return mrb_range_p(obj) ? TRUE : FALSE;
}

mrb_bool mrb_check_type_fiber(mrb_value obj) {
  return mrb_fiber_p(obj) ? TRUE : FALSE;
}

MRB_API void mrb_get_raw_bytes_from_string(mrb_value value, char **bytes,
                                           size_t *len) {
  if (mrb_string_p(value)) {
    *bytes = RSTRING_PTR(value);
    *len = RSTRING_LEN(value);
  } else {
    *bytes = NULL;
    *len = 0;
  }
}

// mruby 4.0: mrb_open() may return a non-NULL state with mrb->exc set on
// initialization failure. MRB_OPEN_FAILURE wraps that check; expose it so the
// managed Ruby.Open() can detect a poisoned state (mrb_state is opaque in C#).
MRB_API mrb_bool mrb_open_failure_p(struct mrb_state *mrb) {
  return MRB_OPEN_FAILURE(mrb) ? TRUE : FALSE;
}

MRB_API void mrb_data_disarm(struct mrb_state *mrb, mrb_value obj) {
  if (mrb_type(obj) == MRB_TT_CDATA) {
    struct RData *d = (struct RData*)mrb_ptr(obj);
    d->type = NULL;
    d->data = NULL;
  }
}

// ===========================================================================
// Native callback trampoline (macOS longjmp-over-managed-frame crash fix).
// See main.h for the full rationale. Core idea: mruby calls a STATIC native
// function; native calls the managed dispatcher, lets it return, then raises
// from native code so the longjmp never crosses the managed frame.
// ===========================================================================

static mrbdotnet_dispatch_fn g_mrbdotnet_dispatch = NULL;

MRB_API void mrbdotnet_set_dispatcher(mrbdotnet_dispatch_fn fn) {
  g_mrbdotnet_dispatch = fn;
}

// Shared core: call the managed dispatcher with the given callbackId and
// (argc, argv); if it signals an error, build a Ruby exception from the message
// and raise it HERE (native frame, after the managed dispatcher has returned).
static mrb_value mrbdotnet_dispatch_and_maybe_raise(
    mrb_state *mrb, mrb_value self, int64_t callback_id,
    int64_t argc, const mrb_value *argv) {
  if (g_mrbdotnet_dispatch == NULL) {
    struct RClass *re = mrb_exc_get_id(mrb, mrb_intern_cstr(mrb, "RuntimeError"));
    mrb_value m = mrb_str_new_cstr(mrb, "mruby-for-dotnet dispatcher not registered");
    mrb_exc_raise(mrb, mrb_exc_new_str(mrb, re, m));
  }

  mrb_bool should_raise = FALSE;
  char msg_buf[1024];
  msg_buf[0] = '\0';

  uint64_t res = g_mrbdotnet_dispatch(
      mrb, self.w, callback_id, argc, (const uint64_t *)argv,
      &should_raise, msg_buf, (int32_t)sizeof(msg_buf));

  if (should_raise) {
    // The managed dispatcher has fully returned; its frame is gone. Build the
    // exception and longjmp from native code below the managed frame.
    //
    // Resolve RuntimeError via RUNTIME symbol intern, NOT the E_RUNTIME_ERROR macro:
    // E_RUNTIME_ERROR expands to MRB_ERROR_SYM(RuntimeError), a COMPILE-TIME presym id.
    // On the macOS universal build this glue is compiled against build/host/include while
    // the per-arch slices link build/<arch>/lib; if the generated presym ids differ, the
    // compile-time id resolves the wrong constant and mrb_exc_get_id raises mruby's own
    // "exception corrupted" sentinel (the arm64-only failure observed). Interning the name
    // at runtime is presym-table-agnostic and correct on every slice.
    msg_buf[sizeof(msg_buf) - 1] = '\0';
    struct RClass *runtime_error = mrb_exc_get_id(mrb, mrb_intern_cstr(mrb, "RuntimeError"));
    mrb_value m = mrb_str_new_cstr(mrb, msg_buf);
    mrb_value exc = mrb_exc_new_str(mrb, runtime_error, m);
    mrb_exc_raise(mrb, exc);
  }

  mrb_value out;
  out.w = res;
  return out;
}

// The single static cfunc handed to mruby for every managed method. Recovers the
// callbackId from the proc env (env[0]) and dispatches.
static mrb_value mrbdotnet_method_trampoline(mrb_state *mrb, mrb_value self) {
  mrb_value idv = mrb_proc_cfunc_env_get(mrb, 0);
  int64_t callback_id = (int64_t)mrb_integer(idv);
  mrb_int argc = 0;
  const mrb_value *argv = NULL;
  mrb_get_args(mrb, "*", &argv, &argc);
  return mrbdotnet_dispatch_and_maybe_raise(mrb, self, callback_id, (int64_t)argc, argv);
}

// Build a proc-backed method (cfunc + env[callbackId]) and return the mrb_method_t.
static mrb_method_t mrbdotnet_make_method(mrb_state *mrb, int64_t callback_id) {
  mrb_value env[1];
  env[0] = mrb_int_value(mrb, (mrb_int)callback_id);
  struct RProc *p = mrb_proc_new_cfunc_with_env(mrb, mrbdotnet_method_trampoline, 1, env);
  mrb_method_t m;
  MRB_METHOD_FROM_PROC(m, p);
  return m;
}

MRB_API void mrbdotnet_define_method_id(mrb_state *mrb, struct RClass *c, mrb_sym mid, int64_t callback_id, mrb_aspec aspec) {
  (void)aspec; // aspec is enforced by mrb_get_args format in user code; proc cfuncs accept "*"
  mrb_method_t m = mrbdotnet_make_method(mrb, callback_id);
  mrb_define_method_raw(mrb, c, mid, m);
}

MRB_API void mrbdotnet_define_private_method_id(mrb_state *mrb, struct RClass *c, mrb_sym mid, int64_t callback_id, mrb_aspec aspec) {
  (void)aspec;
  mrb_method_t m = mrbdotnet_make_method(mrb, callback_id);
  MRB_METHOD_SET_VISIBILITY(m, MRB_METHOD_PRIVATE_FL);
  mrb_define_method_raw(mrb, c, mid, m);
}

MRB_API void mrbdotnet_define_class_method_id(mrb_state *mrb, struct RClass *c, mrb_sym mid, int64_t callback_id, mrb_aspec aspec) {
  (void)aspec;
  mrb_method_t m = mrbdotnet_make_method(mrb, callback_id);
  mrb_define_method_raw(mrb, mrb_singleton_class_ptr(mrb, mrb_obj_value(c)), mid, m);
}

MRB_API void mrbdotnet_define_module_function_id(mrb_state *mrb, struct RClass *c, mrb_sym mid, int64_t callback_id, mrb_aspec aspec) {
  // module_function = both an instance method and a singleton (module) method.
  (void)aspec;
  mrb_method_t mi = mrbdotnet_make_method(mrb, callback_id);
  mrb_define_method_raw(mrb, c, mid, mi);
  mrb_method_t ms = mrbdotnet_make_method(mrb, callback_id);
  mrb_define_method_raw(mrb, mrb_singleton_class_ptr(mrb, mrb_obj_value(c)), mid, ms);
}

MRB_API void mrbdotnet_define_singleton_method_id(mrb_state *mrb, struct RObject *o, mrb_sym mid, int64_t callback_id, mrb_aspec aspec) {
  (void)aspec;
  mrb_method_t m = mrbdotnet_make_method(mrb, callback_id);
  mrb_define_method_raw(mrb, mrb_singleton_class_ptr(mrb, mrb_obj_value(o)), mid, m);
}

MRB_API struct RProc *mrbdotnet_proc_new_with_callback_id(mrb_state *mrb, int64_t callback_id) {
  mrb_value env[1];
  env[0] = mrb_int_value(mrb, (mrb_int)callback_id);
  return mrb_proc_new_cfunc_with_env(mrb, mrbdotnet_method_trampoline, 1, env);
}

// ---- Protect / Ensure / Rescue ----
// These mruby functions take a bare mrb_func_t body (NO env) plus an mrb_value
// data argument. We thread the callbackId(s) through native stack structs and use
// dedicated mrb_func_t bodies that recover the id from a thread-local current-call
// pointer. Because mrb_ensure/mrb_rescue invoke body/handler synchronously (no
// reentrancy across our own protect calls on the same thread between set and use),
// a per-call pointer passed via the data slot is the clean channel: we box the
// native ctx pointer into the data mrb_value as an mrb_cptr and unbox it in the body.

struct mrbdotnet_body_ctx { int64_t id; mrb_value user_data; };

// mrb_func_t body: the ctx pointer is carried in the proc env is unavailable here,
// so it is carried in a cptr passed as the body's `data` (mruby stores `data` and
// passes it back as the cfunc's first stack arg via mrb_get_args? No - mrb_ensure
// passes b_data as the SELF/first arg). mruby calls body(mrb, data_as_self).
static mrb_value mrbdotnet_ctx_body(mrb_state *mrb, mrb_value ctx_as_self) {
  struct mrbdotnet_body_ctx *c = (struct mrbdotnet_body_ctx *)mrb_cptr(ctx_as_self);
  mrb_value ud = c->user_data;
  return mrbdotnet_dispatch_and_maybe_raise(mrb, ud, c->id, 1, &ud);
}

MRB_API mrb_value mrbdotnet_protect(mrb_state *mrb, int64_t body_id, mrb_value data, mrb_bool *error) {
  struct mrbdotnet_body_ctx ctx; ctx.id = body_id; ctx.user_data = data;
  return mrb_protect(mrb, mrbdotnet_ctx_body, mrb_cptr_value(mrb, &ctx), error);
}

MRB_API mrb_value mrbdotnet_ensure(mrb_state *mrb, int64_t body_id, mrb_value b_data, int64_t ensure_id, mrb_value e_data) {
  struct mrbdotnet_body_ctx bctx; bctx.id = body_id; bctx.user_data = b_data;
  struct mrbdotnet_body_ctx ectx; ectx.id = ensure_id; ectx.user_data = e_data;
  return mrb_ensure(mrb, mrbdotnet_ctx_body, mrb_cptr_value(mrb, &bctx),
                    mrbdotnet_ctx_body, mrb_cptr_value(mrb, &ectx));
}

MRB_API mrb_value mrbdotnet_rescue(mrb_state *mrb, int64_t body_id, mrb_value b_data, int64_t rescue_id, mrb_value r_data) {
  struct mrbdotnet_body_ctx bctx; bctx.id = body_id; bctx.user_data = b_data;
  struct mrbdotnet_body_ctx rctx; rctx.id = rescue_id; rctx.user_data = r_data;
  return mrb_rescue(mrb, mrbdotnet_ctx_body, mrb_cptr_value(mrb, &bctx),
                    mrbdotnet_ctx_body, mrb_cptr_value(mrb, &rctx));
}

MRB_API mrb_value mrbdotnet_rescue_exceptions(mrb_state *mrb, int64_t body_id, mrb_value b_data, int64_t rescue_id, mrb_value r_data, struct RClass **classes, mrb_int len) {
  struct mrbdotnet_body_ctx bctx; bctx.id = body_id; bctx.user_data = b_data;
  struct mrbdotnet_body_ctx rctx; rctx.id = rescue_id; rctx.user_data = r_data;
  return mrb_rescue_exceptions(mrb, mrbdotnet_ctx_body, mrb_cptr_value(mrb, &bctx),
                               mrbdotnet_ctx_body, mrb_cptr_value(mrb, &rctx), len, classes);
}
