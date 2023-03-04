\ C Compiler Abstract Syntax Tree
\ requires cc/tok.fs and cc/ops.fs

\ Unary Operators
7 const UOPSCNT
UOPSCNT stringlist UOPTlist "-" "~" "!" "&" "*" "++" "--"

: uopid ( tok -- opid? f )
  UOPTlist sfind dup 0< if drop 0 else 1 then ;
: uoptoken ( opid -- tok ) UOPTlist slistiter ;

\ Postfix Operators
2 const POPSCNT
POPSCNT stringlist POPTlist "++" "--"

: popid ( tok -- opid? f )
  POPTlist sfind dup 0< if drop 0 else 1 then ;
: poptoken ( opid -- tok ) POPTlist slistiter ;

\ Binary Operators
13 const BOPSCNT
BOPSCNT stringlist BOPTlist
"+" "-" "*" "/" "<" ">" "<=" ">=" "==" "!=" "&&" "||" "="

create bopsprectbl  1 c, 1 c, 0 c, 0 c, 2 c, 2 c, 2 c, 2 c,
                    3 c, 3 c, 4 c, 4 c, 5 c,

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
9 const AST_POSTFIXOP
10 const AST_BINARYOP
\ 11 unused
12 const AST_IF
\ 13 unused
14 const AST_FUNCALL

ASTIDCNT stringlist astidnames
"declare" "unit" "function" "return" "constant" "stmts" "args" "lvalue"
"unaryop" "postop" "binop" "_" "if" "_" "call"

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
:w ( UnaryOp ) _[ dup data1 uoptoken stype _] ;
'w noop ( Unused )
:w ( BinaryOp ) _[ dup data1 boptoken stype _] ;
'w noop ( Unused )
'w noop ( If )
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

: _err ( -- ) abort" parsing error" ;
: _assert ( f -- ) not if _err then ;
: _nextt
  nexttputback ?dup if 0 to nexttputback exit then
  nextt ?dup not if abort" expecting token" then ;

: isType? ( tok -- f ) S" int" s= ;
: expectType ( tok -- tok ) dup isType? not if _err then ;
: expectConst ( tok -- n ) dup parse if nip else _err then ;
: isIdent? ( tok -- f )
  dup 1+ c@ identifier1st? not if drop 0 exit then
  c@+ >r begin ( a ) c@+ identifier? not if drop 0 then next drop 1 ;
: expectIdent ( tok -- tok ) dup isIdent? _assert ;
: expectChar ( tok c -- )
  over 1+ c@ = _assert dup c@ 1 = _assert drop ;
: read; ( -- ) _nextt ';' expectChar ;

\ Parse Words

: parsePostfixOp ( tok -- node-or-0 )
  dup popid if ( tok opid )
    nip AST_POSTFIXOP createnode swap , ( node )
  else to nexttputback 0 then ;

\ A Factor can be:
\ 1. a constant
\ 2. an lvalue
\ 3. a unaryop/postfixop containing a factor
\ 4. a function call
: parseFactor ( tok -- node-or-0 )
  dup uopid if ( tok opid )
    nip AST_UNARYOP createnode swap , ( opnode )
    _nextt parseFactor ?dup _assert over addnode ( opnode )
  else ( tok )
    dup isIdent? if                                           \ LValue or FunCall
      _nextt ( prevtok newtok ) dup S" (" s= if               \ FunCall
        drop AST_FUNCALL createnode swap , begin ( node )
          _nextt dup parseFactor ?dup if                      \ an argument
            nip over addnode
            _nextt dup S" ," s= if drop else to nexttputback then 0
          else                                                \ not an argument
            ')' expectChar 1 then until ( node )
      else ( prevtok newtok )                                 \ LValue
        swap AST_LVALUE createnode swap , ( tok lvnode )
        swap parsePostfixOp ( lvnode node-or-0 ) ?dup if ( lvnode opnode )
          tuck addnode then ( lv-or-op-node ) then
    else                                                      \ Constant
      parse if AST_CONSTANT createnode swap , else 0 then
    then
  then ;

\ An expression can be 2 things:
\ 1. a factor
\ 2. a binaryop containing two expressions
: parseExpression ( tok -- exprnode )
  \ tok is expected to be a factor
  parseFactor ?dup _assert _nextt ( factor nexttok )
  dup bopid if ( factor tok binop )
    nip ( factor binop ) AST_BINARYOP createnode swap , ( factor node )
    tuck addnode _nextt ( binnode tok )

    \ consume tokens until binops stop coming
    begin ( bn tok )
      parseFactor ?dup _assert _nextt ( bn factor tok ) dup bopid if ( bn fn tok bopid )
        nip AST_BINARYOP createnode swap , ( bn1 fn bn2 )

        \ find best precedence
        rot ( fn bn2 bn1 ) over data1 bopprec over data1 bopprec < if
          rot> tuck addnode ( bn1 bn2 ) dup rot addnode ( bn2->bn )
        else
          rot over addnode ( bn2 bn1 ) over addnode ( bn2->bn )
        then ( bn )
        _nextt 0 ( bn tok 0 )
      else ( bn fn tok )    \ not a binop
        rot> over addnode swap 1 ( bn tok 1 ) then
    until ( bn tok )

    \ bn not result, rootnode is
    swap rootnode swap
  then
  ( node tok ) to nexttputback ;

: parseDeclare ( parentnode -- dnode )
  0 begin ( pnode *lvl )
    _nextt dup S" *" s= if drop 1+ 0 else 1 then until ( pnode *lvl tok )
  expectIdent rot ( *lvl name pnode ) AST_DECLARE newnode ( *lvl name dnode )
  swap , swap , ( dnode ) ;

: parseDeclarationList ( stmtsnode -- )
  parseDeclare _nextt '=' expectChar dup data1 ( dnode name )
  swap parentnode AST_BINARYOP newnode ( name anode ) 12 ( = ) ,
  AST_LVALUE newnode ( name lvnode ) swap , parentnode ( anode )
  _nextt parseExpression read; ( anode expr ) swap addnode ;

: parseArgSpecs ( funcnode -- )
  _nextt '(' expectChar AST_ARGSPECS newnode _nextt ( argsnode tok )
  dup S" )" s= if 2drop exit then
  begin ( argsnode tok )
    expectType drop dup parseDeclare drop
    _nextt dup S" )" s= if 2drop exit then
    ',' expectChar _nextt again ;

alias noop parseStatements ( funcnode -- )

2 stringlist statementnames "return" "if"
2 wordtbl statementhandler ( snode -- snode )
:w ( return )
  dup AST_RETURN newnode ( snode rnode )
  _nextt parseExpression read; ( snode rnode expr )
  swap addnode ( snode ) ;
:w ( if ) dup AST_IF newnode ( snode ifnode )
  _nextt '(' expectChar
  _nextt parseExpression ( sn ifn expr ) over addnode
  _nextt ')' expectChar
  dup parseStatements ( snode ifnode )
  _nextt dup S" else" s= if ( sn ifn tok )
    drop parseStatements else
    to nexttputback drop then ;

: _ ( parentnode -- )
  _nextt '{' expectChar AST_STATEMENTS newnode _nextt
  begin ( snode tok )
    dup S" }" s= if 2drop exit then
    dup statementnames sfind dup 0< if ( snode tok -1 )
      drop dup isType? if drop dup parseDeclarationList else ( snode tok )
        parseExpression over addnode read; then ( snode )
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
