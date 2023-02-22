f<< tests/harness.fs
f<< asm.fs

testbegin
\ Tests for asm.fs

code foo
  eax 42 i32 mov,
  ebp 4 i32 sub,
  [ebp] eax mov,
  ret,

foo 42 #eq

testend
