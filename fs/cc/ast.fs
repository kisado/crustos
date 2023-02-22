\ C Compiler Abstract Syntax Tree
\ requires cc/tok.fs and cc/ops.fs

\ Unary Operators
\ ID  SYM NAME
\ 0   -   negate
\ 1   ~   complement
\ 2   !   not

3 value UOPSCNT
create uopssyms ," -~!?"

: uopid ( tok -- opid? f )
  c@+ 1 = if c@ uopssyms UOPSCNT [c]? dup 0< if drop 0 else 1 then
    else drop 0 then ;
: uopchar ( opid -- c ) UOPSCNT min uopssyms + c@ ;

12 value BOPSCNT
create BOPTlist 1 c, ," +"  1 c, ," -"  1 c, ," *" 1  c, ," /"
                1 c, ," <"  1 c, ," >"  2 c, ," <=" 2 c, ," >="
                2 c, ," ==" 2 c, ," !=" 2 c, ," &&" 2 c, ," ||"
                0 c,

create bopsprectbl  1 c, 1 c, 0 c, 0 c, 2 c, 2 c, 2 c, 2 c,
                    3 c, 3 c, 4 c, 4 c,

: bopid ( tok -- opid? f )
  BOPTlist sfind dup 0< if drop 0 else 1 then ;
: bopprec ( opid -- precedence ) BOPSCNT min bopsprectbl + c@ ;
: boptoken ( opid -- tok ) BOPTlist slistiter ;

15 value ASTIDCNT
0 value AST_DECLARE
1 value AST_UNIT
2 value AST_FUNCTION
3 value AST_RETURN
4 value AST_CONSTANT
5 value AST_STATEMENTS
6 value AST_ARGSPECS
7 value AST_EXPRESSION
8 value AST_UNARYOP
9 value AST_FACTOR
10 value AST_BINARYOP
11 value AST_ASSIGN
12 value AST_DECLLIST
13 value AST_VARIABLE
14 value AST_FUNCALL

create astidnames 7 c, ," declare"  4 c, ," unit"     8 c, ," function"
                  6 c, ," return"   8 c, ," constant" 5 c, ," stmts"
                  4 c, ," args"     4 c, ," expr"     7 c, ," unaryop"
                  6 c, ," factor"   5 c, ," binop"    6 c, ," assign"
                  8 c, ," decllist" 3 c, ," var"      4 c, ," call"
                  0 c,

0 value curunit     \ points to current Unit, the beginning of the AST
0 value activenode  \ elem we're currently adding to
0 value _skip       \ if 1, skip the next `nextt` in parseast

: astid ( node -- id ) nodeid $3f and ;
: idname ( id -- str ) astidnames slistiter ;
: skipnext 1 to _skip ;

: activeempty? ( -- f ) activenode firstchild not ;

: seqclose ( -- )
  activenode ?dup not if abort" can't go beyond root" then
  0 over cslots! begin parentnode dup nodeclosed? not until
  to activenode ;
: closeuntil ( astid -- )
  begin seqclose activenode astid over = until drop ;

: _[ '[' emit ;
: _] ']' emit ;
: _s _[ dup data1 stype _] ;
: _i _[ dup data1 .x _] ;

ASTIDCNT wordtbl astdatatbl ( node -- node )
'w _s ( Declare )
'w noop ( Unit )
'w _s ( Function )
'w noop ( Return )
'w _i ( Constant )
'w noop ( Statements )
'w noop ( ArgSpecs )
'w noop ( Expression )
:w ( UnaryOp ) _[ dup data1 uopchar emit _] ;
'w noop ( Factor )
:w ( BinaryOp ) _[ dup data1 boptoken stype _] ;
'w _s ( Assign )
'w noop ( DeclarationList )
'w _s ( Variable )
'w _s ( FunCall )

: printast ( node -- )
  ?dup not if ." null" exit then
  dup astid idname stype
  astdatatbl over astid wexec
  firstchild ?dup if
    '(' emit begin
      dup printast nextsibling dup if ',' emit then ?dup not until
    ')' emit then ;

: newnode
  createnode dup activenode addnode ( node )
  dup nodeclosed? not if to activenode else drop then ;

