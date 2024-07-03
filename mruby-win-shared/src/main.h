#include "mruby.h"
#include "mruby/class.h"
#include "mruby/data.h"
#include "mruby/string.h"

extern "C" {
  MRB_API mrb_value mrb_float_value_boxing(struct mrb_state *mrb, mrb_float f);

  MRB_API mrb_value mrb_int_value_boxing(struct mrb_state *mrb, mrb_int i);

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

  MRB_API mrb_value mrb_new_data_object(mrb_state *mrb, RClass *klass, void *datap, mrb_data_type *type);

  MRB_API void *mrb_data_object_get_ptr(mrb_state *mrb, mrb_value obj,
                                        mrb_data_type *type);

  MRB_API void *mrb_data_object_get_type(mrb_value obj);

  MRB_API bool mrb_exception_happened(mrb_state *mrb);

  MRB_API void mrb_print_error_ex(mrb_state *mrb);

  MRB_API RClass *mrb_get_class_ptr(mrb_value value);

  MRB_API RObject *mrb_value_to_obj_ptr(mrb_value value);
}