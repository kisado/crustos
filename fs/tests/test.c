int retconst() {
    return 42;
}

int neg() { return -42; }

int bwnot() {
    return ~42;
}

int exprbinops() {
    return 1 + 2 * 3;
}

int boolops() {
    return 66 < 54 && 2 == 2;
}

int variables() {
    int foo = 40;
    int bar = 2;

    return foo + bar;
}

int funcall() {
    return retconst();
}

int adder(int a, int b) {
    return a + b;
}