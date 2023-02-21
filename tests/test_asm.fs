f<< asm.fs

code foo
  eax 42 i32 mov,
  ebp 4 i32 sub,
  [ebp] eax mov,
  ret,

foo 42 #eq
#psempty
