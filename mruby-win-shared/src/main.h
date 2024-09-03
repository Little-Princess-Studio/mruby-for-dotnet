#include "mruby.h"

#include "mruby/array.h"
#include "mruby/class.h"
#include "mruby/data.h"
#include "mruby/hash.h"
#include "mruby/string.h"

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