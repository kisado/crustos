; PSP=ebp RSP=esp

BITS 32

%define CELLSZ 4
%define PS_SZ 0x1000
%define RS_SZ 0x1000
%define MEMSIZE 0x40000
%define SYSCALL_EXIT 1
%define SYSCALL_READ 3
%define SYSCALL_WRITE 4
%define SYSCALL_CHDIR 12

%macro pspush 1
    sub ebp, CELLSZ
    mov dword [ebp], %1
%endmacro

%macro pspop 1
    mov %1, dword [ebp]
    add ebp, CELLSZ
%endmacro

%macro firstword 3
db %1
dd 0
db %2
%push dict
%$prev_word:
%3:
%endmacro

%macro defword 3
db %1
dd %$prev_word
db %2
%pop
%push dict
%$prev_word:
%3:
%endmacro

%macro sysval 1
mov eax,%1
test byte [toflag], 0xff
jnz to_is_set
mov eax,[eax]
pspush eax
ret
%endmacro

%macro sysalias 1
mov eax,%1
test byte [toflag], 0xff
jnz to_is_set
jmp [eax]
%endmacro

SECTION .bss

areg: resd 1
toflag: resb 1
exitonabort: resb 1
bootptr: resd 1
current: resd 1
here: resd 1
compiling: resd 1
curword: resb 0x20
iinrd: resd 1
inrd: resd 1
    resd PS_SZ
ps_top:
    resd RS_SZ
rs_top:
herestart: resb MEMSIZE

SECTION .data
bootsrc: incbin "boot.fs"
rootfspath: db "fs", 0
wnfstr: db " word not found"
uflwstr: db "stack underflow"
SECTION .text

GLOBAL _start
_start:
    mov byte [exitonabort], 0
    mov dword [bootptr], bootsrc
    mov dword [here], herestart
    mov dword [current], word_mainloop
    mov dword [iinrd], word_bootrd
    mov dword [inrd], word_iinrd
    mov eax, SYSCALL_CHDIR
    mov ebx, rootfspath
    int 0x80
    jmp word_abort

firstword 'bye', 3, word_bye
    mov eax, SYSCALL_EXIT
    mov ebx, 0
    int 0x80

defword 'noop', 4, word_noop
    ret

defword 'quit', 4, word_quit
    cld
    mov byte [toflag], 0
    mov dword [inrd], word_iinrd
    mov esp, rs_top
    jmp word_mainloop

defword 'abort', 5, word_abort
    test byte [exitonabort], -1
    jz _abort_no_exit
    mov eax, SYSCALL_EXIT
    mov ebx, 1
    int 0x80
_abort_no_exit:
    mov ebp, ps_top
    jmp word_quit

defword 'exitonabort', 11, word_exitonabort
    mov byte [exitonabort], 1
    ret

defword 'exit', 4, word_exit
    pop eax
    ret

defword 'execute', 7, word_execute
    pspop eax
    jmp eax

defword '(cell)', 6, word_cellroutine
    pop eax
    pspush eax
    ret

to_is_set:
    mov byte [toflag], 0
    pspop ebx
    mov [eax], ebx
    ret

defword '(val)', 5, word_valroutine
    pop eax
    test byte [toflag], 0xff
    jnz to_is_set
    mov ebx, [eax]
    pspush ebx
    ret

defword '(alias)', 7, word_aliasroutine
    pop eax
    test byte [toflag], 0xff
    jnz to_is_set
    jmp [eax]

defword '(does)', 6, word_doesroutine
    pop eax
    mov ebx, eax
    add ebx, CELLSZ
    pspush ebx
    jmp [eax]

defword '(s)', 3, word_strlit
    pop esi
    pspush esi
    mov eax, 0
    lodsb
    add esi, eax
    jmp esi

defword '(br)', 4, word_brroutine
    pop eax
    jmp dword [eax]

defword '(?br)', 5, word_condbrroutine
    pspop eax
    or eax, eax
    jz word_brroutine
    pop eax
    add eax, CELLSZ
    jmp eax

defword '(next)', 6, word_nextroutine
    dec dword [esp+CELLSZ]
    jnz word_brroutine
    pop eax
    pop ebx
    add eax, CELLSZ
    jmp eax

defword 'boot<', 5, word_bootrd
    mov esi, [bootptr]
    xor eax, eax
    mov al, [esi]
    inc dword [bootptr]
    pspush eax
    ret

defword 'emit', 4, word_emit
    mov eax, SYSCALL_WRITE
    mov ebx, 1
    mov ecx, ebp
    mov edx, 1
    int 0x80
    pspop eax
    ret

