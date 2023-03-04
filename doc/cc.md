# CrustOS C Compiler

The C Compiler is a central piece of CrustOS. It's written in Forth and is
loaded very early in the boot process so that it can compile drivers we're
going to use.

The Compiler needs to meet **two** primary design goals:

1. Be as elegant and expressive as possible in the context of Forth; that is,
   be an elegant fallback to Forth's shortcomings.
2. Minimize the work needed to port existing C applications.

It is *not* a design goal of this C compiler to be able to compile POSIX
applications without changes. It is expected that a significant porting effort
will be needed each time.

Because of the first goal, we have to diverge from ANSI C. The standard library
will likely be significantly different, the macro system too. Both will
hopefully fit better with Forth than their ANSI counterpart.

But because of the second goal, we want to stay reasonably close to ANSI. The
idea is that the porting effort should be mostly a mechanical effort and
it should be as little prone as possible to subtle logic changes caused by the
porting.

For this reason, the core of the language is the same.