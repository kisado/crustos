\ Word Tables

: wordtbl ( n -- a ) create here swap 4 * allot0 1 here c! ;
: w+ ( a -- a+4? ) 4 + dup @ if drop then ;
: :w ( a -- a+4? ) here xtcomp over ! w+ ;
: 'w ( a -- a+4? ) ' over ! w+ ;
: wexec ( tbl idx -- ) 4 * + @ execute ;
