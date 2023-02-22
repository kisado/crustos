: 2drop drop drop ;
: 2dup over over ;
: leave r> r~ 1 >r >r ;
: = - not ;
: > swap < ;
: 0< <<c nip ; : 0>= 0< not ;
: >= < not ; : <= > not ;
: -^ swap - ;

: immediate current 1- dup c@ $80 or swap c! ;
: ['] ' litn ; immediate
: compile ' litn ['] call, call, ; immediate
: if compile (?br) here 4 allot ; immediate
: then here swap ! ; immediate
: else compile (br) here 4 allot here rot ! ; immediate
: begin here ; immediate
: again compile (br) , ; immediate
: until compile (?br) , ; immediate
: next compile (next) , ; immediate

: code word entry ;
: create code compile (cell) ;
: value code compile (val) , ;

: \ begin in< $0a = until ; immediate
: ," begin in< dup '"' = if drop exit then c, again ;
: S" compile (br) here 4 allot here ," tuck here -^ swap
  here swap ! swap litn litn ; immediate

: ( begin
  word dup c@ 1 = if
    1+ c@ ')' = if exit then else drop then
  again ; immediate

: c@+ dup 1+ swap c@ ;
: c!+ tuck c! 1+ ;

create _ $100 allot
: tocstr ( str -- a ) c@+ >r _ r@ move 0 _ r> + c! _ ;
: fclose ( fd -- ) 6 ( close ) swap 0 0 ( close fd 0 0 ) lnxcall drop ;

create _ 'C' c, 'a' c, 'n' c, ''' c, 't' c, $20 c, 'o' c, 'p' c, 'e' c, 'n' c,

: fopen ( fname -- fd )
  tocstr 5 ( open ) swap 0 0 ( open cstr noflag O_RDONLY ) lnxcall
  dup 0< if _ 10 rtype abort then ;
create _ 1 allot
: fread ( fd -- c-or-0 ) 3 ( read ) swap _ 1 lnxcall 1 = if _ c@ else 0 then ;

create _fds $20 allot
_fds value 'curfd
0 value fecho

: >fd ( fd -- ) 'curfd 4 + tuck ! to 'curfd ;
: fd@ ( -- fd ) 'curfd @ ;
: fd~ ( -- ) fd@ fclose 'curfd 4 - to 'curfd ;
: f< ( -- c-or-0 ) fd@ fread ;
: fin< f< ?dup not if ( EOF )
  fd~ fd@ not if ['] iin< to in< then $20 then
  fecho if dup emit then ;
: f<< word fopen >fd ['] fin< to in< ;

f<< init.fs
