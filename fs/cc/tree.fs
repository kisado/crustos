\ C Compiler Tree Structure

: nodeid ( node -- id ) c@ ;
: nodeid! ( id node -- ) c! ;
: cslots ( node -- slots ) 1+ 1+ c@ ;
: cslots! ( slots node -- ) 1+ 1+ c! ;
: cslots- ( node -- ) dup cslots 1- swap cslots! ;
: cslots+ ( node -- ) dup cslots 1+ swap cslots! ;
: closenode ( node -- ) 0 swap cslots! ;
: nodeclosed? ( node -- f ) cslots not ;
: parentnode ( node -- parent ) 3 + @ ;
: parentnode! ( parent node -- ) 3 + ! ;
: firstchild ( node -- child ) 7 + @ ;
: firstchild! ( child node -- ) 7 + ! ;
: nextsibling ( node -- next ) 11 + @ ;
: nextsibling! ( next node -- ) 11 + ! ;
: prevsibling ( node -- prev ) 15 + @ ;
: prevsibling! ( prev node -- ) 15 + ! ;
: 'data ( node -- 'data ) 19 + ;
: data1 ( node -- n ) 'data @ ;
: data1! ( n node -- ) 'data ! ;
: data2 ( node -- n ) 'data 4 + @ ;
: data2! ( n node -- ) 'data 4 + ! ;
: data3 ( node -- n ) 'data 8 + @ ;
: data3! ( n node -- ) 'data 8 + ! ;
: data4 ( node -- n ) 'data 12 + @ ;
: data4! ( n node -- ) 'data 12 + ! ;

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
: createnode ( slots id -- node ) here >r c, 0 c, c, 16 allot0 r> ;
: addnode ( node parent -- )
  dup nodeclosed? if abort" node is closed" then
  2dup swap parentnode! dup cslots- ( node parent )
  dup lastchild ?dup if ( n p lc )
    nip ( n lc ) 2dup nextsibling! swap prevsibling!
  else
    ( n p ) firstchild! then ;
: removenode ( node -- )
  dup parentnode cslots+
  dup parentnode firstchild over = if
    dup nextsibling over parentnode firstchild!
  else
    dup nextsibling over prevsibling nextsibling! then
  dup nextsibling if
    dup prevsibling swap nextsibling prevsibling!
  else drop then ;
