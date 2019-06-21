
class A{
	f() : Int{
        2
	};
};

class B inherits A{
	f() : Int{
		3
	};
};

class Main inherits IO{
    
	x : B <- new B;
	y:Int;

	main() : Int {{
		y <- x.f();
		out_int(y);
		y;
	}};
};