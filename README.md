# My take on the Lox interpreter

* Example:
```go
var fib = fn(n) {
    var a = 0.0;
    var b = 1;
    var c;

    for (var i = 0; i < n; i++) {
        c = a + b;
        a = b;
        b = c;
    }

    <- a;
};

for (var i = 0; i < 10; i++) {
    print(i, fib(i));
}

print((fn(n) => n + "possible!!!";)("anonymous functions are "));
```
```
0 0 
1 1
2 1
3 2
4 3
5 5
6 8
7 13
8 21
9 34
anonymous functions are possible!!!
```