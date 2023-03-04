\ C Compiler Abstract Syntax Tree
\ requires cc/tok.fs and cc/ops.fs

\ Unary Operators
\ ID  SYM NAME
\ 0   -   negate
\ 1   ~   complement
\ 2   !   not

3 const UOPSCNT
create uopssyms ," -~!?"

: uopid ( tok -- opid? f )
  c@+ 1 = if c@ uopssyms UOPSCNT [c]? dup 0< if drop 0 else 1 then
    else drop 0 then ;
: uopchar ( opid -- c ) UOPSCNT min uopssyms + c@ ;

2 const LOPSCNT
create lopssyms ," &*?"

: lopid ( tok -- opid? f )
  c@+ 1 = if c@ lopssyms LOPSCNT [c]? dup 0< if drop 0 else 1 then
    else drop 0 then ;
: lopchar ( opid -- c ) LOPSCNT min lopssyms + c@ ;

12 const BOPSCNT
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

15 const ASTIDCNT
0 const AST_DECLARE
1 const AST_UNIT
2 const AST_FUNCTION
3 const AST_RETURN
4 const AST_CONSTANT
5 const AST_STATEMENTS
6 const AST_ARGSPECS
7 const AST_LVALUE
8 const AST_UNARYOP
9 const AST_ASSIGN
10 const AST_BINARYOP
11 const AST_LVALUEOP
12 const AST_IF
\ 13 unused
14 const AST_FUNCALL

create astidnames 7 c, ," declare"  4 c, ," unit"     8 c, ," function"
                  6 c, ," return"   8 c, ," constant" 5 c, ," stmts"
                  4 c, ," args"     6 c, ," lvalue"   7 c, ," unaryop"
                  6 c, ," assign"   5 c, ," binop"    6 c, ," lvalop"
                  2 c, ," if"       1 c, ," _"        4 c, ," call"
                  0 c,

0 value curunit

: astid ( node -- id ) nodeid $3f and ;
: idname ( id -- str ) astidnames slistiter ;

: _[ '[' emit ;
: _] ']' emit ;
: _s _[ dup data1 stype _] ;
: _i _[ dup data1 .x _] ;

ASTIDCNT wordtbl astdatatbl ( node -- node )
:w ( Declare ) _[ dup data1 stype spc> dup data2 .x1 _] ;
'w noop ( Unit )
'w _s ( Function )
'w noop ( Return )
'w _i ( Constant )
'w noop ( Statements )
'w noop ( ArgSpecs )
'w _s ( LValue )
:w ( UnaryOp ) _[ dup data1 uopchar emit _] ;
'w noop ( Assign )
:w ( BinaryOp ) _[ dup data1 boptoken stype _] ;
:w ( LvalueOp ) _[ dup data1 lopchar emit _] ;
'w noop ( Unused )
'w noop ( Unused )
'w _s ( FunCall )

: printast ( node -- )
  ?dup not if ." null" exit then
  dup astid dup AST_FUNCTION = if nl> then idname stype
  astdatatbl over astid wexec
  firstchild ?dup if
    '(' emit begin
      dup printast nextsibling dup if ',' emit then ?dup not until
    ')' emit then ;

: newnode ( parent astid -- newnode )
  createnode ( parent node ) dup rot addnode ( node ) ;

\ if not 0, next '_nextt' call will fetch token from here
0 value nexttputback

: _err ( tok -- )
  stype spc> abort"  parsing error" ;
: _assert ( tok f -- ) not if _err then ;
: _nextt
  nexttputback ?dup if 0 to nexttputback exit then
  nextt ?dup not if abort" expecting token" then ;

: isType? ( tok -- f ) S" int" s= ;
: expectType ( tok -- tok ) dup isType? not if _err then ;
: expectConst ( tok -- n ) dup parse if nip else _err then ;
: isIdent? ( tok -- f )
  dup 1+ c@ a-z? not if drop 0 exit then
  c@+ >r begin ( a ) c@+ identifier? not if drop 0 then next drop 1 ;
: expectIdent ( tok -- tok ) dup isIdent? _assert ;
: expectChar ( tok c -- )
  over 1+ c@ = _assert dup c@ 1 = _assert drop ;
: read; ( -- ) _nextt ';' expectChar ;

\ Parse Words

: parseLvalue ( tok -- lvnode )
  dup lopid if ( tok opid )
    nip AST_LVALUEOP createnode swap , ( lopnode )
    _nextt parseLvalue ( lopnode lvnode ) over addnode
  else ( tok ) expectIdent AST_LVALUE createnode swap , then ;

