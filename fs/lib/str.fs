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
