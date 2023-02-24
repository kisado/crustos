f<< tests/harness.fs
f<< lib/xdict.fs

testbegin
\ Tests for xdict

newxdict mydict
mydict xcreate foo 1 c, 2 c, 3 c,
42 mydict xvalue bar
mydict x' foo execute 1+ c@ 2 #eq
mydict x' bar execute 42 #eq
word noop mydict xfind not #

testend
