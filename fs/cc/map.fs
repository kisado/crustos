\ C Compiler Variable map

\ This tree is generated after AST parsing, before code generation to map
\ some names to some numbers, namely:
\ 1. Function Addresses
\ 2. Local Variable Stack Frame (SF) offsets

3 value MAPIDCNT
0 value MAP_UNIT
1 value MAP_FUNCTION
2 value MAP_VARIABLE

create mapidnames 4 c, ," unit" 8 c, ," function" 3 c, ," var"
                  0 c,

0 value curmap
0 value activenode

: _err ( -- ) abort" mapping error" ;
: newnode
  createnode dup activenode addnode ( node )
  dup nodeclosed? not if to activenode else drop then ;

: Unit ( -- ) -1 MAP_UNIT createnode dup to curmap to activenode ;
: Function ( name -- ) -1 MAP_FUNCTION newnode , 0 , 0 , 0 , ;
: Variable ( offset name -- ) 0 MAP_VARIABLE newnode , , ;

: _[ '[' emit ;
: _] ']' emit ;

MAPIDCNT wordtbl mapdatatbl ( node -- node )
'w noop ( Unit )
:w ( Function ) _[
  dup data1 data1 stype ',' emit dup data2 .x ',' emit dup data3 .x _] ;
:w ( Variable ) _[ dup data1 stype ',' emit dup data2 .x _] ;

: printmap ( node -- )
  ?dup not if ." null" exit then
  dup nodeid mapidnames slistiter stype
  mapdatatbl over nodeid wexec
  firstchild ?dup if
    '(' emit begin
      dup printmap nextsibling dup if ',' emit then ?dup not until
    ')' emit then ;

\ Return node SF size and then increase it by 4.
: funsfsz+ ( node -- sfsz )
  dup nodeid MAP_FUNCTION = not if _err then
  dup data2 tuck ( sz n sz ) 4 + swap 'data 4 + ! ;

: findvarinmap ( name node -- varnode )
  dup nodeid MAP_FUNCTION = not if _err then
  firstchild dup if begin ( name node )
    2dup data1 s= if nip exit then nextsibling dup not
  until then ( name node ) nip ;

: findfuncinmap ( name node -- funcnode )
  dup nodeid MAP_UNIT = not if _err then
  firstchild dup if begin ( name node )
    2dup data1 data1 s= if nip exit then nextsibling dup not
  until then ( name node ) nip ;

: mapfunction ( astfunction -- )
  dup Function activenode over data2! dup begin ( astfunc astfunc )
    AST_DECLARE nextnodeid dup if
      dup parentnode nodeid AST_ARGSPECS = if
        activenode data3 4 + activenode data3! then
      dup data1 activenode funsfsz+ swap Variable 0 else 1 then
  until 2drop ;

: mapunit ( astunit -- )
  Unit firstchild ?dup not if exit then begin ( astnode )
    dup nodeid AST_FUNCTION = if dup mapfunction then
    nextsibling ?dup not until ;
