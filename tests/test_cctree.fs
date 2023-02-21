\ C Compiler Tree Tests
f<< cc/tree.fs

2 1 createnode value n1
n1 nodeid 1 #eq
n1 nodeclosed? not #

-1 2 createnode value n2
n2 n1 addnode
n2 nodeid 2 #eq
n2 parentnode n1 #eq
n2 nextsibling 0 #eq
n2 prevsibling 0 #eq
n2 firstchild 0 #eq
n1 firstchild n2 #eq

0 3 createnode 42 , value n3
n3 n1 addnode
n3 nodeid 3 #eq
n3 'data @ 42 #eq
n3 parentnode n1 #eq
n3 nextsibling 0 #eq
n3 prevsibling n2 #eq
n3 firstchild 0 #eq
n1 firstchild n2 #eq
n1 nodeclosed? #
n1 lastchild n3 #eq

0 4 createnode value n4
n4 n2 addnode
n4 nodeid 4 #eq
n4 parentnode n2 #eq
n4 nextsibling 0 #eq
n4 prevsibling 0 #eq
n2 firstchild n4 #eq

n1 nodedepth 2 #eq
n2 nodedepth 1 #eq
n3 nodedepth 0 #eq
n4 nodedepth 0 #eq

n1 childcount 2 #eq
n2 childcount 1 #eq
n3 childcount 0 #eq
n4 childcount 0 #eq

: traverse ( node -- )
  dup begin dup nodeid dup .x1 c, nextnode ?dup not until drop ;
create expected 1 c, 2 c, 4 c, 3 c,
create res n1 traverse
expected res 4 []= #

n1 n1 4 nextnodeid n4 #eq drop
n4 n4 4 nextnodeid 0 #eq drop

create expected 2 c, 4 c,
create res n2 traverse
expected res 2 []= #

n2 removenode
create expected 1 c, 3 c,
create res n1 traverse
expected res 2 []= #

#psempty
