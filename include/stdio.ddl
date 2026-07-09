#include strapi.ddl

cmd stdio.pop 0s
	; 0s - string
	spt 0s "\:" string
	pop string

	mov 1f 0
	mov 2s ""
	mov 5s ""
	len string 6f

    jf 6f < 0 stdiopopSkipLoop
    jf 6f == 0 stdiopopSkipLoop

	stdiopopLoop:
		get string 1f 2s

		add 5s 2s 5s
		inc 1f 1f

		jf 1f < 6f stdiopopLoop
        jmp stdiopopNext
    
    stdiopopSkipLoop:
        mov 1s ""
        mov 2s ""
        mov 5s ""

        mov 6s ""
        mov 1s ""

        jmp stdiopopEnd

    stdiopopNext:
        mov 1s ""
        mov 2s ""
        mov 6s ""

        mov 1s 5s
        mov 5s ""

    stdiopopEnd:
ret

cmd stdio.cipher 0s 1s
	; 0s - text, Key

	spt 0s "\:" textTokens
	len textTokens 2f
	dec 2f 2f ; text length

	spt 1s "\:" keyTokens
	len keyTokens 3f
	dec 3f 3f ; key length

	mov 4f 0 ; text i
	mov 5f 0 ; key i

	mov 6s "" ; text token buffer
	mov 7s "" ; key token buffer

	mov 8f 0 ; text num buffer
	mov 9f 0 ; key num buffer

	mov 10f 0 ; result num buffer
	mov 11s "" ; result token buffer

	mov 12s "" ; result

	stdiocipherLoop:
		jf 5f < 3f stdiocipherLoopNext
			jf 5f == 3f stdiocipherLoopNext
				mov 5f 0
		stdiocipherLoopNext:
			get textTokens 4f 6s
			get keyTokens 5f 7s
			
			chr 6s 8f
			chr 7s 9f

			xor 8f 9f 10f
			
			chr 10f 11s

			add 12s 11s 12s

			inc 4f 4f
			inc 5f 5f
			jf 4f < 2f stdiocipherLoop
			jf 4f == 2f stdiocipherLoop

	mov 1s 12s
	mov 2s ""
	mov 3s ""
	mov 4s ""
	mov 5s ""
	mov 6s ""
	mov 7s ""
	mov 8s ""
	mov 9s ""
	mov 10s ""
	mov 11s ""
	mov 12s ""
ret

cmd stdio.replace 0s 1s 2s
    ; Разбиваем строку 0s по разделителю 1s и кладем в массив r_arr
    spt 0s 1s r_arr
    
    ; Получаем количество элементов массива в регистр 6f
    len r_arr 6f
    
    ; Защита: если массив пуст (например, строка пустая), сразу выходим
    jf 6f == 0 rplace_end

    mov 5s ""         ; В 5s будем постепенно собирать итоговую строку
    mov 1f 0          ; Счетчик цикла (начинаем с 0)

	rplace_loop:
		; Достаем текущий кусочек текста из массива в 3s
		get r_arr 1f 3s
		
		; Приклеиваем этот кусочек к нашему результату
		add 5s 3s 5s
		
		; Увеличиваем счетчик
		inc 1f 1f
		
		; Важно: используем '==', так как твой парсер не поддерживает '>='
		; Если мы дошли до последнего элемента массива, выходим, 
		; чтобы не приклеить лишнюю строку замены в самом конце!
		jf 1f == 6f rplace_end
		
		; Добавляем строку-заменитель (2s) между кусочками текста
		add 5s 2s 5s
		
		; Идем на следующую итерацию
		jmp rplace_loop

	rplace_end:
		; Перезаписываем исходный регистр 0s готовым результатом
		mov 1s 5s
		
		; Очищаем временные регистры, чтобы не мусорить в памяти
		mov 5s ""
		mov 3s ""
ret

cmd stdio.mpt 0s 1f
    mul 0s 1f 0s
    mov 0m 0s
ret

cmd stdio.pwc 0s 1s
	; text, color
	mov 1m 1s
	mov 0m 0s
	mov 1m White
ret

cmd stdio.error 0s 1s
	; typeOfError, textOfError
	mov 1m red
	trm 0s 0s
	mov 0m 0s
	mov 0m "Error: "
	mov 0m 1s
	mov 1m white
	endl
	hlt
ret

cmd stdio.gui 0s 1s 2s 3s 4s
	; 0s - array name, 1s - color of area, 2s - top&down symbol, 3s - walls symbol, 4s - color of bg
	get 0s 0.width 0f
	get 0s 0.height 1f
	get 0s 0.x 2f
	get 0s 0.y 3f
	mov 4f 0 ; i
	sub 0f 2 5f
	sub 1f 2 6f
	mov 7f 0 ; i2
	inc 3f 3f
	mov 7m 0 3f

	area:
		mov 1m 1s
		jf 2f == 0 top

		mov 0m " "
		inc 4f 4f
		jf 4f < 2f area
		mov 4f 0

		top:
			mov 0m 2s
			inc 4f 4f
			jf 4f < 0f top
			endl
			mov 4f 0

		walls:
			mov 0m " "
			inc 4f 4f
			jf 4f < 2f walls

			mov 0m 3s
			mov 2m 4s
			call [" ", 5f] stdio.mpt

			mov 2m black
			mov 0m 3s
			endl
			mov 4f 0
			inc 7f 7f

			jf 7f < 6f walls
			mov 7f 0

		downar:
			call [" ", 2f] stdio.mpt
			call [2s, 0f] stdio.mpt
			endl
			mov 1m white
		
		mov 0s ""
		mov 1s ""
		mov 2s ""
		mov 3s ""
		mov 4s ""
		mov 0f 0
		mov 1f 0
		mov 2f 0
		mov 3f 0
		mov 4f 0
		mov 5f 0
		mov 6f 0
		mov 7f 0
ret