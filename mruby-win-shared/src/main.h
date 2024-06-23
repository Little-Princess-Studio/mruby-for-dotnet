#include "mruby.h"

extern "C" {
  MRB_API mrb_value mrb_float_value_boxing(struct mrb_state *mrb, mrb_float f);

  MRB_API mrb_value mrb_int_value_boxing(struct mrb_state *mrb, mrb_int i);

  MRB_API mrb_value mrb_symbol_value_boxing(mrb_sym i);

  MRB_API mrb_value mrb_nil_value_boxing();

  MRB_API mrb_value mrb_true_value_boxing();

  MRB_API mrb_value mrb_false_value_boxing();

  MRB_API mrb_value mrb_undef_value_boxing();
}