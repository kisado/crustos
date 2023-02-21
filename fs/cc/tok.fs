\ C Compiler Tokenization
\ Throughout the CC code, "tok" means a string, which represents a token.

alias in< cc<
0 value putback
: _err abort" tokenization error" ;
: _cc< ( -- c ) putback ?dup if 0 to putback else cc< then ;
create buf LNSZ allot
: 0-9? ( c -- f ) '0' - 10 < ;
: a-z? ( c -- f ) dup 'A' - 26 < swap 'a' - 26 < or ;
: identifier? ( c -- f ) dup 0-9? swap a-z? or ;

create special1st ," (){}!~+-*/<>=&|;"
create special2nd ," =&|"

\ advance to the next non-whitespace and return the char encountered.
\ if end of stream is reached, c is 0
: tonws ( -- c ) 0 begin ( c )
    drop _cc< dup dup EOF <= swap ws? not or until ( c )
  dup EOF <= if drop 0 then ;

\ Returns the next token as a string or 0 when there's no more tokens to consume.
: nextt ( -- tok-or-0 ) tonws dup if ( c )
  A>r LNSZ scratchallot >A A>r ( R:tok ) 0 Ac!+ ( len placeholder )
  dup identifier? if begin ( c )
    Ac!+ cc< dup identifier? not until to putback
  else
    dup special1st 16 [c]? 0< if _err then
    Ac!+ cc<
    dup special2nd 3 [c]? 0< if to putback else Ac!+ then
  then
  r> ( buf ) A> over 1+ - ( tok len ) over c! r>A then ;
