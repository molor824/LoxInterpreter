# My take on the Lox interpreter

* Example:
```go
fn fib(n) {
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

print((fn(n) n + "possible!!!";)("anonymous functions are "));
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
```go
class Vector2 {
    var x = 0;
    var y = 0;
    fn __init(x, y) {
        this.x = x;
        this.y = y;
    }
    fn clone() Vector2(this.x, this.y);
    fn add(other) <- Vector2(this.x + other.x, this.y + other.y);
}
var a = Vector2(1, 1);
var b = Vector2(2.3, -52E-2);
print(a.add(b));
```
```
Vector2 {
    x = 3.3;
    y = 0.48;
}
```