defword 'key', 3, word_key
    pspush 0
    mov eax, SYSCALL_READ
    mov ebx, 0
    mov ecx, ebp
    mov edx, 1
    int 0x80
    ret

defword 'lnxcall', 7, word_lnxcall
    pspop edx
    pspop ecx
    pspop ebx
    pspop eax
    int 0x80
    pspush eax
    ret

defword 'drop', 4, word_drop
    add ebp, CELLSZ
    ret

defword 'dup', 3, word_dup
    mov eax, [ebp]
    sub ebp, CELLSZ
    mov [ebp], eax
    ret

defword '?dup', 4, word_conddup
    test dword [ebp], -1
    jnz word_dup
    ret

defword 'swap', 4, word_swap
    mov eax, [ebp]
    mov ebx, [ebp+CELLSZ]
    mov [ebp], ebx
    mov [ebp+CELLSZ], eax
    ret

defword 'over', 4, word_over
    mov eax, [ebp+CELLSZ]
    sub ebp, CELLSZ
    mov [ebp], eax
    ret

defword 'rot', 3, word_rot
    mov eax, [ebp]
    mov ebx, [ebp+CELLSZ]
    mov ecx, [ebp+CELLSZ*2]
    mov [ebp], ecx
    mov [ebp+CELLSZ], eax
    mov [ebp+CELLSZ*2], ebx
    ret

defword 'nip', 3, word_nip
    pspop eax
    mov [ebp], eax
    ret

defword 'tuck', 4, word_tuck
    mov eax, [ebp]
    mov ebx, [ebp+CELLSZ]
    mov [ebp], ebx
    mov [ebp+CELLSZ], eax
    pspush eax
    ret

defword 'rot>', 4, word_rotr
    mov eax, [ebp]
    mov ebx, [ebp+CELLSZ]
    mov ecx, [ebp+CELLSZ*2]
    mov [ebp], ebx
    mov [ebp+CELLSZ], ecx
    mov [ebp+CELLSZ*2], eax
    ret

; Warning: RS routines are all called, which means that we have to work from
; the second item from the top rather than the first.

defword 'r>', 2, word_rs2ps
    pop eax
    sub ebp, CELLSZ
    pop dword [ebp]
    jmp eax

defword '>r', 2, word_ps2rs
    pspop eax
    xchg eax, [esp]
    jmp eax

defword 'r@', 2, word_rsget
    mov eax, [esp+CELLSZ]
    pspush eax
    ret

defword 'r~', 2, word_rsdrop
    pop eax
    add esp, CELLSZ
    jmp eax

defword 'scnt', 4, word_scnt
    mov eax, ps_top
    sub eax, ebp
    shr ax, 2
    pspush eax
    ret

defword 'rcnt', 4, word_rcnt
    mov eax, rs_top
    sub eax, esp
    shr ax, 2
    dec ax
    pspush eax
    ret

defword '>A', 2, word_Aset
    pspop eax
    mov [areg], eax
    ret

defword 'A>', 2, word_Aget
    mov eax, [areg]
    pspush eax
    ret

defword 'Ac@', 3, word_Acfetch
    mov eax, 0
    mov esi, [areg]
    mov al, [esi]
    pspush eax
    ret

defword 'Ac!', 3, word_Acstore
    pspop eax
    mov esi, [areg]
    mov [esi], al
    ret

defword 'A+', 2, word_Ainc
    inc dword [areg]
    ret

defword 'A-', 2, word_Adec
    dec dword [areg]
    ret

defword 'A>r', 3, word_A2rs
    pop eax
    push dword [areg]
    jmp eax

defword 'r>A', 3, word_rs2A
    pop eax
    pop dword [areg]
    jmp eax

defword 'to', 2, word_to
    mov byte [toflag], 1
    ret

defword 'to?', 3, word_tocond
    xor eax, eax
    mov al, [toflag]
    pspush eax
    mov byte [toflag], 0
    ret

defword '1+', 2, word_inc
    inc dword [ebp]
    ret

defword '1-', 2, word_dec
    dec dword [ebp]
    ret

defword 'c@', 2, word_cfetch
    mov esi, [ebp]
    mov eax, 0
    mov al, [esi]
    mov [ebp], eax
    ret

defword 'c!', 2, word_cstore
    pspop eax
    pspop ebx
    mov [eax], bl
    ret

defword 'c,', 2, word_cwrite
    pspop eax
    mov esi, [here]
    mov [esi], al
    inc dword [here]
    ret

defword '@', 1, word_fetch
    mov esi, [ebp]
    mov eax, [esi]
    mov [ebp], eax
    ret

