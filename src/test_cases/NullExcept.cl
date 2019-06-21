class A{
    f():Int{
        5
    };
};

class Main inherits IO{
    x:A;
    y:Int;
    z:Int <- 0;
    main() : Int{{
        y <- in_int();
        z <- y + x.f();
        out_int(z);
        z;
    }};
};