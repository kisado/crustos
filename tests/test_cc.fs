\ C Compiler Tests

f<< cc/cc.fs

: opentestc S" test.c" fopen >fd ;
opentestc

' f< to cc<

cc1,
fd~
retconst 42 #eq
neg -42 #eq
bwnot $ffffffd5 #eq
exprbinops 7 #eq
boolops 0 #eq
variables 42 #eq

#psempty
