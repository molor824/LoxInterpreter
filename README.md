# My take on the Lox interpreter
First lox interpreter made in C#

# Example:
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
    var x;
    var y;
    fn __init(x, y) {
        this.x = x;
        this.y = y;
    }
}
class Vector3 : Vector2 {
    var z;
    fn __init(x, y, z) {
        this.z = z;
        base.__init(x, y);
    }
}

var a = Vector2(3, 2);
print(a);
a = Vector3(6, 1, 2);
print(a, a.base);
```
```
Vector2 {
  x = 3;
  y = 2;
}
Vector3 {
  z = 2;
}
Vector2 {
  x = 6;
  y = 1;
}
```