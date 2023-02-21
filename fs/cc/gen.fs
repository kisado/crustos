\ C Compiler Code Generation Utils
\ Requires wordtbl, asm, cc/tree and cc/ast

\ Code generation

: _err ( node -- ) printast abort"  unexpected node" ;

UOPSCNT wordtbl uopgentbl ( -- )
:w ( - ) eax neg, ;
:w ( ~ ) eax not, ;
:w ( ! )
  eax eax test,
  eax 0 i32 mov,
  al setz, ;

BOPSCNT wordtbl bopgentblmiddle ( node -- node )
'w noop ( + )
'w noop ( - )
'w noop ( * )
'w noop ( / )
'w noop ( < )
'w noop ( > )
'w noop ( <= )
'w noop ( >= )
'w noop ( == )
'w noop ( != )
:w ( && ) ( node -- jump_addr node )
  eax eax test,
  0 jz, here 4 - swap ;
:w ( || ) ( node -- jump_addr node )
  eax eax test,
  0 jnz, here 4 - swap ;

BOPSCNT wordtbl bopgentblpost ( -- )
:w ( + ) eax ebx add, ;
:w ( - ) eax ebx sub, ;
:w ( * ) ebx mul, ;
:w ( / ) abort" TODO" ;
:w ( < )
  eax ebx cmp,
  eax 0 i32 mov,
  al setg, ;
:w ( > ) abort" TODO" ;
:w ( <= ) abort" TODO" ;
:w ( >= ) abort" TODO" ;
:w ( == )
  eax ebx cmp,
  eax 0 i32 mov,
  al setz, ;
:w ( != ) abort" TODO" ;
:w ( && ) ( jump_addr -- ) here over - 4 - swap ! ;
:w ( || ) ( jump_addr -- ) here over - 4 - swap ! ;

alias noop gennode ( node -- ) \ forward declaration

: genchildren ( node -- )
  firstchild ?dup if begin dup gennode nextsibling ?dup not until then ;

: spit ( a u -- ) A>r >r >A begin Ac@+ .x1 spc> next r>A ;
: getfuncnode ( node -- node ) AST_FUNCTION parentnodeid data2 ;

ASTIDCNT wordtbl gentbl ( node -- )
'w genchildren ( Declare )
'w genchildren ( Unit )
:w ( Function )
  _debug if ." debug: " dup data1 stype nl> then
  dup data1 entry
  dup data2 ( MAP_FUNCTION ) data2 ?dup if
    ebp i32 sub, then
  genchildren
  _debug if current here current - spit nl> then ;
:w ( Return ) dup genchildren ( node )
  getfuncnode data2 ?dup if
    ebp i32 add, then
  ebp 4 i32 sub,
  [ebp] eax mov,
  ret, ;
:w ( Constant ) eax data1 i32 mov, ;
:w ( Statements ) genchildren ;
'w genchildren ( Arguments )
'w genchildren ( Expression )
:w ( UnaryOp ) dup genchildren data1 uopgentbl swap wexec ;
'w genchildren ( Factor )
:w ( BinaryOp )
  ( node ) >r
  r@ childcount 2 = not if abort" binop node with more than 2 children" then
  r@ firstchild dup nextsibling swap ( n1 n2 )
  gennode bopgentblmiddle r@ data1 wexec eax push,
  gennode ebx pop, bopgentblpost r> data1 wexec ;
:w ( Assign )
  dup genchildren ( node )
  dup data1 ( node name )
  swap getfuncnode findvarinmap ( varnode ) data2 ( offset )
  [ebp]+ eax mov, ;
'w genchildren ( DeclarationList )
:w ( Variable )
  dup data1 ( node name )
  swap getfuncnode findvarinmap ( varnode ) data2 ( offset )
  eax [ebp]+ mov, ;

: _ ( node -- ) gentbl over astid wexec ;
current to gennode
