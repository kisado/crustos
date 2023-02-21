\ C Compiler Stage 1
\ Requires cc/gen.fs, cc/ast.fs, asm.fs and wordtbl.fs

\ compiles input coming from the `cc<` alias (defautls to `in<`) and writes the
\ result to here
\
\ aborts on error

: cc1, ( -- )
  parseast curunit _debug if dup printast nl> then
  dup mapunit _debug if curmap printmap then
  gennode ;
