f<< tests/harness.fs
f<< lib/str.fs

testbegin
\ Tests for str.fs

create list
  5 c, ," hello"
  3 c, ," foo"
  3 c, ," bar"
  0 c,

: _ S" foo" list sfind 1 #eq ; _
: _ S" hello" list sfind 0 #eq ; _
: _ S" baz" list sfind -1 #eq ; _

testend
