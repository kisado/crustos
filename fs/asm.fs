\ i386 assembler
\
\ This assembler is far far from being complete. Only operations needed by the C
\ compiler are implemented.
\
\ This assembler implies that the code will run in protected mode with the D
\ attribute set.

\ MOD/RM constants
0 value AX
1 value CX
2 value DX
3 value BX
4 value SP
5 value BP
6 value SI
7 value DI
8 value MEM
9 value IMM

\ Variables
-1 value tgt        \ target | bit 31 is set if in indirect mode
-1 value src        \ source | bit 31 is set for indirect mode
-1 value disp       \ displacement to use

: asm$ -1 to tgt -1 to src -1 to disp ;
: _err abort" argument error" ;

\ Addressing Modes
: tgt? tgt 0>= ;
: src? src 0>= ;
: disp? disp 0>= ;
: _id dup 0< if _err then $1f and ;
: tgtid tgt _id ;
: srcid src _id ;
: mod ( tgt-or-src -- mod ) >> >> $c0 and ;
: is16? ( tgt-or-src -- f ) 10 rshift 1 and ;
: isimm? ( -- f ) src _id IMM = ;

: tgt-or-src! tgt 0< if to tgt else to src then ;
: r! ( reg -- ) $300 or ( mod 3 ) tgt-or-src! ;
: [r]! ( reg -- ) $100 or ( mod 1 ) tgt-or-src! ;

: eax AX r! ; alias eax ax alias eax al
: ebx BX r! ; alias ebx bx alias ebx bl
: ebp BP r! ;
: [ebp] BP [r]! 0 to disp ;
: [ebp]+ ( disp -- ) BP [r]! to disp ;
: i32 IMM $400 or to src ;

\ Writing the thing
\ TODO: Big mess, rewrite the entire thing.
: disp, disp? if disp c, then ;
: prefix, ( -- ) exit
  tgt is16? if $66 c, then src isimm? not swap is16? and if $67 c, then ;
: inh, ( op -- ) c, asm$ ;
: modrm1, ( reg op -- )                   \ modrm op with 1 argument
  prefix, c, ( reg ) 3 lshift tgtid tgt mod or or ( modrm ) c,
  disp? if disp c, then asm$ ;
: modrm2, ( imm? reg op -- )                  \ modrm op with 2 arguments
  prefix,
  isimm? not if src mod $c0 = not if
    2 + c, ( src ) mod tgt 3 lshift or srcid or ( modrm ) c,
    disp, asm$ exit then then
  c, 3 lshift tgtid or tgt mod or ( modrm ) c,
  isimm? if , then disp, asm$ ;

\ Operations
: add, isimm? if 0 $81 else src $01 then modrm2, ;
: cmp, isimm? if 7 $81 else src $39 then modrm2, ;
: jmp, ( rel32 -- ) $e9 c, , ;
: jz, ( rel32 -- ) $0f c, $84 c, , ;
: jnz, ( rel32 -- ) $0f c, $85 c, , ;
: mov, isimm? if prefix, $b8 tgtid or c, , asm$ else src $89 modrm2, then ;
: mul, 4 $f7 modrm1, ;
: neg, 3 $f7 modrm1, ;
: not, 2 $f7 modrm1, ;
: pop, prefix, $58 tgtid or c, asm$ ;
: push, prefix, $50 tgtid or c, asm$ ;
: ret, $c3 inh, ;
: setg, $0f c, 0 $9f modrm1, ;
: setl, $0f c, 0 $9c modrm1, ;
: setz, $0f c, 0 $94 modrm1, ;
: sub, isimm? if 5 $81 else src $29 then modrm2, ;
: test, isimm? if 0 $f7 else src $85 then modrm2, ;
