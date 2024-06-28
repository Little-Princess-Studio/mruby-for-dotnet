#include "mruby.h"
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

  MRB_API const char* mrb_string_value_unboxing(struct mrb_state* mrb, mrb_value value);
}