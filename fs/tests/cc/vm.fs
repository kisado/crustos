f<< tests/harness.fs
f<< asm.fs
f<< cc/vm.fs
testbegin

\ C Compiler Virtual Machine Tests

\ binop[+](binop[*](const[2],const[3]),const[1])
vm$
code test1
  0 0 vmprelude,
  2 const>operand
  operand>result
  3 const>operand
  vmmul,
  1 const>operand
  vmadd,
  vmret,
test1 7 #eq

\ binop[+](binop[-](const[3], const[1]),binop[*](const[2],const[3]))
vm$
code test2
  0 0 vmprelude,
  3 const>operand
  operand>result
  1 const>operand
  vmsub,
  pushresult,
  2 const>operand
  operand>result
  3 const>operand
  vmmul,
  popresult,
  vmadd,
  vmret,
test2 8 #eq

\ sub 2 args
vm$
code test3
  8 0 vmprelude,
  4 sf+>operand
  operand>result
  0 sf+>operand
  vmsub,
  vmret,
54 12 test3 42 #eq

\ assign 2 local vars
vm$
code test4
  0 8 vmprelude,

  42 const>operand
  operand>result
  4 sf+>operand
  result>operand

  5 const>operand
  operand>result
  0 sf+>operand
  result>operand

  4 sf+>operand
  operand>result
  0 sf+>operand

  vmadd,
  vmret,
test4 47 #eq


\ variable reference and dereference
vm$
code test5
  0 8 vmprelude,

  42 const>operand
  operand>result
  4 sf+>operand
  result>operand

  4 sf+>operand
  operand>&operand
  operand>result
  0 sf+>operand
  result>operand

  0 sf+>operand
  operand>[operand]
  operand>result
  vmret,
test5 42 #eq

\ assign and dereference
vm$
code test6
  0 8 vmprelude,

  42 const>operand
  operand>result
  4 sf+>operand
  result>operand

  4 sf+>operand
  operand>&operand
  operand>result
  0 sf+>operand
  result>operand

  54 const>operand
  operand>result
  0 sf+>operand
  operand>[operand]
  result>operand

  4 sf+>operand
  operand>result
  vmret,
test6 54 #eq

testend
