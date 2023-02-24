f<< tests/harness.fs
f<< cc/cc.fs

testbegin
\ Tests for the C Compiler AST

: _parse S" tests/cc/test.c" fopen >fd ['] f< to cc< parseast ;
_parse

curunit firstchild dup astid AST_FUNCTION #eq ( fnode )
: s S" retconst" ;
dup data1 s s= #
firstchild nextsibling dup astid AST_STATEMENTS #eq ( snode )
firstchild dup astid AST_RETURN #eq ( rnode )
firstchild dup astid AST_CONSTANT #eq ( cnode )
data1 42 #eq

testend
