\ Tests for fs/lib/core.fs

: incer doer , does> @ 1+ ;
41 incer foo
101 incer bar

foo 42 #eq
bar 102 #eq

#psempty
