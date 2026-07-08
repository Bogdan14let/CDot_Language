cmd math.floor 0f
	; 0f - source number
	rnd 0f 0f 1f 
	
	jf 0f < 0 floor_negative
	
	jmp floor_end

	floor_negative:
		sub 0f 1f 2f 
		
		jf 2f == 0 floor_end
		
		sub 1f 1 1f

	floor_end:
ret

cmd math.ceil 0f
	; 0f - source number
	rnd 0f 0f 1f
	inc 1f 1f
	
	jf 0f < 0 ceil_negative
	
	jmp ceil_end

	ceil_negative:
		sub 0f 1f 2f 
		
		jf 2f == 0 ceil_end
		
		add 1f 1 1f

	ceil_end:
ret

cmd math.round 0f
	; 0f - source number
	rnd 0f 0f 1f
	sub 0f 1f 1f

	jf 1f == 0 notRound
	jf 1f > 0.5 roundUp
	jf 1f == 0.5 roundUp
	jf 1f < 0.5 roundDown

	roundUp:
		rnd 0f 0f 1f 
		inc 1f 1f
		
		jf 0f < 0 c_negative
		
		jmp c_end

		c_negative:
			sub 0f 1f 2f 
			
			jf 2f == 0 c_end
			
			add 1f 1 1f

		c_end:
			jmp roundEnd

	roundDown:
		rnd 0f 0f 1f 
	
		jf 0f < 0 f_negative
		
		jmp f_end

		f_negative:
			sub 0f 1f 2f 
			
			jf 2f == 0 f_end
			
			sub 1f 1 1f

		f_end:
			jmp roundEnd

	notRound:
		mov 1f 0f

	roundEnd:
ret

cmd math.pi
	mov 1f 3.14159265358979323846
ret

cmd math.sqrt 0f
	; 0f - source number
	jf 0f < 0 sqrt_zero
	jf 0f == 0 sqrt_zero

	div 0f 2 1f 
	mov 3f 0 ; i

	sqrt_fast_loop:
		div 0f 1f 2f      ; 2f = number / x
		add 1f 2f 2f      ; 2f = x + (number / x)
		div 2f 2 1f       ; 1f = (x + (number / x)) / 2 (new number)

		inc 3f 3f
		jf 3f < 1000 sqrt_fast_loop
		
	jmp sqrt_end

	sqrt_zero:
		mov 1f 0

	sqrt_end:
		mov 2f 0
		mov 3f 0
ret

cmd math.rad 0f
	call [] math.pi
	mul 0f 1f 1f
	div 1f 180 1f
ret

cmd math.sin 0f
	; 0f - угол в радианах. Результат вернем в 1f.

	; Шаг 1: Считаем степени угла
	mul 0f 0f 2f         ; 2f = x^2
	mul 2f 0f 3f         ; 3f = x^3
	mul 3f 2f 4f         ; 4f = x^5
	mul 4f 2f 5f         ; 5f = x^7

	; Шаг 2: Считаем элементы ряда (деление на факториалы)
	div 3f 6 3f          ; 3f = x^3 / 3!
	div 4f 120 4f        ; 4f = x^5 / 5!
	div 5f 5040 5f       ; 5f = x^7 / 7!

	; Шаг 3: Собираем всё вместе: x - (x^3/6) + (x^5/120) - (x^7/5040)
	sub 0f 3f 1f         ; 1f = x - x^3/6
	add 1f 4f 1f         ; 1f = (x - x^3/6) + x^5/120
	sub 1f 5f 1f         ; 1f = финальный синус!

	; Очищаем временные регистры
	mov 2f 0
	mov 3f 0
	mov 4f 0
	mov 5f 0
ret

cmd math.cos 0f
	; 0f - угол в радианах. Результат вернем in 1f.

	; Шаг 1: Считаем степени угла
	mul 0f 0f 2f         ; 2f = x^2
	mul 2f 2f 3f         ; 3f = x^4
	mul 3f 2f 4f         ; 4f = x^6

	; Шаг 2: Делим на факториалы
	div 2f 2 2f          ; 2f = x^2 / 2!
	div 3f 24 3f         ; 3f = x^4 / 4!
	div 4f 720 4f        ; 4f = x^6 / 6!

	; Шаг 3: Собираем вместе: 1 - (x^2/2) + (x^4/24) - (x^6/720)
	sub 1 2f 1f          ; 1f = 1 - x^2/2
	add 1f 3f 1f         ; 1f = (1 - x^2/2) + x^4/24
	sub 1f 4f 1f         ; 1f = финальный косинус!

	; Очищаем временные регистры
	mov 2f 0
	mov 3f 0
	mov 4f 0
ret

cmd math.tan 0f
	mov 5f 0f
	call [5f] math.cos
	mov 6f 1f

	call [5f] math.sin

	div 1f 6f 1f

	mov 5f 0
	mov 6f 0
ret

cmd math.ctg 0f
	mov 5f 0f

	call [5f] math.sin
	mov 6f 1f 

	call [5f] math.cos

	div 1f 6f 1f

	mov 5f 0
	mov 6f 0
ret

cmd math.sin2 0f
	call [0f] math.sin
	mul 1f 1f 1f
ret

cmd math.cos2 0f
	call [0f] math.cos
	mul 1f 1f 1f
ret

cmd math.tan2 0f
	call [0f] math.tan
	mul 1f 1f 1f
ret

cmd math.ctg2 0f
	call [0f] math.ctg
	mul 1f 1f 1f
ret

cmd math.pow 0f 1f
    mov 2f 0f
    mov 3f 1
    mov 4f 0

    jf 1f > -1 check_abs_num
	    mul 1f -1 1f
	    mov 4f 1
    check_abs_num:
	    jf 1f == pow_zero
	    jf 1f == 1 pow_one

	    pow_loop:
	        inc 3f 3f
	        mul 0f 2f 0f
	        jf 3f < 1f pow_loop

	    jf 4f == 1 apply_neg_pow
	    jmp pow_end

	    apply_neg_pow:
	        div 1 0f 0f
	        jmp pow_end

	    pow_zero:
	        mov 0f 1
	        jmp pow_end
	    pow_one:
	        jmp pow_end

	    pow_end:
	        mov 1f 0f
	        mov 2f 0
	        mov 3f 0
	        mov 4f 0
ret