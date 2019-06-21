class Main inherits IO{
    
    x:Int;
    y:Int;
    p:Int;

    mini(x:Int, y:Int) : Int{
        if x < y then
            x
        else
            y
        fi
    };

    maxi(x:Int, y:Int) : Int{
        if x < y then
            y
        else
            x
        fi
    };

    main() : Int{{
        x <- in_int();
        y <- in_int();

        p <- 
            let z:Int <- maxi(x, y) / mini(x, y) in z;

        out_int(p);
        p;
    }};
};