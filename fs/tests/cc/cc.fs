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
variables 82 #eq
funcall 42 #eq
2 3 adder 5 #eq
42 plusone 43 #eq
ptrget 42 #eq
ptrset 54 #eq
12 condif 13 #eq
42 condif 142 #eq
42 incdec 43 #eq
54 incdecp 55 #eq

testend
