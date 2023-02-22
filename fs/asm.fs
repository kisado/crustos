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
: op, ( op -- ) dup 8 rshift ?dup if c, then c, ;
: inh, ( op -- ) op, asm$ ;
: modrm1, ( reg op -- )                   \ modrm op with 1 argument
  prefix, op, ( reg ) 3 lshift tgtid tgt mod or or ( modrm ) c,
  disp? if disp c, then asm$ ;
: modrm<imm, ( imm immreg op -- )
  op, 3 lshift tgtid or tgt mod or ( modrm ) c, , disp, asm$ ;
: modrm2, ( imm? reg op -- )                  \ modrm op with 2 arguments
  src mod $c0 = not if
    2 + c, ( src ) mod tgt 3 lshift or srcid or
  else
    c, 3 lshift tgtid or tgt mod or then
  ( modrm ) c, disp, asm$ ;

\  Operations
\ Inherent
: op ( opcode -- ) doer c, does> ( a -- ) c@ inh, ;
$c3 op ret,

\ Relative Jumps
: op ( opcode -- ) doer , does> ( rel32 a -- ) @ op, , ;
$00e9 op jmp,
$0f84 op jz,
$0f85 op jnz,

\ Single Operand
: op ( reg opcode -- ) doer , c, does> ( a -- )
  dup @ swap 4 + c@ swap modrm1, ;
4 $f7 op mul,
3 $f7 op neg,
2 $f7 op not,
0 $0f9f op setg,
0 $0f9c op setl,
0 $0f94 op setz,

\ Two Operands
: op ( immop immreg regop -- ) doer c, c, c, does> ( imm? a -- )
  prefix,
  isimm? if
    dup 1+ c@ ( immreg ) swap 2 + c@ ( immreg immop ) modrm<imm, else
    c@ src swap ( reg regop ) modrm2, then ;
$81 0 $01 op add,
$81 7 $39 op cmp,
$81 5 29 op sub,
$f7 0 $85 op test,

\ tgt or-ed in
: op ( op -- ) doer c, does> ( a -- ) c@ tgtid or c, asm$ ;
$58 op pop,
$50 op push,

\ Special
: mov,
  prefix, isimm? if
    $b8 tgtid or c, , asm$ else
    src $89 modrm2, then ;