\ AST Nodes
: Declare ( name -- ) -1 AST_DECLARE newnode , ;
: Unit ( -- ) -1 AST_UNIT createnode dup to curunit to activenode ;
: Function ( name -- ) 2 AST_FUNCTION newnode , 0 , ;
: Return ( -- ) 1 AST_RETURN newnode ;
: Constant ( n -- ) 0 AST_CONSTANT newnode , ;
: Statements ( -- ) -1 AST_STATEMENTS newnode ;
: ArgSpecs ( -- ) -1 AST_ARGSPECS newnode ;
: Expression ( -- ) -1 AST_EXPRESSION newnode ;
: UnaryOp ( opid -- ) 1 AST_UNARYOP newnode , ;
: Factor ( -- ) 1 AST_FACTOR newnode ;
: BinaryOp ( opid -- node ) 2 AST_BINARYOP newnode , ;
: Assign ( name -- ) 1 AST_ASSIGN newnode , ;
: DeclarationList ( -- ) -1 AST_DECLLIST newnode ;
: Variable ( name -- ) 0 AST_VARIABLE newnode , ;
: FunCall ( name -- ) 0 AST_FUNCALL newnode , ;

: _err ( tok -- )
  stype spc>
  activenode ?dup if astid .x1 spc> then
  abort"  parsing error" ;
: _assert ( tok f -- ) not if _err then ;
: _nextt nextt ?dup not if abort" expecting token!" then ;

: isType? ( tok -- f ) S" int" s= ;
: expectType ( tok -- tok ) dup isType? not if _err then ;
: expectConst ( tok -- n ) dup parse if nip else _err then ;
: isIdent? ( tok -- f )
  dup 1+ c@ a-z? not if drop 0 exit then
  c@+ >r begin ( a ) c@+ identifier? not if drop 0 then next drop 1 ;
: expectIdent ( tok -- tok ) dup isIdent? _assert ;
: expectChar ( tok c -- )
  over 1+ c@ = _assert dup c@ 1 = _assert drop ;

: tokenfromlist ( tok list optbl -- )
  >r ( tok list R:optbl ) over swap sfind ( tok idx )
  tuck 0>= _assert ( idx tok ) drop r> swap wexec ;

: binopswap ( bopid target -- )
  dup dup parentnode ( op tgt tgt parent )
  swap removenode to activenode ( bopid tgt )
  swap BinaryOp ( tgt ) activenode addnode ;

create StatementsTList 1 c, ," }" 6 c, ," return" 3 c, ," int" 0 c,
3 wordtbl StatementsOps ( -- )
'w seqclose ( } )
:w ( return ) Return Expression ;
:w ( int ) DeclarationList ;

ASTIDCNT wordtbl astparsetbl
:w ( Declare )
  '=' expectChar activenode data1 Assign Expression ;
:w ( Unit ) isType? _assert _nextt expectIdent Function ;
:w ( Function ) activenode cslots 2 = if
    '(' expectChar ArgSpecs else
    '{' expectChar Statements then ;
'w _err ( Return )
'w _err ( Constant )
:w ( Statements ) StatementsTList StatementsOps tokenfromlist ;
:w ( ArgSpecs )
  dup S" )" s= if drop seqclose exit then
  begin ( tok )
    expectType drop _nextt expectIdent Declare seqclose
    _nextt dup S" )" s= if drop seqclose exit then
    ',' expectChar _nextt again ;
:w ( Expression )
  activeempty? if dup uopid if UnaryOp drop exit then then
  dup bopid if ( tok binopid )
    swap activeempty? if _err then
    ( bopid tok ) drop
    activenode lastchild ( bopid prev ) dup astid AST_BINARYOP = if
      ( bopid tgt ) 2dup data1 bopprec swap bopprec > if ( bopid tgt )
        firstchild nextsibling then then
    binopswap exit then
  skipnext Factor ;
:w ( UnaryOp ) skipnext Factor ;
:w ( Factor )
  dup isIdent? if
    _nextt ( prevtok newtok ) dup S" (" s= if
      drop FunCall _nextt ')' expectChar
    else
      skipnext swap Variable then
  else
    expectConst Constant then
  seqclose ;
:w ( BinaryOp ) skipnext Factor ;
'w _err ( Assign )
:w ( DeclarationList )
  expectIdent Declare ;
'w _err ( Variable )
'w _err ( FunCall )

: parseast ( -- ) Unit begin
  _skip if 0 to _skip else nextt ?dup not if exit then then
  dup S" ;" s= if drop AST_STATEMENTS closeuntil else
    astparsetbl activenode astid wexec then
  again ;
