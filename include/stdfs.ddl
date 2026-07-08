cmd stdfs.apd 0s 1s
	; file name, text
	rdl 0s 2s ; read the file
	
	add 2s 1s 2s ; add text to the file's end

	wrt 0s 2s
	mov 2s ""
ret

cmd stdfs.prp 0s 1s
	; file name, text
	rdl 0s 2s ; read the file
	
	add 1s 2s 2s ; add text to the file's start

	wrt 0s 2s
	mov 2s ""
ret