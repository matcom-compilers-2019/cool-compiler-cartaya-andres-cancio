DATA:

type_Main : Main
type_Object : Object
type_Int : Int
type_IO : IO
type_String : String
type_Bool : Bool
error_null : Null Reference Exception
error_div0 : Divition By Zero Exception
error_indexout : Index Out Range Exception

TYPES:

type Main {

    attribute y;

    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;
    method out_string : IO.out_string;
    method out_int : IO.out_int;
    method in_string : IO.in_string;
    method in_int : IO.in_int;
    method fibo : Main.fibo;
    method main : Main.main;

}

type Object {


    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;

}

type Int {


    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;

}

type IO {


    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;
    method out_string : IO.out_string;
    method out_int : IO.out_int;
    method in_string : IO.in_string;
    method in_int : IO.in_int;

}

type String {


    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;
    method length : String.length;
    method concat : String.concat;
    method substr : String.substr;

}

type Bool {


    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;

}


FUNCTION:

function Main.fibo{
    ARG self;
    ARG x;

    LOCAL Main.fibo.exp0;
    LOCAL Main.fibo.resultif1;
    LOCAL Main.fibo.exp4;
    LOCAL Main.fibo.resultif5;
    LOCAL Main.fibo.exp8;
    LOCAL Main.fibo.exp9;
    LOCAL Main.fibo.exp10;
    LOCAL Main.fibo.exp11;
    LOCAL Main.fibo.exp12;

    if self = 0 ERROR : [error_null] ;
    Main.fibo.exp0 = x = 0;
    if Main.fibo.exp0 jmp Main.fibo.if2;
    Main.fibo.exp4 = x = 1;
    if Main.fibo.exp4 jmp Main.fibo.if6;
    Main.fibo.exp9 = x - 1;
    
    ARG self;
    ARG Main.fibo.exp9;
    Main.fibo.exp8 = vcall Main fibo(Main);
    Main.fibo.exp11 = x - 2;
    
    ARG self;
    ARG Main.fibo.exp11;
    Main.fibo.exp10 = vcall Main fibo(Main);
    Main.fibo.exp12 = Main.fibo.exp8 + Main.fibo.exp10;
    Main.fibo.resultif5 = Main.fibo.exp12;
    jmp Main.fibo.endif7;
    #Main.fibo.if6;
    Main.fibo.resultif5 = 1;
    #Main.fibo.endif7;
    Main.fibo.resultif1 = Main.fibo.resultif5;
    jmp Main.fibo.endif3;
    #Main.fibo.if2;
    Main.fibo.resultif1 = 1;
    #Main.fibo.endif3;
    RETURN Main.fibo.resultif1;
}

function Main.main{
    ARG self;

    LOCAL Main.main.exp0;
    LOCAL Main.main.exp1;
    LOCAL Main.main.get_attr2;
    LOCAL Main.main.get_attr3;

    if self = 0 ERROR : [error_null] ;
    SETATTR self y 0 (Main);
    
    ARG self;
    ARG 7;
    Main.main.exp0 = vcall Main fibo(Main);
    SETATTR self y Main.main.exp0 (Main);
    Main.main.get_attr2 = GETATTR self y (Main);
    
    ARG self;
    ARG Main.main.get_attr2;
    Main.main.exp1 = vcall Main out_int(Main);
    Main.main.get_attr3 = GETATTR self y (Main);
    RETURN Main.main.get_attr3;
}

function IO.out_string{
    ARG self;
    ARG x;


    PRINT x;
    RETURN self;
}

function IO.out_int{
    ARG self;
    ARG x;


    PRINT x;
    RETURN self;
}

function descend{
    ARG s;
    ARG p;

    LOCAL result;
    LOCAL exp0;
    LOCAL obj;
    LOCAL p2;

    obj = TYPEOF Object;
    result = 0;
    #descend.iterate;
    exp0 = s = p;
    if exp0 jmp descend.end;
    exp0 = p = obj;
    if exp0 jmp descend.wrong;
    exp0 = p = 0;
    if exp0 jmp descend.wrong;
    p2 = FATHERTYPE p;
    p = p2;
    result = result + 1;
    jmp descend.iterate;
    #descend.wrong;
    result = 10000;
    #descend.end;
    RETURN result;
}

function Object.abort{
    ARG self;


    Cool_Compiler.AST.CIL.CIL_Abort;
    RETURN 0;
}

function Object.type_name{
    ARG self;

    LOCAL res;
    LOCAL t;

    t = TYPEOF self;
    res = TYPENAME t;
    RETURN res;
}

function String.length{
    ARG self;

    LOCAL x;

    Cool_Compiler.AST.CIL.CIL_LengthStr;
    RETURN x;
}

function String.concat{
    ARG self;
    ARG other;

    LOCAL res;
    LOCAL salva;

    Cool_Compiler.AST.CIL.CIL_StrConcat;
    self = salva;
    RETURN self;
}

function Object.copy{
    ARG self;

    LOCAL res;
    LOCAL t;

    t = TYPEOF self;
    res = ALLOCATE t;
    Cool_Compiler.AST.CIL.CIL_Copy;
    RETURN res;
}

function String.substr{
    ARG self;
    ARG i;
    ARG j;

    LOCAL x;

    Cool_Compiler.AST.CIL.CIL_StrStr;
    RETURN x;
}

function IO.in_int{
    ARG self;

    LOCAL x;

    x = READ_INT;
    RETURN x;
}

function main{

    LOCAL self;

    self = ALLOCATE Main;
    
    ARG self;
    self = vcall Main main(Main);
}



