namespace GameEngine.Core;

public class ScriptService
{

    enum ScriptInstructions
    {
        // data/memory instructions
        inst_define,
        inst_set,
        inst_select,
        inst_delete,

        // process instructions
        inst_return,
        inst_goto,
        inst_jmp,
        inst_call,
        inst_loop,
        inst_doop,
        inst_donothing,

        // conditional operators
        inst_compare,
        inst_istrue,
        inst_isfalse,
        inst_isnum,
        inst_isnan,
        inst_istext,
        inst_iseqls,
        inst_isgrtn,
        inst_islstn,

        // output instructions
        inst_print,
        inst_write,

        // binary operators
        inst_add,
        inst_sub,
        inst_mul,
        inst_div,
    }

}