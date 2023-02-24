\ C Compiler Variable map

\ This tree is generated after AST parsing, before code generation to map
\ some names to some numbers, namely:
\ 1. Function Addresses
\ 2. Local Variable Stack Frame (SF) offsets

\ Those maps are xdicts. The first level is a dictionary of functions. It is
\ located at "curmap". Each function in the AST results in an entry in this
\ map. Each entry has this structure:
\ 4b link to AST_FUNCTION node
\ 4b SF size, which *includes* args size. SF-args=local vars
\ 4b args size
\ 4b address
\ 4b variable declaration xdict

newxdict curmap

: fmap.astnode @ ;
: fmap.sfsize 4 + @ ;
: fmap.sfsize+ ( fmap -- sfoff )
  dup fmap.sfsize swap 4 + ( sz a ) 4 swap +! ( sz ) ;
: fmap.argsize 8 + @ ;
: fmap.argsize+ ( n fmap -- ) 8 + +! ;
: fmap.address 12 + @ ;
: fmap.address! 12 + ! ;
: fmap.vmap 16 + ;
: vmap.sfoff @ ;

: _err ( -- ) abort" mapping error" ;
: printmap ( -- )
  curmap @ ?dup not if exit then begin ( w )
    dup wordname[] rtype spc>
    dup fmap.sfsize .x spc> dup fmap.argsize .x nl>
    dup fmap.vmap @ ?dup if begin ( w vmap )
      spc> spc> dup wordname[] rtype spc> dup vmap.sfoff .x nl>
      prevword ?dup not until then ( w )
    prevword ?dup not until ;

: Function ( astnode -- entry )
  dup data1 ( name ) curmap xentry ( astnode )
  here swap , 16 allot0 ( entry ) ;
: Variable ( offset name -- ) curmap @ fmap.vmap xentry , ;

: findvarinmap ( name funcentry -- varentry )
  fmap.vmap xfind not if _err then ;

: findfuncinmap ( name -- funcentry ) curmap xfind not if _err then ;

: mapfunction ( astfunction -- )
  dup Function ( astfunc fmap ) over data2! ( astfunc ) begin ( curnode )
    AST_DECLARE nextnodeid dup if ( astdecl )
      dup parentnode nodeid AST_ARGSPECS = if
        4 curmap @ fmap.argsize+ then
      dup data1 curmap @ fmap.sfsize+ swap Variable 0 else 1 then
  until ( curnode ) drop ;

: mapunit ( astunit -- )
  0 curmap !
  firstchild ?dup not if exit then begin ( astnode )
    dup nodeid AST_FUNCTION = if dup mapfunction then
    nextsibling ?dup not until ;
