.data
	 type_Main: .asciiz "Main"
	 type_Object: .asciiz "Object"
	 type_Int: .asciiz "Int"
	 type_IO: .asciiz "IO"
	 type_String: .asciiz "String"
	 type_Bool: .asciiz "Bool"
	 error_null: .asciiz "Null Reference Exception"
	 error_div0: .asciiz "Divition By Zero Exception"
	 error_indexout: .asciiz "Index Out Range Exception"
	 Object: .word 7, 8, 0, type_Object, Object.abort, Object.type_name, Object.copy
	 Int: .word 7, 8, Object, type_Int, Object.abort, Object.type_name, Object.copy
	 IO: .word 11, 8, Object, type_IO, Object.abort, Object.type_name, Object.copy, IO.out_string, IO.out_int, IO.in_string, IO.in_int
	 String: .word 10, 8, Object, type_String, Object.abort, Object.type_name, Object.copy, String.length, String.concat, String.substr
	 Bool: .word 7, 8, Object, type_Bool, Object.abort, Object.type_name, Object.copy
	 Main: .word 12, 16, IO, type_Main, Object.abort, Object.type_name, Object.copy, IO.out_string, IO.out_int, IO.in_string, IO.in_int, Main.main
.text

	 Main.main:
	 addiu $sp, $sp, -44

	 la $a0, error_null
	 li $t0, 0 
	 lw $t1, 0($fp) 
	 beq $t0, $t1, exception
	 li $t0, 1
	 lw $t1, 0($fp)
	 sw $t0, 8($t1)
	 li $t0, 5
	 lw $t1, 0($fp)
	 sw $t0, 12($t1)
	 li $t0, 1
	 sw $t0, 0($sp)
	 Main.main.Let.while2:
	 lw $t1, 0($fp)
	 lw $t0, 12($t1)
	 sw $t0 12($sp)
	 lw $t0, 0($sp)
	 lw $t1, 12($sp)
	 sge $t2, $t1, $t0
	 sw $t2 16($sp)
	 lw $t0, 16($sp) 
	 bne $t0, $zero, Main.main.Let.dowhile4
	 j Main.main.Let.endwhile5
	 Main.main.Let.dowhile4:
	 lw $t1, 0($fp)
	 lw $t0, 8($t1)
	 sw $t0 20($sp)
	 lw $t0, 20($sp)
	 lw $t1, 0($sp)
	 mul $t2, $t0, $t1
	 sw $t2 24($sp)
	 lw $t0, 24($sp)
	 lw $t1, 0($fp)
	 sw $t0, 8($t1)
	 lw $t0, 0($sp)
	 li $t1, 1
	 add $t2, $t0, $t1
	 sw $t2 28($sp)
	 lw $t0, 28($sp)
	 sw $t0, 0($sp)
	 lw $t0, 28($sp)
	 sw $t0, 8($sp)
	 j Main.main.Let.while2
	 Main.main.Let.endwhile5:
	 lw $t0, 8($sp)
	 sw $t0, 4($sp)
	 lw $t1, 0($fp)
	 lw $t0, 8($t1)
	 sw $t0 36($sp)
	 addiu $sp, $sp, -16
	 sw $ra, 12($sp)
	 sw $fp, 8($sp)
	 lw $t0, 0($fp)
	 sw $t0, 0($sp)
	 lw $t0, 52($sp)
	 sw $t0, 4($sp)
	 move $fp, $sp
	 la $t0, Main
	 lw $t1, 32($t0)
	 jal $t1
	 lw $ra, 12($sp)
	 lw $fp, 8($sp)
	 addiu $sp, $sp, 16
	 sw $v0 32($sp)
	 lw $t1, 0($fp)
	 lw $t0, 8($t1)
	 sw $t0 40($sp)
	 lw $v0 40($sp)
	 addiu $sp, $sp, 44
	 jr $ra


	 IO.out_string:
	 addiu $sp, $sp, -0
	 lw $t0, 4($fp)
	 addiu $a0, $t0 , 8
	 li $v0, 4
	 syscall
	 lw $v0 0($fp)
	 addiu $sp, $sp, 0
	 jr $ra


	 IO.out_int:
	 addiu $sp, $sp, -0
	 li $v0 , 1
	 lw $a0 , 4($fp)
	 syscall
	 lw $v0 0($fp)
	 addiu $sp, $sp, 0
	 jr $ra


	 descend:
	 addiu $sp, $sp, -16
	 la $t0 Object
	 sw $t0 8($sp)
	 li $t0, 0
	 sw $t0, 0($sp)
	 descend.iterate:
	 lw $t0, 0($fp)
	 lw $t1, 4($fp)
	 seq $t2, $t0, $t1
	 sw $t2 4($sp)
	 lw $t0, 4($sp) 
	 bne $t0, $zero, descend.end
	 lw $t0, 4($fp)
	 lw $t1, 8($sp)
	 seq $t2, $t0, $t1
	 sw $t2 4($sp)
	 lw $t0, 4($sp) 
	 bne $t0, $zero, descend.wrong
	 lw $t0, 4($fp)
	 li $t1, 0
	 seq $t2, $t0, $t1
	 sw $t2 4($sp)
	 lw $t0, 4($sp) 
	 bne $t0, $zero, descend.wrong
	 sw $t1, 4($fp)
	 lw $t0, 8($t1)
	 sw $t0, 12($sp)
	 lw $t0, 12($sp)
	 sw $t0, 4($fp)
	 lw $t0, 0($sp)
	 li $t1, 1
	 add $t2, $t0, $t1
	 sw $t2 0($sp)
	 j descend.iterate
	 descend.wrong:
	 li $t0, 10000
	 sw $t0, 0($sp)
	 descend.end:
	 lw $v0 0($sp)
	 addiu $sp, $sp, 16
	 jr $ra


	 Object.abort:
	 addiu $sp, $sp, -0
	li $v0, 10
	 syscall
	 li $v0 0
	 addiu $sp, $sp, 0
	 jr $ra


	 Object.type_name:
	 addiu $sp, $sp, -8
	 lw $t0 0($fp)
	 lw $t1 4($t0)
	 sw $t1 4($sp)
	 lw $t0, 4($sp)
	 lw $a0, 12($t0)
	 li $v0, 4
	 syscall
	 lw $v0 0($sp)
	 addiu $sp, $sp, 8
	 jr $ra


	 String.length:
	 addiu $sp, $sp, -4
	 lw $v0, 0($fp)
	 lw $v1, 0($v0)
	 addi $v1, $v1, -8
	 sw $v1, 0($sp)
	 lw $v0 0($sp)
	 addiu $sp, $sp, 4
	 jr $ra


	 String.concat:
	 addiu $sp, $sp, -8
	 lw $t0 0($fp)
	 lw $t0 0($t0)
	 li $a0 0
	 lw $t1 4($fp)
	 lw $t1 0($t1)
	 add $a0 $t0 $t1
	 addi $a0 $a0 -8
	 li $v0 9
	 syscall
	 sw $v0 4($sp)
	 la $t2 String
	 sw $a0 0($v0)
	 sw $t2 4($v0)
	 lw $t0 0($fp)
	 lw $t1 4($fp)
	 lw $t2 0($t0)
	 addi $t2 $t2 -8
	 lw $t4 0($t1)
	 addi $t4 $t4 -8
	 addi $t0 $t0 8
	 addi $t1 $t1 8
	 addi $v0 $v0 8
	 .concat1: 
	 beq $t2 $zero .endconcat1
	 lb $t3 0($t0)
	 sb $t3 0($v0)
	 addi $t2, $t2, -1
	 addi $t0, 1
	 addi $v0, 1
	 j .concat1
	 .endconcat1: 
	 move $t2 $t4
	 .concat2: 
	 beq $t2 $zero .endconcat2
	 lb $t3 0($t1)
	 sb $t3 0($v0)
	 addi $t2, $t2, -1
	 addi $t1, 1
	 addi $v0, 1
	 j .concat2
	 .endconcat2: 
	 lw $t0, 4($sp)
	 sw $t0, 0($fp)
	 lw $v0 0($fp)
	 addiu $sp, $sp, 8
	 jr $ra


	 Object.copy:
	 addiu $sp, $sp, -8
	 lw $t0 0($fp)
	 lw $t1 4($t0)
	 sw $t1 4($sp)
	 sw $t0, 4($sp)
	 lw $a0, 0($t0)
	 li $v0, 9
	 syscall
	 sw $v0 0($sp)
	 sw $a0, 0($v0)
	 sw $t0, 4($v0)
	 lw $t0 0($fp)
	 lw $t2 0($t0)
	 lw $t1 0($sp)
	 .cop: 
	 beq $t2 $zero .endcop
	 lb $t3 0($t0)
	 sb $t3 0($t1)
	 addi $t2, $t2, -1
	 addi $t0, 1
	 addi $t1, 1
	 j .cop
	 .endcop: 
	 lw $v0 0($sp)
	 addiu $sp, $sp, 8
	 jr $ra


	 String.substr:
	 addiu $sp, $sp, -4
	 lw $t0 0($fp)
	 lw $t0 0($t0)
	 addi $t0 $t0 -8
	 lw $t1 4($fp)
	 lw $t2 8($fp)
	 lw $t3 0($fp)
	 move $a0 $t1
	 add $a0 $a0 $t2
	 sle $t4, $a0, $t0
	 la $a0 error_indexout
	 beq $t4, $zero, exception
	 lw $t0 0($fp)
	 lw $t0 0($t0)
	 lw $t3 0($fp)
	 li $a0 0
	 addi $a0 $t2 0
	 addi $a0 $a0 8
	 li $v0 9
	 syscall
	 sw $v0 0($sp)
	 la $t2 String
	 sw $a0 0($v0)
	 sw $t2 4($v0)
	 lw $t0 0($fp)
	 lw $t1 4($fp)
	 addi $t0, $t0, 8
	 addi $v0, $v0, 8
	 lw $t1 4($fp)
	 lw $t2 8($fp)
	 add $t0, $t0, $t1
	 .sub: 
	 beq $t2 $zero .endsub
	 lb $t3 0($t0)
	 sb $t3 0($v0)
	 addi $t2, $t2, -1
	 addi $t0, 1
	 addi $v0, 1
	 j .sub
	 .endsub: 
	 lw $v0 0($sp)
	 addiu $sp, $sp, 4
	 jr $ra


	 IO.in_int:
	 addiu $sp, $sp, -4
	 li $v0, 5
	 syscall
	 sw $v0 0($sp)
	 lw $v0 0($sp)
	 addiu $sp, $sp, 4
	 jr $ra


	 main:
	 addiu $sp, $sp, -4
	 la $t0, Main
	 lw $a0, 4($t0)
	 li $v0, 9
	 syscall
	 sw $v0 0($sp)
	 sw $a0, 0($v0)
	 sw $t0, 4($v0)
	 addiu $sp, $sp, -12
	 sw $ra, 8($sp)
	 sw $fp, 4($sp)
	 lw $t0, 12($sp)
	 sw $t0, 0($sp)
	 move $fp, $sp
	 la $t0, Main
	 lw $t1, 44($t0)
	 jal $t1
	 lw $ra, 8($sp)
	 lw $fp, 4($sp)
	 addiu $sp, $sp, 12
	 sw $v0 0($sp)



        li $v0, 10
        syscall

        exception:
            li $v0, 4
            syscall
            li $v0, 10
            syscall
