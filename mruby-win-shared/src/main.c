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
  return mrb_obj_value(Data_Wrap_Struct(mrb, klass, type, datap));
}

void *mrb_data_object_get_ptr(struct mrb_state *mrb, mrb_value obj, struct mrb_data_type *type) {
  void *p;
  Data_Get_Struct(mrb, obj, type, p);
  return p;
}

void *mrb_data_object_get_type(mrb_value obj) { return DATA_PTR(obj); }

mrb_bool mrb_exception_happened(struct mrb_state *mrb) {
  return mrb->exc != NULL;
}

void mrb_print_error_ex(struct mrb_state* mrb) {
    mrb_print_error(mrb);
}

struct RClass *mrb_get_class_ptr(mrb_value value) { return mrb_class_ptr(value); }

mrb_bool mrb_bool_true() { return TRUE; }
mrb_bool mrb_bool_false() { return FALSE; }
