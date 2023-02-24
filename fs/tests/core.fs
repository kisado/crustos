f<< tests/harness.fs

testbegin
\ Tests for fs/lib/core.fs

\ Words: Does
: incer doer , does> @ 1+ ;
41 incer foo
101 incer bar

foo 42 #eq
bar 102 #eq

\ Semantics: to
42 value foo
43 to foo
foo 43 #eq
5 to+ foo
foo 48 #eq
to' foo @ 48 #eq

testend
