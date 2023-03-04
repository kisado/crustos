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
    bar = foo + bar;

    return foo + bar;
}

int funcall() {
    return retconst();
}

int adder(int a, int b) {
    return a + b;
}

int plusone(int x) {
    return adder(1, x);
}

int ptrget() {
    int a = 42;
    int *b = &a;

    return *b;
}

int ptrset() {
    int a = 42;
    int *b = &a;

    *b = 54;
    
    return a;
}

int condif(int x) {
    if (x == 42) {
        x = x + 100;
    }

    return x;
}