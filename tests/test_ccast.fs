\ Tests for the C Compiler AST

f<< cc/cc.fs

: opentestc S" test.c" fopen >fd ;
opentestc
' f< to cc<
parseast

curunit firstchild dup astid AST_FUNCTION #eq ( fnode )
: s S" retconst" ;
dup data1 s s= #
firstchild nextsibling dup astid AST_STATEMENTS #eq ( snode )
firstchild dup astid AST_RETURN #eq ( rnode )
firstchild ( expr ) firstchild ( factor )
firstchild dup astid AST_CONSTANT #eq ( cnode )
data1 42 #eq

#psempty
