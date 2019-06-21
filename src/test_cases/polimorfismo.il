DATA:

type_A : A
type_B : B
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

type A {


    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;
    method f : A.f;

}

type B {


    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;
    method f : B.f;

}

type Main {

    attribute x;
    attribute y;

    method abort : Object.abort;
    method type_name : Object.type_name;
    method copy : Object.copy;
    method out_string : IO.out_string;
    method out_int : IO.out_int;
    method in_string : IO.in_string;
    method in_int : IO.in_int;
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

function A.f{
    ARG self;


    if self = 0 ERROR : [error_null] ;
    RETURN 2;
}

function B.f{
    ARG self;


    if self = 0 ERROR : [error_null] ;
    RETURN 3;
}

function Main.main{
    ARG self;

    LOCAL Main.main.exp0;
    LOCAL Main.main.exp1;
    LOCAL Main.main.get_attr2;
    LOCAL Main.main.get_typ3;
    LOCAL Main.main.exp4;
    LOCAL Main.main.get_attr5;
    LOCAL Main.main.get_attr6;

    if self = 0 ERROR : [error_null] ;
    Main.main.exp0 = ALLOCATE B;
    SETATTR self x Main.main.exp0 (Main);
    SETATTR self y 0 (Main);
    Main.main.get_attr2 = GETATTR self x (Main);
    if Main.main.get_attr2 = 0 ERROR : [error_null] ;
    Main.main.get_typ3 = TYPEOF Main.main.get_attr2;
    
    ARG Main.main.get_attr2;
    Main.main.exp1 = vcall Main.main.get_typ3 f(B);
    SETATTR self y Main.main.exp1 (Main);
    Main.main.get_attr5 = GETATTR self y (Main);
    
    ARG self;
    ARG Main.main.get_attr5;
    Main.main.exp4 = vcall Main out_int(Main);
    Main.main.get_attr6 = GETATTR self y (Main);
    RETURN Main.main.get_attr6;
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



