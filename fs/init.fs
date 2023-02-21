\ Initialization Layer
\ Called at the end of boot.fs

f<< lib/core.fs
f<< sys/rdln.fs

: init S" crustOS" stype rdln$ ;
init
