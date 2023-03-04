TARGETS = crust

all: $(TARGETS)

crust: crust.asm boot.fs
	@echo " ASM      crust.asm"
	@nasm -f elf32 crust.asm -o crust.o
	@echo " LD       crust"
	@ld -m elf_i386 crust.o -o $@

.PHONY: run
run: crust
	@stty -icanon -echo; ./crust; stty icanon echo

.PHONY: test
test: crust
	@echo "f<< tests/all.fs bye" | ./crust && echo "all tests passed"

.PHONY: clean
clean:
	@rm -f $(TARGETS) crust.o
	@echo "cleaned crustOS"