\ Extra dictionaries
\ Create and use dictionaries besides the system one. Such dictionaries use the
\ same words as words for the system dictionary. It does so by fiddling with
\ "current".

\ Usage: You begin by allocating a new dict structure with "newxdict <name>".
\ Then, you just need to use "x" words like "xentry", "x'" and "xfind". If you
\ need dict-related words that aren't proxied by "x" words, you can use xdict[
\ and ]xdict directly, but be careful not to use this in interpret mode because
\ you'll lock yourself out of your system dict. Also, you can't nest xdict[ calls.

0 value currentbkp
: newxdict create 4 allot0 ;
: xdict[ ( 'dict -- ) current to currentbkp @ to current ;
: ]xdict ( 'dict -- ) current swap ! currentbkp to current ;
: xdictproxy ( w -- ) doer , does> ( 'dict 'w -- )
  over xdict[ swap >r @ execute r> ]xdict ;

' '       xdictproxy x'
' find    xdictproxy xfind
' entry   xdictproxy xentry
' create  xdictproxy xcreate
' value   xdictproxy xvalue
