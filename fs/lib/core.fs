\ Core Forth words which are hard to live without

\ Compiling Words
: [compile] ' call, ; immediate
: const code litn exit, ;
: alias ' code compile (alias) , ;
: doer code compile (does) 4 allot ;

\ Memory
: Ac@+ Ac@ A+ ;
: Ac!+ Ac! A+ ;
: fill ( a u b -- *A* ) rot> >r >A begin dup Ac!+ next drop ;
: allot0 ( n -- ) here over 0 fill allot ;

\ Arithmetic
: << <<c drop ;
: >> >>c drop ;
: <> ( n n -- l h ) 2dup > if swap then ;
: min <> drop ;
: max <> nip ;

\ Emitting
$20 const SPC $0d const CR $0a const LF
$08 const BS  $04 const EOF

: nl> CR emit LF emit ;
: spc> SPC emit ;
: stype ( str -- ) c@+ rtype ;
: ," begin in< dup '"' = if drop exit then c, again ;
: S" compile (s) here 1 allot here ," here -^ ( 'len len ) swap c! ; immediate
: ." [compile] S" compile stype ; immediate
: abort" [compile] ." compile abort ; immediate

\ Sequences
: [c]? ( c a u -- i )
  ?dup not if 2drop -1 exit then A>r over >r >r >A ( c )
  begin dup Ac@+ = if leave then next ( c )
  A- Ac@ = if A> r> - ( i ) else r~ -1 then r>A ;

\ Number Formatting
create _ ," 0123456789abcdef"
: .xh $f and _ + c@ emit ;
: .x1 dup 4 rshift .xh .xh ;
: .x2 dup 8 rshift .x1 .x1 ;
: .x dup 16 rshift .x2 .x2 ;

\ Diagnostic
: psdump scnt not if exit then
  scnt >A begin dup .x spc> >r scnt not until
  begin r> scnt A> = until ;
: .S ( -- )
  S" SP " stype scnt .x1 spc> S" RS " stype rcnt .x1 spc>
  S" -- " stype stack? psdump ;
: does> r> ( exit current definition ) current 5 + ! ;
