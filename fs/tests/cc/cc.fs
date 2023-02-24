
f<< tests/harness.fs
f<< cc/cc.fs

testbegin
\ C Compiler Tests

: _cc S" tests/cc/test.c" fopen >fd ['] f< to cc< cc1, ;
_cc

retconst 42 #eq
neg -42 #eq
bwnot $ffffffd5 #eq
exprbinops 7 #eq
boolops 0 #eq
variables 42 #eq
funcall 42 #eq
2 3 adder 5 #eq
42 plusone 43 #eq

testend