defword '!', 1, word_store
    pspop eax
    pspop ebx
    mov [eax], ebx
    ret

defword ',', 1, word_write
    pspop eax
    mov esi, [here]
    mov [esi], eax
    add dword [here], CELLSZ
    ret

defword '+', 1, word_add
    pspop eax
    add [ebp], eax
    ret

defword '-', 1, word_sub
    pspop eax
    sub [ebp], eax
    ret

defword '*', 1, word_mul
    pspop eax
    mov ebx, [ebp]
    mul ebx
    mov [ebp], eax
    ret

defword 'and', 3, word_and
    pspop eax
    and [ebp], eax
    ret

defword 'or', 2, word_or
    pspop eax
    or [ebp], eax
    ret

defword 'xor', 3, word_xor
    pspop eax
    xor [ebp], eax
    ret

defword 'not', 3, word_not
    mov eax, [ebp]
    mov dword [ebp], 0
    test eax, eax
    setz byte [ebp]
    ret

defword '<', 1, word_lt
    pspop eax
    sub [ebp], eax
    mov dword [ebp], 0
    setc byte [ebp]
    ret

defword '<<c', 3, word_shlc
    pspush 0
    shl dword [ebp+CELLSZ], 1
    setc byte [ebp]
    ret

defword '>>c', 3, word_shrc
    pspush 0
    shr dword [ebp+CELLSZ], 1
    setc byte [ebp]
    ret

defword 'lshift', 6, word_lshift
    pspop ecx
    shl dword [ebp], cl
    ret

defword 'rshift', 6, word_rshift
    pspop ecx
    shr dword [ebp], cl
    ret

litncode:
    pspush 0
litncode_end:
defword 'litn', 4, word_litn
    pspush litncode
    pspush litncode_end-litncode-CELLSZ
    call word_movewrite
    jmp word_write

defword 'call,', 5, word_callwrite
    pspush 0xe8
    call word_cwrite
    mov eax, [ebp]
    sub eax, [here]
    sub eax, 4
    mov [ebp], eax
    jmp word_write

defword 'exit,', 5, word_exitwrite
    pspush 0xc3
    jmp word_cwrite

; The part below used to be written in a pseudo cross-compatible forth, but
; the tooling around it was too complex for what it was worth.

defword 'current', 7, word_current
    sysval current

defword 'here', 4, word_here
    sysval here

defword 'compiling', 9, word_compiling
    sysval compiling

; interactive in<
defword 'iin<', 4, word_iinrd
    sysalias iinrd

; where `word` feeds itself
defword 'in<', 3, word_inrd
    sysalias inrd

defword 'allot', 5, word_allot
    pspop eax
    add dword [here], eax
    ret

defword 'move', 4, word_move
    pspop ecx
    pspop edi
    pspop esi
    test ecx, ecx
    jz _ret
    rep movsb
_ret:
    ret

defword 'move,', 5, word_movewrite
    pspop ecx
    pspop esi
    test ecx, ecx
    jz _ret
    mov edi, [here]
    add dword [here], ecx
    rep movsb
    ret

defword 'rtype', 5, word_rtype
    pspop ecx
    pspop esi
_rtype_loop:
    xor eax, eax
    mov al, [esi]
    pspush eax
    push esi
    push ecx
    call word_emit
    pop ecx
    pop esi
    inc esi
    dec ecx
    jnz _rtype_loop
    ret

defword '(wnf)', 5, word_wnf
    mov esi, curword+1
    xor ecx, ecx
    mov cl, [curword]
    call _rtype_loop
    mov ecx, 15
    mov esi, wnfstr
    call _rtype_loop
    jmp word_abort

defword 'stack?', 6, word_stackcond
    cmp ebp, ps_top
    jna _ret
    mov ecx, 15
    mov esi, uflwstr
    call _rtype_loop
    jmp word_abort

defword 'curword', 7, word_curword
    pspush curword
    ret

defword 'word', 4, word_word
_word_loop1:
    call [inrd]
    pspop eax
    cmp eax, 0x21
    jc _word_loop1
    mov ebx, curword+1
_word_loop2:
    mov [ebx], al
    inc ebx
    push ebx
    call [inrd]
    pop ebx
    pspop eax
    cmp eax, 0x21
    jnc _word_loop2
    sub ebx, curword+1
    mov [curword], bl
    jmp word_curword

_parse_c:
    cmp ecx, 3
    jnz _parse_no
    cmp byte [esi+2], "'"
    jnz _parse_no
    xor eax, eax
    mov al, [esi+1]
    mov [ebp], eax
    pspush 1
    ret

