Generates a [Hexagony](https://esolangs.org/wiki/Hexagony) program from another program written in a simple language.
Generated programs can be very large and slow.
Use `Runner` or `Test` class for generation.

### Language syntax

```c
/*
  Literals and variables
*/

// All variables are signed arbitrary-precision integers.
someVar = 12; // variables are implicitly declared by assigning a value to an unused identifier
π = 3; // unicode variable names are supported
x = 'a'; // x = 97;
y = get; // read a single byte from STDIN and set `y` to its value (hexagony command `,`)
z = read; // read an integer from STDIN and set `z` to its value (hexagony command `?`)
x = (x + y * z) / someVar - 44 % read; // you can write an arithmetic expression anywhere a number is expected
x += y * z;
x++; // increment/decrement is a statement, you can't use it inside an expression
x = 0b101010 + 0o52 + 42 + 0x2b; // x = 42 + 42 + 42 + 42;
x = (10^9 - 1)^3; /* You can use the exponentiation operator if both operands are constant expressions.
                     Any operator with constant operands is evaluated at compile time.*/

/*
  Printing
*/

write(x); // write the decimal representation of `x` to STDOUT (hexagony command `!`)
write("π = ", π, ".", someVar + 2); // write "π = 3.14"
writeln(); // write "\n"
writeln("Simple escape sequences are supported: \'\"\\\0\a\b\f\n\r\t\v");
put(x); // write `x` modulo 256 as byte to STDOUT (hexagony command `;`)
put(226, 133, 159+x); // write `x` in roman numerals (if 1 ≤ x ≤ 12)

/*
  Control flow
*/

if (x >= '0' && x <= '9')
  write("x is digit");
else if (x >= 'a' && x <= 'z' || x >= 'A' && x <= 'Z')
  write("x is letter");
else if (x < 32 && !(x == ' ' || x == '\t' || x == '\n'))
  write("x is binary");
else {
  write("x is something else: ");
  put(x);
}

pow = 1;
while (x > 0) { x--; pow *= 2; }

for (a = 0; a < 10; a++) // the first and third parts are assignments, the second part is condition
  writeln(a);

for (;;) { // but all three parts are optional
  c = get;
  if (c < 0)
    break;
  put(c);
}

// A variable declared inside a code block (body of a condition or a loop)
// does not exist outside the code block. So variable `c` does not exist here.
// You can declare another variable with the same name:
c = read;
if (c < 0)
  exit; // terminate program

/*
  Labeled loops
*/

for outer_loop (i=2; i<=100; i++) { // labeled `for`
  for (j=2; j<i; j++)
    if (i%j == 0)
      continue outer_loop; // start a new iteration of `outer_loop`
  writeln(i);
}
```
