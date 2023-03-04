\ String Utilities
\ All string utitilies operate on an "active string", which is set with `>s`

$100 value STR_MAXSZ

: ws? ( c -- f ) SPC <= ;
: s= ( s1 s2 -- f ) over c@ 1+ []= ;
: s) ( str -- a ) c@+ + ;

: sfind ( str list -- idx ) -1 rot> begin ( idx s a )
  rot 1+ rot> ( idx s a )
  2dup s= if ( found ) 2drop exit then
  s) dup c@ not until ( idx s a ) 2drop drop -1 ;

: slistiter ( idx list -- str )
  swap dup if >r begin s) next else drop then ;

\ Given a list of character ranges, which are given in the form of a string of
\ character pairs, return whether the specified character is in one of the ranges.
: rmatch ( c range -- f )
  A>r >A Ac@+ >> ( len/2 ) >r begin ( c )
    dup Ac@+ Ac@+ ( c c lo hi ) =><= if drop r~ r>A 1 exit then
  next ( c ) drop 0 r>A ;

create _ 2 c, ," 09"
: 0-9? ( c -- f ) _ rmatch ;
create _ 4 c, ," AZaz"
: A-Za-z? ( c -- f ) _ rmatch ;
create _ 6 c, ," AZaz09"
: alnum? ( c -- f ) _ rmatch ;

\ Create a list of strings (same format as sfind above) with the specified
\ number of elements. Each element must be "quoted" with no space (unless you
\ want them in the string) in the quotes.
\
\ Example: 3 stringlist mylist "foo" "bar" "hello world!"
: stringlist create >r begin
  0 begin drop in< dup ws? not until ( c )
  '"' = not if '"' emit abort"  expected" then
  [compile] S" drop next 0 c, ;
