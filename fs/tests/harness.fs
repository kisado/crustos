exitonabort

\ `#` means `assert`
: # ( f -- ) not if abort" assert failed" then ;
: #eq ( n n -- ) 2dup = if 2drop else swap .x ."  != " .x abort then ;
: testbegin 1 to fecho ;
: testend 0 to fecho scnt 0 #eq ;
