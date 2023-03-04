\ C Compiler Virtual Machine
\ Requires asm

\ The goal of this VM is to provide a unified API for code generation of a C
\ AST across CPU architecture.

\ Computation done by this generated code is centered around the "Result".
\ concept. The goal is to make the CPU move bits around towards that Result.
\ The Result always lives on a particular register reserved for this role. For
\ example, on x86, it's EAX.

\ The VM very often interacts with the Result through another concept: the
\ Operand. Moving from the operand to the result, from the result to the
\ operand, running an operation on both the Result and the operand, etc.

\ The Operand doesn't live in a particular register, it comes from multiple
\ types of sources. Its possible values are:

\ None: no operand specified
\ Constant: a constant value
\ Stack Frame: an address on the Stack Frame
\ Register: value currently being held in an "alternate" register (EBX on x86)

\ We operate the VM by first specifying an operand, moving the operand toward
\ the result, performing ops, moving the result out. If we need to keep an
\ intermediate result for later, we can push it to a stack (the mechanism for
\ this is arch-specific). The pushed result can be later pulled backed into the
\ Operand.

\ To avoid errors, moving an operand to a non-empty and non-pushed Result is an
\ error. To set the operand when it's not None is also an error.

\ For usage example, see tests/cc/vm.fs

$00 const VM_NONE
$01 const VM_CONSTANT
$02 const VM_STACKFRAME
$03 const VM_REGISTER

0 value resultset?
VM_NONE value operand
0 value operandarg
0 value operandlvl
0 value argsz
0 value locsz
0 value callsz

: vm$ 0 to resultset? VM_NONE to operand 0 to operandlvl 0 to callsz ;

: _err abort" vm error" ;
: _assert not if _err then ;

\ Get current operand SF offset, adjusted with callsz
: opersf+ ( -- off ) operandarg callsz + ;

\ Resolve current operand as an assembler 'src' argument
: operandAsm ( -- )
  operand VM_CONSTANT = if
    operandarg i32
  else operand VM_REGISTER = if
    ebx
  else operand VM_STACKFRAME = if
    opersf+ [ebp]+
  else _err then then then
  VM_NONE to operand ;

: result! 1 to resultset? ;

\ Force current operand to be copied to the 'alternate' register
: operand>reg ( -- )
  operand VM_REGISTER = not if
    ebx operandAsm mov, VM_REGISTER to operand then ;

: resolvederef
  operandlvl if
    operandlvl 0< if
      0 to operandlvl
      VM_REGISTER to operand
      ebx ebp mov,
      opersf+ ?dup if ebx i32 add, then
    else operand>reg begin
      ebx [ebx] mov, -1 to+ operandlvl operandlvl not until then then ;

: const>operand ( n -- )
  VM_NONE operand = _assert
  VM_CONSTANT to operand to operandarg ;

: sf+>operand ( offset -- )
  VM_NONE operand = _assert
  VM_STACKFRAME to operand to operandarg ;

: operand>result ( -- )
  resultset? not _assert
  resolvederef
  eax operandAsm mov, result! ;

: operand?>result operand VM_NONE = not if operand>result then ;

: operand>&operand
  operand VM_STACKFRAME = _assert
  operandlvl 0>= _assert
  -1 to operandlvl ;

: operand>[operand]
  operand VM_STACKFRAME = operand VM_REGISTER = or _assert
  1 to+ operandlvl ;

: result>operand
  resultset? _assert
  operand VM_STACKFRAME = if
    operandlvl if
      -1 to+ operandlvl operand>reg resolvederef
      [ebx] eax mov,
      else operandAsm eax mov, then
    else operand VM_REGISTER = if
      -1 to+ operandlvl resolvederef
      [ebx] eax mov,
      else _err then then
  0 to resultset? VM_NONE to operand ;

\ Generate function prelude code by allocating 'locsz' bytes on PS
: vmprelude, ( argsz locsz -- )
  to locsz to argsz
  locsz if ebp locsz i32 sub, then ;

\ Deallocate locsz and argsz. If result is set, keep a 4b in here and push the result there
: vmret,
  locsz argsz + resultset? if 4 - then
  ?dup if ebp i32 add, then
  resultset? if [ebp] eax mov, then
  ret, ;
: pushresult, resultset? _assert eax push, 0 to resultset? ;
: popresult,
  VM_NONE operand = _assert
  VM_REGISTER to operand
  ebx pop, ;
: callargallot, ( bytes -- )
  dup to callsz ebp i32 sub, ;

: vmcall>result, ( addr -- )
  resultset? not _assert
  call,
  eax [ebp] mov, 1 to resultset?
  ebp 4 i32 add, 0 to callsz ;
: vmadd, eax operandAsm add, result! ;
: vmsub, eax operandAsm sub, result! ;
: vmmul, operand>reg operandAsm mul, result! ;
: vmneg, eax neg, ;
: vmnot, ( ~ ) eax not, ;
: vmboolnot,
  eax eax test,
  eax 0 i32 mov,
  al setz, ;

: vm<,
  eax operandAsm cmp,
  eax 0 i32 mov,
  al setg, ;
: vm==,
  eax operandAsm cmp,
  eax 0 i32 mov,
  al setz, ;

: vmjmp! ( 'jump_addr -- ) here over - 4 - swap ! ;
: vmjz, ( -- addr )
  eax eax test, 0 to resultset?
  0 jz, here 4 - ;
: vmjnz, ( -- addr )
  eax eax test, 0 to resultset?
  0 jnz, here 4 - ;
