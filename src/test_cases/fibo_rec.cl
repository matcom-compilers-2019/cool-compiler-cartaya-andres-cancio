class Main inherits IO{
    y:Int;
	fibo(x:Int) : Int{
		if (x = 0) then
			1
		else
			if (x = 1) then
				1
			else
				fibo(x - 1) + fibo(x - 2)
			fi
		fi
	};
	main() : Int {{
		y <- fibo(7);
		out_int(y);
		y;
	}};
};