_parse_h:
    cmp ecx, 2
    jc _parse_no
    inc esi
    dec ecx
    xor eax, eax
    xor ebx, ebx
_parse_h_loop:
    mov bl, [esi]
    or bl, 0x20
    sub bl, '0'
    jc _parse_no
    cmp bl, 10
    jc _parse_h_ok
    sub bl, 'a'-'0'
    jc _parse_no
    add bl, 10
    cmp bl, 16
    jnc _parse_no
_parse_h_ok:
    shl eax, 4
    add eax, ebx
    inc esi
    dec ecx
    jnz _parse_h_loop
    mov [ebp], eax
    pspush 1
    ret
__h_no:
    mov dword [ebp], 0
    ret

_parse_ud:
    test ecx, ecx
    jz _parse_no
    xor eax, eax
_parse_ud_loop:
    mov ebx, 10
    mul ebx
    mov bl, [esi]
    sub bl, '0'
    jc _parse_no
    cmp bl, 10
    jnc _parse_no
    add eax, ebx
    inc esi
    dec ecx
    jnz _parse_ud_loop
    mov [ebp], eax
    pspush 1
    ret

defword 'parse', 5, word_parse
    mov esi, [ebp]
    xor ecx, ecx
    mov cl, [esi]
    inc esi
    cmp byte [esi], "'"
    jz _parse_c
    cmp byte [esi], '$'
    jz _parse_h
    cmp byte [esi], '-'
    jnz _parse_ud
    inc esi
    dec ecx
    call _parse_ud
    test dword [ebp], -1
    jz _parse_no
    neg dword [ebp+CELLSZ]
    ret
_parse_no:
    mov dword [ebp], 0
    ret

defword '[]=', 3, word_rangeeq
    pspop ecx
    pspop edi
    pspop esi
    xor eax, eax
    repz cmpsb
    setz al
    pspush eax
    ret

defword 'find', 4, word_find
    mov esi, [ebp]
    xor ecx, ecx
    mov cl, [esi]
    inc dword [ebp]
    mov edx, [current]
_find_loop:
    mov edi, edx
    dec edi
    mov al, [edi]
    and al, 0x7f
    cmp al, cl
    jnz _find_skip1
    sub edi, 4
    sub edi, ecx
    mov esi, [ebp]
    repz cmpsb
    jnz _find_skip2
    mov [ebp], edx
    pspush 1
    ret
_find_skip2:
    mov cl, al
_find_skip1:
    sub edx, 5
    mov edx, [edx]
    test edx, edx
    jnz _find_loop
    mov dword [ebp], 0
    ret

defword "'", 1, word_apos
    call word_word
    call word_find
    pspop eax
    test eax, eax
    jz word_wnf
    ret

defword 'entry', 5, word_entry
    mov esi, [ebp]
    xor ecx, ecx
    mov cl, [esi]
    inc dword [ebp]
    pspush ecx
    call word_tuck
    call word_movewrite
    call word_current
    call word_write
    call word_cwrite
    mov eax, [here]
    mov [current], eax
    ret

defword 'xtcomp', 6, word_xtcomp
    mov dword [compiling], 1
_xtcomp_loop:
    call word_word
    call word_parse
    pspop eax
    test eax, eax
    jz _xtcomp_notlit
    call word_litn
    jmp _xtcomp_loop
_xtcomp_notlit:
    call word_curword
    call word_find
    pspop eax
    test eax, eax
    jz word_wnf
    mov eax, [ebp]
    dec eax
    mov bl, [eax]
    and bl, 0x80
    jnz _xtcomp_imm
    call word_callwrite
    jmp _xtcomp_loop
_xtcomp_imm:
    call word_execute
    test dword [compiling], -1
    jnz _xtcomp_loop
    jmp word_exitwrite

defword ':', 1, word_docol
    call word_word
    call word_entry
    jmp word_xtcomp

defword ';', 0x81, word_compstop
    mov dword [compiling], 0
    ret

defword 'run1', 4, word_run1
    xor eax, eax
    mov al, [toflag]
    push eax
    mov byte [toflag], 0
    call word_word
    call word_parse
    pspop eax
    test eax, eax
    jz _run1_notlit
    pop eax
    mov [toflag], al
    ret
_run1_notlit:
    call word_curword
    call word_find
    pspop eax
    test eax, eax
    jz word_wnf
    pop eax
    mov [toflag], al
    call word_execute
    jmp word_stackcond

defword 'mainloop', 8, word_mainloop
    call word_run1
    jmp word_mainloop