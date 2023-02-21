\ Readline Interface

64 value LNSZ
create in( LNSZ allot
: in) in( 64 + ;
in) value in>

: bs? BS over = swap $7f = or ;

: emitv ( c -- ) dup SPC - $5f < if emit else drop then ;
: lntype ( ptr c -- ptr+1 f )
  dup bs? if ( ptr c )
    drop dup in( > if 1- BS emit then spc> BS emit 0
  else ( ptr c )
    dup emitv dup rot c!+ ( c ptr+1 ) dup in) = rot SPC < or ( ptr+1 f )
  then ;

: rdln
  in( LNSZ SPC fill S"  ok" stype nl>
  in( begin key lntype until drop nl> ;
: rdln<? ( -- c-or-0 )
  in> in) < if in> c@+ swap to in> else 0 then ;
: rdln< ( -- c ) rdln<? ?dup not if
  rdln in( to in> SPC then ;
: rdln$ ['] rdln< dup to iin< to in< in) to in> ;
