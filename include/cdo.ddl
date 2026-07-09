#include strapi.ddl
#include fsapi.ddl

cmd cdo.Parse 0s 1s
    ; 0s - file name, 1s - list name

    spt 0s "." exts
    len exts 2f
    dec 2f 2f
    get exts 2f 3s
    jf 3s == "cdo" cdo.ParseContinue
        mov 5m 0 1 "Invalid file extension, must be '.cdo'!"
    cdo.ParseContinue:
        rdl 0s 2s
        spt 2s "\n" lines

        mov 3f 0 ; i
        mov 4f 0 ; start's index
        mov 6s "" ; method
        mov 7s "" ; info
        mov 8s "" ; from

        cdo.ParseStart:
            get lines 3f 0s
            trm 0s 0s

            low 0s 0s

            inc 3f 3f
            jf 0s != "start" cdo.ParseStart

            dec 3f 5f ; find real start's index
            mov 4f 5f ; start's index

            cdo.ParseGetSource:
                inc 4f 4f
                get lines 4f 0s

                trm 0s 0s

                spt 0s ":" command

                get command 0 0s

                low 0s 0s
                
                jf 0s == "method" cdo.ParseGetMethod
                jf 0s == "info" cdo.ParseGetInfo
                jf 0s == "from" cdo.ParseGetFrom
                jf 0s == "end" cdo.ParseEnd
                jmp cdo.ParseGetSource

            cdo.ParseGetMethod:
                get command 1 0s
                trm 0s 0s
                mov 6s 0s ; method
                jmp cdo.ParseGetSource
            
            cdo.ParseGetInfo:
                get command 1 0s
                trm 0s 0s
                spt 0s "," words
                len words 7f
                mov 7s 0s ; info
                jmp cdo.ParseGetSource

            cdo.ParseGetFrom:
                get command 1 0s
                trm 0s 0s
                mov 8s 0s ; from
                jmp cdo.ParseGetSource
            
        cdo.ParseEnd:
            push 1s {method=6s, info=7s, from=8s}
            
            mov 0s ""
            mov 1s ""
            mov 2s ""

            mov 3s ""
            mov 4s ""
            mov 5s ""

            mov 6s ""
            mov 7s ""
            mov 8s ""
ret