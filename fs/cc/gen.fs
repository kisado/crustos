\ C Compiler Code Generation Utils
\ Requires wordtbl, cc/vm and cc/ast

\ Code generation

: _err ( node -- ) printast abort"  unexpected node" ;

UOPSCNT wordtbl uopgentbl ( -- )
'w vmneg, ( - )
'w vmnot, ( ~ )
'w vmboolnot, ( ! )

LOPSCNT wordtbl lopgentbl ( -- )
'w operand>&operand ( & )
'w operand>[operand] ( * )

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
:w ( && ) vmjz, swap ;
:w ( || ) vmjnz, swap ;

BOPSCNT wordtbl bopgentblpost ( -- )
'w vmadd, ( + )
'w vmsub, ( - )
'w vmmul, ( * )
:w ( / ) abort" TODO" ;
'w vm<, ( < )
:w ( > ) abort" TODO" ;
:w ( <= ) abort" TODO" ;
:w ( >= ) abort" TODO" ;
'w vm==, ( == )
:w ( != ) abort" TODO" ;
'w vmjmp! ( && )
'w vmjmp! ( || )

alias noop gennode ( node -- ) \ forward declaration

: genchildren ( node -- )
  firstchild ?dup if begin dup gennode nextsibling ?dup not until then ;

: spit ( a u -- ) A>r >r >A begin Ac@+ .x1 next r>A ;
: getfuncmap ( node -- funcentry ) AST_FUNCTION parentnodeid data2 ;

: lvsfoff ( lvnode -- off )
  dup data1 swap getfuncmap ( name funcentry ) findvarinmap ( varentry )
  vmap.sfoff ;

ASTIDCNT wordtbl gentbl ( node -- )
'w drop ( Declare )
'w genchildren ( Unit )
:w ( Function )
  _debug if ." debug: " dup data1 stype nl> then
  vm$
  dup data1 entry
  dup data2 ( astfunc mapfunc )
  here over fmap.address!
  dup fmap.argsize swap fmap.sfsize over - ( argsz locsz ) vmprelude,
  genchildren
  _debug if current here current - spit nl> then ;
:w ( Return )
  genchildren operand?>result vmret, ;
:w ( Constant ) data1 const>operand ;
:w ( Statements ) genchildren ;
'w genchildren ( ArgSpecs )
:w ( LValue ) lvsfoff sf+>operand ;
:w ( UnaryOp )
  dup genchildren
  operand?>result
  data1 uopgentbl swap wexec ;
:w ( Assign )
  firstchild ?dup not if _err then ( lvnode )
  dup nextsibling ?dup not if _err then ( lvnode exprnode )
  gennode operand?>result
  gennode
  result>operand ;
:w ( BinaryOp )
  ( node ) >r
  r@ childcount 2 = not if abort" binop node with more than 2 children" then
  r@ firstchild dup nextsibling swap ( n1 n2 )
  gennode bopgentblmiddle r@ data1 wexec
  operand?>result
  resultset? if
    pushresult, gennode operand?>result popresult, else
    gennode operand?>result then
  bopgentblpost r> data1 wexec ;
:w ( LValueOp )
  dup firstchild ?dup not if _err then gennode
  data1 lopgentbl swap wexec ;
:w ( If )
  firstchild ?dup not if _err then dup gennode ( exprnode )
  operand?>result vmjz, swap ( jump_addr exprnode )
  nextsibling ?dup not if _err then gennode ( jump_addr ) vmjmp! ;
'w _err ( unused )
:w ( FunCall )
  dup childcount 4 * callargallot,
  dup firstchild ?dup if -4 swap begin ( cursf+ argnode )
    dup gennode operand?>result swap dup sf+>operand result>operand
    4 - swap nextsibling ?dup not until drop then
  ( node ) data1 ( name ) findfuncinmap ( mapfunc )
  fmap.address vmcall>result, ;

: _ ( node -- ) gentbl over astid wexec ;
current to gennode
