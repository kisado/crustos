\ Core Forth words which are hard to live without

\ Compiling Words
: [compile] ' call, ; immediate
: const code litn exit, ;
4 const CELLSZ
: alias ' code compile (alias) , ;
: doer code compile (does) CELLSZ allot ;
: does> r> ( exit current definition ) current 5 + ! ;

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
: =><= ( n l h -- f ) over - rot> ( h n l ) - >= ;

\ Emitting
$20 const SPC $0d const CR $0a const LF
$08 const BS  $04 const EOF

: nl> CR emit LF emit ;
: spc> SPC emit ;
: stype ( str -- ) c@+ rtype ;
: ," begin in< dup '"' = if drop exit then c, again ;
: S" ( comp: -- ) ( not-comp: -- str )
  compiling if compile (s) else here then
  here 1 allot here ," here -^ ( 'len len ) swap c! ; immediate
: ." [compile] S" compile stype ; immediate
: abort" [compile] ." compile abort ; immediate

\ Sequences
: [c]? ( c a u -- i )
  ?dup not if 2drop -1 exit then A>r over >r >r >A ( c )
  begin dup Ac@+ = if leave then next ( c )
  A- Ac@ = if A> r> - ( i ) else r~ -1 then r>A ;

\ Dictionary
: prevword ( w -- w ) 5 - @ ;
: wordlen ( w -- len ) 1- c@ $3f and ;
: wordname[] ( w -- sa sl )
  dup wordlen swap 5 - over - ( sl sa ) swap ;

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
: dump ( a -- )
  A>r >A 8 >r begin
    ':' emit A> dup .x spc> ( a )
    8 >r begin Ac@+ .x1 Ac@+ .x1 spc> next ( a ) >A
    16 >r begin Ac@+ dup SPC - $5e > if drop '.' then emit next
  nl> next r>A ;
