#!/bin/sh

echo "Lines of code in CrustOS"
echo "All Forth code excluding tests:"
find . -name "*.fs" | grep -v tests | xargs cat | wc -l
echo "...excluding empty lines and comments:"
find . -name "*.fs" | grep -v tests | xargs cat | grep -v "^\\\\" | grep -v '^$' | wc -l
echo "C compiler:"
find fs/cc -type f | xargs cat | wc -l
echo "...excluding empty lines and comments:"
find fs/cc -type f | xargs cat | grep -v "^\\\\" | grep -v '^$' | wc -l
echo "Test code:"
find fs/tests -type f | xargs cat | wc -l
echo "Assember:"
cat crust.asm | wc -l