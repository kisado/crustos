\ C Compiler Tokenization
\ Throughout the CC code, "tok" means a string, which represents a token.

0 const TOK_KEYWORD
1 const TOK_IDENTIFIER
2 const TOK_CONSTANT
3 const TOK_STRLIT
4 const TOK_SYMBOL

31 stringlist keywords "break" "case" "char" "const" "continue" "default" "do"
                       "double" "else" "enum" "extern" "float" "for" "goto" "if"
                       "inline" "int" "register" "return" "short" "signed"
                       "sizeof" "static" "struct" "switch" "typedef" "union"
                       "unsigned" "void" "volatile" "while"

: isKeyword? ( s -- f ) keywords sfind 0>= ;

\ For symbol parsing, we exploit one particularity: all 2 chars symbols start
\ with a symbol that is also a 1 char symbol and all 3 chars symbols begin with
\ 2 chars that are also a 2 chars symbol.
\ list of 1 char symbols
create symbols1 ," +-*/~&<>=[](){}.%^?:;,"

: isSym1? ( c -- f ) symbols1 22 [c]? 0>= ;

\ list of 2 chars symbols
create symbols2 ," <=>===!=&&||++---><<>>+=-=*=/=%=&=^=|="

: isSym2? ( c1 c2 -- f )
  A>r 19 >r symbols2 >A begin ( c1 c2 )
    over Ac@+ = over Ac@+ = and if 2drop r~ r>A 1 exit then
  next 2drop 0 r>A ;

\ are c1/c2 either << or >>?
: is<<>>? ( c1 c2 -- f )
  dup '<' = over '>' = or rot> ( f1 c1 c2 ) = and ( f ) ;

alias in< cc<
0 value putback
: _err abort" tokenization error" ;
: _cc< ( -- c ) putback ?dup if 0 to putback else cc< then ;

create _ 6 c, ," AZaz__"
: identifier1st? ( c -- f ) _ rmatch ;

create _ 8 c, ," 09AZaz__"
: identifier? ( c -- f ) _ rmatch ;

\ advance to the next non-whitespace and return the char encountered.
\ if end of stream is reached, c is 0
: tonws ( -- c ) 0 begin ( c )
    drop _cc< dup dup EOF <= swap ws? not or until ( c )
  dup EOF <= if drop 0 then ;

: _writesym ( c3? c2? c1 len -- str )
  4 scratchallot dup >r ( c3? c2? c1 len a )
  over >r c!+ ( c a ) begin c!+ next drop r> ( str ) ;

\ Returns the next token as a string or 0 when there's no more tokens to consume
: nextt ( -- tok-or-0 )
  tonws dup not if ( EOF ) exit then ( c )
  dup isSym1? if ( c )
    cc< 2dup isSym2? if ( c1 c2 )
      2dup is<<>>? if ( c1 c2 )
        cc< dup '=' if ( c1 c2 '=' )
          rot> swap 3 ( '=' c2 c1 len ) _writesym
        else ( c1 c2 c3 )
          to putback swap 2 ( c2 c1 len ) _writesym then
      else swap 2 ( c2 c1 len ) _writesym then
    else ( c1 c2 ) to putback 1 ( c1 len ) _writesym then
  else ( c ) \ not a symbol
    dup identifier? not if _err then
    A>r LNSZ scratchallot >A A>r ( R:tok ) 0 Ac!+ ( len placeholder )
    begin ( c )
      Ac!+ cc< dup identifier? not until to putback
    r> ( buf ) A> over 1+ - ( tok len ) over c! r>A
  then ;
