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
\ 7 unused
8 value AST_UNARYOP
\ 9 unused
10 value AST_BINARYOP
11 value AST_ASSIGN
\ 12 unused
13 value AST_VARIABLE
14 value AST_FUNCALL

create astidnames 7 c, ," declare"  4 c, ," unit"     8 c, ," function"
                  6 c, ," return"   8 c, ," constant" 5 c, ," stmts"
                  4 c, ," args"     4 c, ," expr"     7 c, ," unaryop"
                  1 c, ," _"        5 c, ," binop"    6 c, ," assign"
                  1 c, ," _"        3 c, ," var"      4 c, ," call"
                  0 c,

0 value curunit

: astid ( node -- id ) nodeid $3f and ;
: idname ( id -- str ) astidnames slistiter ;

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
'w noop ( Unused )
:w ( UnaryOp ) _[ dup data1 uopchar emit _] ;
'w noop ( Unused )
:w ( BinaryOp ) _[ dup data1 boptoken stype _] ;
'w _s ( Assign )
'w noop ( Unused )
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
      to nexttputback AST_VARIABLE createnode swap , then
  else
    parse if AST_CONSTANT createnode swap , else 0 then
  then ;

: parseExpression ( tok -- exprnode )
  dup uopid if ( tok uopid )
    nip AST_UNARYOP createnode ( uopid node ) swap , ( node )
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

: parseAssign ( parentnode tok -- )
  '=' expectChar
  AST_ASSIGN createnode ( pnode anode ) over data1 ( name ) ,
  dup rot addnode ( anode )
  _nextt parseExpression read; ( anode expr ) swap addnode ;

: parseDeclarationList ( stmtsnode -- )
  _nextt expectIdent
  swap AST_DECLARE newnode swap ,
  _nextt parseAssign ;

: parseArgSpecs ( funcnode -- )
  _nextt '(' expectChar AST_ARGSPECS newnode _nextt ( argsnode tok )
  dup S" )" s= if 2drop exit then
  begin ( argsnode tok )
    expectType drop dup _nextt expectIdent
    swap AST_DECLARE newnode drop ,
    _nextt dup S" )" s= if 2drop exit then
    ',' expectChar _nextt again ;

: parseStatements ( funcnode -- )
  _nextt '{' expectChar AST_STATEMENTS newnode _nextt
  begin ( snode tok )
    dup S" }" s= if 2drop exit then
    dup S" return" s= if
      drop AST_RETURN createnode ( snode rnode ) 2dup swap addnode
      _nextt parseExpression read; ( snode rnode expr )
      swap addnode ( snode )
    else
      expectType drop dup parseDeclarationList then
    _nextt again ;

: parseFunction ( unitnode tok -- )
  swap AST_FUNCTION newnode swap , 0 , ( funcnode )
  dup parseArgSpecs parseStatements ;

: parseast ( -- )
  AST_UNIT createnode dup to curunit
  nextt ?dup not if exit then begin ( unitnode tok )
    isType? _assert _nextt expectIdent over swap parseFunction ( unitnode )
    nextt ?dup not until ( unitnode ) drop ;
