f<< tests/harness.fs
f<< lib/str.fs

testbegin
\ Tests for str.fs

3 stringlist list "hello" "foo" "bar"

S" foo" list sfind 1 #eq
S" hello" list sfind 0 #eq
S" baz" list sfind -1 #eq

'c' 0-9? not #
'9' 0-9? #
'z' A-Za-z? #
'0' A-Za-z? not #
'z' alnum? #
'0' alnum? #



testend
