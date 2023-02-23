\ C Compiler Tree Structure

: nodeid ( node -- id ) c@ ;
: nodeid! ( id node -- ) c! ;
: parentnode ( node -- parent ) 4 + @ ;
: parentnode! ( parent node -- ) 4 + ! ;
: firstchild ( node -- child ) 8 + @ ;
: firstchild! ( child node -- ) 8 + ! ;
: nextsibling ( node -- next ) 12 + @ ;
: nextsibling! ( next node -- ) 12 + ! ;
: prevsibling ( node -- prev ) 16 + @ ;
: prevsibling! ( prev node -- ) 16 + ! ;
: 'data ( node -- 'data ) 20 + ;
: data1 ( node -- n ) 'data @ ;
: data1! ( n node -- ) 'data ! ;
: data2 ( node -- n ) 'data 4 + @ ;
: data2! ( n node -- ) 'data 4 + ! ;
: data3 ( node -- n ) 'data 8 + @ ;
: data3! ( n node -- ) 'data 8 + ! ;
: data4 ( node -- n ) 'data 12 + @ ;
: data4! ( n node -- ) 'data 12 + ! ;
: rootnode ( n -- n ) dup parentnode if parentnode rootnode then ;

: nextnode ( ref node -- ref next )
  dup firstchild ?dup if nip else begin ( ref node )
    2dup = if drop 0 exit then
    dup nextsibling ?dup if nip exit then
    parentnode 2dup = until drop 0
  then ;
: nextnodeid ( ref node id -- ref node )
  >r begin nextnode dup not if r~ exit then dup nodeid r@ = until r~ ;
: parentnodeid ( node id -- node )
  >r begin parentnode dup not if r~ exit then dup nodeid r@ = until r~ ;

: lastchild ( node -- child )
  firstchild dup if begin dup nextsibling ?dup not if exit then nip again then ;
: nodedepth ( node -- n ) firstchild ?dup if nodedepth 1+ else 0 then ;
: childcount ( node -- n )
  0 swap firstchild ?dup if begin swap 1+ swap nextsibling ?dup not until then ;
: createnode ( id -- node ) here >r c, 19 allot0 r> ;
: addnode ( node parent -- )
  2dup swap parentnode! ( node parent )
  dup lastchild ?dup if ( n p lc )
    nip ( n lc ) 2dup nextsibling! swap prevsibling!
  else
    ( n p ) firstchild! then ;
: removenode ( node -- )
  dup parentnode firstchild over = if
    dup nextsibling over parentnode firstchild!
  else
    dup nextsibling over prevsibling nextsibling! then
  dup nextsibling if
    dup prevsibling swap nextsibling prevsibling!
  else drop then ;
