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

    attribute x;
    attribute y;
    attribute p;

    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;
    method out_string : IO.out_string;
    method out_int : IO.out_int;
    method in_string : IO.in_string;
    method in_int : IO.in_int;
    method mini : Main.mini;
    method maxi : Main.maxi;
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

function Main.mini{
    ARG self;
    ARG x;
    ARG y;

    LOCAL Main.mini.exp0;
    LOCAL Main.mini.resultif1;

    if self = 0 ERROR : [error_null] ;
    Main.mini.exp0 = x < y;
    if Main.mini.exp0 jmp Main.mini.if2;
    Main.mini.resultif1 = y;
    jmp Main.mini.endif3;
    #Main.mini.if2;
    Main.mini.resultif1 = x;
    #Main.mini.endif3;
    RETURN Main.mini.resultif1;
}

function Main.maxi{
    ARG self;
    ARG x;
    ARG y;

    LOCAL Main.maxi.exp0;
    LOCAL Main.maxi.resultif1;

    if self = 0 ERROR : [error_null] ;
    Main.maxi.exp0 = x < y;
    if Main.maxi.exp0 jmp Main.maxi.if2;
    Main.maxi.resultif1 = x;
    jmp Main.maxi.endif3;
    #Main.maxi.if2;
    Main.maxi.resultif1 = y;
    #Main.maxi.endif3;
    RETURN Main.maxi.resultif1;
}

function Main.main{
    ARG self;

    LOCAL Main.main.exp0;
    LOCAL Main.main.exp1;
    LOCAL Main.main.exp2;
    LOCAL Main.main.get_attr3;
    LOCAL Main.main.get_attr4;
    LOCAL Main.main.exp5;
    LOCAL Main.main.get_attr6;
    LOCAL Main.main.get_attr7;
    LOCAL Main.main.exp8;
    LOCAL Main.main.Let.z0;
    LOCAL Main.main.Let.exp1;
    LOCAL Main.main.exp9;
    LOCAL Main.main.get_attr10;
    LOCAL Main.main.get_attr11;

    if self = 0 ERROR : [error_null] ;
    SETATTR self x 0 (Main);
    SETATTR self y 0 (Main);
    SETATTR self p 0 (Main);
    
    ARG self;
    Main.main.exp0 = vcall Main in_int(Main);
    SETATTR self x Main.main.exp0 (Main);
    
    ARG self;
    Main.main.exp1 = vcall Main in_int(Main);
    SETATTR self y Main.main.exp1 (Main);
    Main.main.get_attr3 = GETATTR self x (Main);
    Main.main.get_attr4 = GETATTR self y (Main);
    
    ARG self;
    ARG Main.main.get_attr3;
    ARG Main.main.get_attr4;
    Main.main.exp2 = vcall Main maxi(Main);
    Main.main.get_attr6 = GETATTR self x (Main);
    Main.main.get_attr7 = GETATTR self y (Main);
    
    ARG self;
    ARG Main.main.get_attr6;
    ARG Main.main.get_attr7;
    Main.main.exp5 = vcall Main mini(Main);
    if Main.main.exp5 = 0 ERROR : [error_div0] ;
    Main.main.exp8 = Main.main.exp2 / Main.main.exp5;
    Main.main.Let.z0 = Main.main.exp8;
    Main.main.Let.exp1 = Main.main.Let.z0;
    SETATTR self p Main.main.Let.exp1 (Main);
    Main.main.get_attr10 = GETATTR self p (Main);
    
    ARG self;
    ARG Main.main.get_attr10;
    Main.main.exp9 = vcall Main out_int(Main);
    Main.main.get_attr11 = GETATTR self p (Main);
    RETURN Main.main.get_attr11;
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



