namespace GameEngine.Core.Scripting;

public enum DrasmOperations : byte
{
    // data/memory instructions
    op_define,
    op_set,
    op_select,
    op_delete,

    // process instructions
    op_return,
    op_goto,
    op_jmp,
    op_call,
    op_loop,
    op_doop,
    op_break,
    op_nop,

    // conditional operators
    op_compare,
    op_istrue,
    op_isfalse,
//  op_isnum,
//  op_isnan,
//  op_istext,
    op_iseqls,
    op_isneqls,
    op_isgrtn,
    op_islstn,
    op_istype,

    // output instructions
    op_print,
    op_write,

    // binary operators
    op_add,
    op_sub,
    op_mul,
    op_div,
    op_rem,

    // misc
    def_label
}

public enum DrasmParameterTypes : byte
{
    pt_identifier,
    pt_null,
    pt_string,
    pt_boolean,

    // numbers
    pt_number,
    pt_number_byte,
    pt_number_int,
    pt_number_single,
    pt_number_double,

    // arrays
    pt_array,
    pt_arrayIndex,
    pt_arrayLength,
    pt_arrayList,

    // build-in data
    pt_data_returnValue,

    _undefined

}
