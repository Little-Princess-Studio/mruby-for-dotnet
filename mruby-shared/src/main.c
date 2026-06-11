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

// ---------------------------------------------------------------------------
// longjmp firewall (macOS native test-host crash fix)
//
// mruby signals errors with setjmp/longjmp (mrb->jmp). .NET does NOT support a
// longjmp crossing a managed frame: it leaves CoreCLR's per-thread explicit-frame
// chain (Thread::m_pFrame) pointing at a dead InlinedCallFrame, and a later GC
// stack-walk dereferences the dangling frame and crashes the process (the same class
// as dotnet/runtime#1445, NLua's lua_error->longjmp).
//
// The guaranteed-bug case is the managed callback bridge raising a Ruby exception by
// calling mrb_raise/mrb_exc_raise FROM INSIDE the managed callback: that throw longjmps
// straight out of the managed frame. The fix is to set the pending exception WITHOUT
// throwing, so the managed callback returns normally and the native VM epilogue observes
// mrb->exc and propagates it from native VM code (below the managed frame).
// ---------------------------------------------------------------------------

// Set the pending exception without longjmp. The managed callback bridge calls this
// instead of mrb_raise/mrb_exc_raise so it can return mrb_nil normally; the VM checks
// mrb->exc after the C function returns and propagates from native code.
//
// mrb_exc_set is a real (non-static) symbol in the statically-linked mruby, but it is
// not declared in any public header, so forward-declare it. It normalizes nil (clears
// mrb->exc) and sets the exception object pointer with the proper write barrier - more
// correct than assigning mrb->exc directly.
void mrb_exc_set(struct mrb_state *mrb, mrb_value exc);

MRB_API void mrbdotnet_set_pending_exception(struct mrb_state *mrb, mrb_value exc) {
  mrb_exc_set(mrb, exc);
}
