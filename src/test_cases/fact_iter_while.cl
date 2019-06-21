
class Main inherits IO{
	
	z:Int <- 1;
	y:Int <- 5;

	main() : Int {{
		
		let i:Int <- 1 in
			while i <= y loop{
				z <- z * i;
				i <- i + 1;
			}
			pool; 
		out_int(z);
		z;
	}};
};