\ Parse a constant, variable or function call
: parseFactor ( tok -- node-or-0 )
  dup isIdent? if
    _nextt ( prevtok newtok ) dup S" (" s= if
      drop AST_FUNCALL createnode swap , begin ( node )
        _nextt dup parseFactor ?dup if
          nip over addnode
          _nextt dup S" ," s= if drop else to nexttputback then 0
        else
          ')' expectChar 1 then until ( node )
    else
      to nexttputback parseLvalue then
  else
    parse if AST_CONSTANT createnode swap , else 0 then
  then ;

: parseUnaryOp ( tok -- opid? astid? f )
  dup uopid if
    nip AST_UNARYOP 1 else lopid if
      AST_LVALUEOP 1 else 0 then then ;

: parseExpression ( tok -- exprnode )
  dup parseUnaryOp if ( tok opid astid )
    createnode ( tok opid node ) swap , nip ( node )
    _nextt parseExpression ( uopnode expr )
    over addnode ( node )
  else ( tok )
    parseFactor ?dup _assert _nextt ( factor nexttok )
    dup bopid if ( factor tok binop )
      nip ( factor binop ) AST_BINARYOP createnode swap , ( factor node )
      tuck addnode _nextt ( binnode tok )
      begin ( bn tok )
        parseFactor ?dup _assert _nextt ( bn factor tok ) dup bopid if ( bn fn tok bopid )
          nip AST_BINARYOP createnode swap , ( bn1 fn bn2 )
          rot ( fn bn2 bn1 ) over data1 bopprec over data1 bopprec < if
            rot> tuck addnode ( bn1 bn2 ) dup rot addnode ( bn2->bn )
          else
            rot over addnode ( bn2 bn1 ) over addnode ( bn2->bn )
          then ( bn )
          _nextt 0 ( bn tok 0 )
        else ( bn fn tok )
          rot> over addnode swap 1 ( bn tok 1 ) then
      until ( bn tok )
      swap rootnode swap
    then
    ( node tok ) to nexttputback
  then ;

: parseDeclare ( parentnode -- dnode )
  0 begin ( pnode *lvl )
    _nextt dup S" *" s= if drop 1+ 0 else 1 then until ( pnode *lvl tok )
  expectIdent rot ( *lvl name pnode ) AST_DECLARE newnode ( *lvl name dnode )
  swap , swap , ( dnode ) ;

: parseDeclarationList ( stmtsnode -- )
  parseDeclare _nextt '=' expectChar dup data1 ( dnode name )
  swap parentnode AST_ASSIGN newnode ( name anode )
  AST_LVALUE newnode ( name lvnode ) swap , parentnode ( anode )
  _nextt parseExpression read; ( anode expr ) swap addnode ;

: parseArgSpecs ( funcnode -- )
  _nextt '(' expectChar AST_ARGSPECS newnode _nextt ( argsnode tok )
  dup S" )" s= if 2drop exit then
  begin ( argsnode tok )
    expectType drop dup parseDeclare drop
    _nextt dup S" )" s= if 2drop exit then
    ',' expectChar _nextt again ;

\ Parse an LValue
: parseAssign ( parent tok -- )
  swap AST_ASSIGN newnode swap ( anode tok )
  parseLvalue ( anode lvnode ) over addnode ( anode )
  _nextt '=' expectChar ( anode )
  _nextt parseExpression read; ( anode expr ) swap addnode ;

alias noop parseStatements

create statementnames 6 c, ," return" 2 c, ," if" 0 c,
2 wordtbl statementhandler
:w ( return )
  dup AST_RETURN newnode ( snode rnode )
  _nextt parseExpression read; ( snode rnode expr )
  swap addnode ( snode ) ;
:w ( if ) dup AST_IF newnode ( snode ifnode )
  _nextt '(' expectChar
  _nextt parseExpression ( sn ifn expr ) over addnode
  _nextt ')' expectChar
  parseStatements ;

: _ ( parentnode -- )
  _nextt '{' expectChar AST_STATEMENTS newnode _nextt
  begin ( snode tok )
    dup S" }" s= if 2drop exit then
    dup statementnames sfind dup 0< if ( snode tok -1 )
      drop dup isType? if drop dup parseDeclarationList else ( snode tok )
        over rot> parseAssign then ( snode )
    else ( snode tok idx ) nip statementhandler swap wexec then ( snode )
    _nextt again ;
current to parseStatements

: parseFunction ( unitnode tok -- )
  swap AST_FUNCTION newnode swap , 0 , ( funcnode )
  dup parseArgSpecs parseStatements ;

: parseast ( -- )
  AST_UNIT createnode dup to curunit
  nextt ?dup not if exit then begin ( unitnode tok )
    isType? _assert _nextt expectIdent over swap parseFunction ( unitnode )
    nextt ?dup not until ( unitnode ) drop ;
