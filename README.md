# CDot Language Documentation
**v4.4.18 — core language + DLL/FFI, Native UI, Lightweight UI, File System, and Strings extensions**

> This document is an adaptation of the v3.3.10 documentation to v4.4.18, updated against the actual
> `Parser.cs` implementation. Everything unchanged from v3.3.10 has been kept as-is; new or modified
> behavior is called out explicitly, and the Changelog (§22) lists exactly what moved between the two
> versions.

## 1. Introduction

CDot is a VM-based programming language with manual memory management and its own runtime.

The language is designed for:
- writing scripts
- building interactive programs and games
- developing custom DSLs (languages built on top of CDot)

As of v4.4.18, the core language can be extended with **five** optional modules, each enabled via
`#include`: `dllapi.ddl` (calling external libraries through FFI), `winapi.ddl` (full native windows and
UI controls), `windows.ddl` (a lightweight window/graphics module — new in this version, see §18.5),
`fsapi.ddl` (file system access), and `strapi.ddl` (string operations and command/expression execution).
Extension commands are only available once the corresponding library is included — see §14.

**What changed since v3.3.10** (high level; full details in §22):
- A new general-purpose register class, the **result registers** (`r`, e.g. `0r`, `1r`, …), was added to
  the memory model (§3.1) together with the `send` command that writes to them (§5.1).
- A new lightweight UI module, `windows.ddl`, was added (§18.5) — it exposes a subset of the Native UI
  surface (window, picture box, text get/set, mouse polling, basic layout directives) without pulling in
  full control widgets.
- A new command, `winmouse`, was added for polling raw mouse events; it requires `windows.ddl`
  specifically (§18.5).
- Two new commands, `argc` and `argv`, expose command-line arguments passed to the compiled program
  (§11.1).
- `chr` is now bidirectional: depending on the destination register type it converts code→character or
  character→code (§12).
- System port `5m` (§4) now has clearly defined, distinct read and write semantics: writing to it raises a
  custom error with an explicit line/code/message, reading from it returns the current OS username.

## 2. Execution Model (VM)

The program executes line by line.

Core components:
- memory (`object[]`)
- instruction pointer
- instruction set

Flow control:
- `jmp` — unconditional jump
- `jf` — conditional jump
- `call` / `ret` — functions
- `cmd` — command block declaration (bracket-free argument form, see §16)
- `map` — inline switch-case inside func/cmd (see §7)

*(Unchanged from v3.3.10.)*

## 3. Memory Model

All memory is:

```
object[] memory
```

Address = type + index. Types:

| Type | Meaning |
|---|---|
| `f` | float |
| `s` | string |
| `m` | system port |
| `r` | **result register (new in v4.4.18 — see §3.1)** |

Example:
```
0f  → number
1s  → string
```

Memory management: `made` and `not` operate directly on addresses:

| Syntax | Description |
|---|---|
| `made addr` | MALLOC / DEALLOC — allocates memory at addr, or frees it if the address is already allocated (toggle behavior). |
| `not addr` | Logical/bitwise NOT applied to the value at addr; the result is written back to the same location. |

### 3.1 Result Registers (`r`) — new in v4.4.18

Alongside the `f`/`s`/`m` addresses, v4.4.18 introduces a separate bank of **result registers**, addressed
with the `r` suffix (`0r`, `1r`, `2r`, …). Unlike `f` and `s` registers, a result register is untyped: it
can hold a number, a string, or any other value (for example a raw result coming back from `exc`).

- Result registers are **written only through the `send` command** (§5.1).
- They can be **read almost anywhere** a memory address is accepted as a source: `mov`, `exc`, `wrt`,
  `rdl`, `spt`, `trm`, `low`, `big`, `chr`, `ffi` string arguments, `winmouse`/`wingettext`/`winsettext`,
  and system ports accessed through `mov` (`0m`, `1m`, `2m`, `3m`, `4m`, `5m`, `6m`, `7m`, `8m`, `9m`) all
  accept an `r` register as their source value.
- There is no bracketed/indexed collision with `s`/`f` addresses — `0r` is a distinct address space from
  `0s` and `0f`.

## 4. System Ports

| Syntax | Description |
|---|---|
| `0m` | output |
| `1m` | text color |
| `2m` | background |
| `3m` | sleep |
| `4m` | window title |
| `5m` | error (see below) |
| `6m` | CMD |
| `7m` | cursor |
| `8m` | resize console (may not work) |
| `9m` | show/hide console |

**Port `5m` clarification (v4.4.18):** this port behaves differently depending on direction.
- **As a write target**, `mov 5m line code "message"` raises a custom runtime error: `line` is the line
  number to report (pass `0` to use the current line), `code` is the exit code, and `message` is the error
  text. All three arguments may be literals or `f`/`s`/`r` registers.
- **As a read source**, `mov 0s 5m` (or into any string-capable destination) returns the current OS
  username instead of an error value.

All other ports behave as in v3.3.10. Ports `1m`/`2m` (colors), `3m` (sleep), `4m` (title), `6m` (CMD),
`7m` (cursor x/y), and `8m` (resize width/height) now also accept their arguments as `f`/`s`/`r` registers
in addition to literals.

## 5. Assignment

| Syntax | Description |
|---|---|
| `mov dest source` | Assigns the value of source to dest. |

### 5.1 `send` — writing to result registers (new in v4.4.18)

| Syntax | Description |
|---|---|
| `send dest(r) source` | Copies the value of `source` (a literal, or an `f`/`s`/`r` address) into the result register `dest`. `dest` must be an `r` address. |

Example:
```
send 0r "hello from a result register"
mov 0m 0r / endl
```

## 6. Arithmetic & Bitwise

Basic arithmetic:

| Syntax | Description |
|---|---|
| `add op1 op2 dest` | dest = op1 + op2 |
| `sub op1 op2 dest` | dest = op1 - op2 |
| `mul op1 op2 dest` | dest = op1 * op2 |
| `div op1 op2 dest` | dest = op1 / op2 |
| `xor op1 op2 dest` | Bitwise XOR: dest = op1 xor op2 |

Increment / decrement:

| Syntax | Description |
|---|---|
| `inc reg target` | Increases the value at target by reg (or by 1 when used as a counter). |
| `dec reg target` | Decreases the value at target the same way as inc. |

Note: this section does not document `exc` (execute command / evaluate math expression). `exc` is part of
the Strings & Execution extension — see §9 — and requires `#include strapi.ddl`.

*(Unchanged from v3.3.10.)*

## 7. Flow Control

Labels:
```
label:
jmp label
jf a > b label
```

`map` — inline switch-case branching on a value inside a func/cmd. The `pos` key is the default branch. The
construct itself does not declare anything (it is not added to the function/command/label index):
```
map source
■case1: command
■pos: default_command
ret
```

*(Unchanged from v3.3.10.)*

## 8. Arrays

```
arr [1,2,3] nums
```

Commands:

| Syntax | Description |
|---|---|
| `get array index dest_reg` | Reads the element of array at index into dest_reg. |
| `set array index source` | Writes source into array at index. |
| `len array dest_reg` | Writes the length of array into dest_reg. |
| `push array source` | Appends source to the end of array. |
| `pop array` | Removes the last element of array. |

### 8.1 Stack (LIFO)

`push` / `pop` implement a stack.

### 8.2 Structures

```
arr [{a={b=10}}] data
```

Access:
```
get data 0.a.b 0f
```

### 8.3 Dynamic Indexing

The index can be a variable:
```
get nums 0f 1f
```

*(Unchanged from v3.3.10.)*

## 9. Strings & Execution (`strapi.ddl`)

Available only when `#include strapi.ddl` is present.

Supported escape sequences: `\n` `\t` `"`

String operations:

| Syntax | Description |
|---|---|
| `spt source separator target_array` | SPT — splits source by separator, storing the result in target_array. |
| `trm source dest_reg` | TRM — trims whitespace from both ends of source, result in dest_reg. |
| `low source dest_reg` | LOW — converts source to lowercase. |
| `big source dest_reg` | BIG — converts source to uppercase. |

Command / expression execution:

| Syntax | Description |
|---|---|
| `exc command/math res` | EXC — executes a CDot command, or evaluates a math expression passed as a string; when used for math, the result is stored in res. |

`source`/`dest_reg`/`res` in all of the above now also accept `r` (result) registers, in addition to `s`
literals and `s` addresses, as documented in §3.1.

## 10. File System (`fsapi.ddl`)

Available only when `#include fsapi.ddl` is present.

| Syntax | Description |
|---|---|
| `wrt file source` | Writes source to file. |
| `rdl file dest` | Reads the contents of file into dest. |
| `exs file result` | EXS — executes file (an external script/process); the result of execution is stored in result. |

`file` and `source`/`dest` now also accept `r` registers.

## 11. Input System

| Syntax | Description |
|---|---|
| `key dest` | KEY — reads user input into dest. |
| `dlk` | DLK — asynchronous input (does not block execution). |

### 11.1 Command-Line Arguments — new in v4.4.18

| Syntax | Description |
|---|---|
| `argc dest_reg` | Writes the number of command-line arguments passed to the compiled program into the float register dest_reg. |
| `argv array` | Appends each command-line argument to `array` (an existing array declared with `arr`), one element per argument. Numeric-looking arguments are stored as numbers, everything else as strings. |

Example:
```
arr [] args
argc 0f
argv args
```

## 12. Random / Utility

| Syntax | Description |
|---|---|
| `rnd min max dest_reg` | Random number in the range [min, max]. |
| `chr src dest_reg` | CHR — bidirectional (see below). |

**`chr` is now bidirectional (changed in v4.4.18).** The direction is determined by the type of
`dest_reg`:
- If `dest_reg` is a **string** (`s`) address: `src` is treated as an ASCII code (`0`–`127`, literal or
  numeric/`r` register) and the resulting character is written to `dest_reg`. This matches the v3.3.10
  behavior.
- If `dest_reg` is a **float** (`f`) address: `src` is treated as a single character (a string literal, or
  an `s`/`r` address holding one) and its ASCII code is written to `dest_reg`.

Codes/characters outside the 0–127 ASCII range raise an error.

## 13. Screen

| Syntax | Description |
|---|---|
| `cls` | Clears the screen. |
| `endl` | Line break. |

*(Unchanged from v3.3.10.)*

## 14. Project Setup & Configuration

### 14.1 Creating a New Project

Running:
```
cdot new
```
scaffolds a new project in the current directory: it generates a `config.cfg` file with default settings
and a `main.cdt` entry-point source file.

Tip: run `cdot help` at any time to see the full list of available CLI commands and options directly in
your terminal.

### 14.2 config.cfg Reference

`config.cfg` is an INI-style properties file (`Key = value`, `;` starts a comment) grouped into three
sections.

**Main settings**

| Syntax | Description |
|---|---|
| `MainStartFile = "main.cdt"` | The entry-point source file that gets executed when the project runs. |
| `AditionalContent = []` | A list of extra `.ddl` library filenames made available to the editor for autocomplete/indexing purposes. This does not by itself enable a library's commands in a `.cdot`/`.cdt` file — that still requires `#include` (see §14.3). |
| `Pause = false` | Whether the console/window stays open and waits for a key press after the program finishes, instead of closing immediately. |
| `Clear = false` | Whether the console is cleared right before the program starts running. |

**Window settings**

| Syntax | Description |
|---|---|
| `Title = ""` | The title shown in the console/window title bar. |
| `Icon = ""` | Path to an `.ico` file used as the application/window icon. |

**Language settings**

| Syntax | Description |
|---|---|
| `AllowCMD = true` | Whether the program is permitted to run shell commands through the CMD system port (`6m`). |
| `MaxMemory = 100` | The maximum number of cells available in the memory array (`object[] memory`) — an upper bound on addresses the program can allocate. Note: this bound applies to `f`/`s` addresses; result registers (`r`) use a separate, fixed-size bank (see §3.1) and are not affected by `MaxMemory`. |
| `Cursor = true` | Whether the console cursor is shown. |
| `Ending = true` | Whether an end-of-program indicator is shown once execution finishes (e.g. an exit/completion message). |

### 14.3 In-File Compiler Directives — new in v4.4.18

In addition to `config.cfg`, the following settings can now be set directly at the top of a source file with
a `#` directive, taking effect for that file's compilation (mirroring, and overriding, the corresponding
`config.cfg` keys above):

| Syntax | Description |
|---|---|
| `#maxmemory N` | Equivalent to `MaxMemory` in config.cfg. |
| `#pause true|false` | Equivalent to `Pause`. |
| `#clear true|false` | Equivalent to `Clear`. |
| `#cursor true|false` | Equivalent to `Cursor`; also applied immediately to the console. |
| `#allowcmd true|false` | Equivalent to `AllowCMD`. |
| `#ending true|false` | Equivalent to `Ending`. |
| `#include libname.ddl` | See §14.4. |

Directives are recognized whether or not there is a space after `#` (i.e. `#include x` and `#include x` are
equivalent).

Ending's exact behavior should be confirmed against the runtime you're targeting — it is documented here
based on its config key name.

### 14.4 #include

`#include` — imports an external library (`.ddl`) and enables its associated command set:

| Syntax | Description |
|---|---|
| `#include dllapi.ddl` | Enables the §17 commands: `dll`, `@using`, `ffi`. |
| `#include winapi.ddl` | Enables the full native UI command set in §18 (`winopen`, all control commands, `winwait`, …) and the directives in §19 (`@place…`). |
| `#include windows.ddl` | **New in v4.4.18.** Enables the lightweight UI subset described in §18.5 — window, picture box, text get/set, `winwait`, `winmouse`, and a subset of layout/style directives — without the control-widget commands that `winapi.ddl` provides. |
| `#include fsapi.ddl` | Enables the §10 File System commands: `wrt`, `rdl`, `exs`. |
| `#include strapi.ddl` | Enables the §9 Strings & Execution commands: `spt`, `trm`, `low`, `big`, `exc`. |

`winapi.ddl` and `windows.ddl` are independent modules — including one does not automatically enable the
other's exclusive commands (see the table in §18.5 for exactly which commands each one unlocks).

Libraries can also be referenced for editor indexing/autocomplete purposes via `AditionalContent` in
config.cfg (§14.2) — this is separate from `#include` and does not enable extension commands in the source
file itself.

Import order matters for `std*.ddl` libraries: a `std*.ddl` library (e.g. `stdio.ddl`) must be `#include`-d
before any code in the file that calls its functions in dot notation (§15.2). Place these `#include` lines
at the very top of the file, above everything else — including one further down, or after code that
already calls `libname.funcname`, will not work.

## 15. Syntax Notes

### 15.1 Statement Separator ( / )

A forward slash `/` lets you place several statements on a single line. It is purely a formatting shortcut
— each part is executed as if it were written on its own line, in order, left to right:
```
@bg "form" "#000000" / @icon "form" "icon.ico"
mov 0m 2s / endl
```
This is commonly used to pair a value-producing statement with a follow-up action on the same line, e.g.
printing a value and then immediately emitting a line break with `endl`.

### 15.2 Library Function Calls (dot notation)

Once a library is loaded via `#include`, some of its functions are called as `libname.funcname` instead of
being exposed as bare global commands. Arguments follow the same positional rules as built-in commands:
```
stdio.pop dest
stdio.replace source "÷" "/"
```
Which functions a given library exposes as bare commands (like `winopen`) versus dotted calls (like
`stdio.pop`) depends on that library — check the library's own documentation for its full function list.

Reminder: as noted in §14.4, `std*.ddl` libraries must be `#include`-d at the top of the file before their
dot-notation functions are called anywhere below.

*(Unchanged from v3.3.10.)*

## 16. Functions & Call Stack

Function declaration (classic form, with bracketed argument list):
```
func [0f, 1s] myFunc
■add 0f 1f 2f
■mov 0m 2f
ret
```

Call:
```
call [5,10] myFunc
```

`cmd` — an alternative command-block declaration, without brackets around the arguments:
```
cmd myCmd arg1 arg2
■...
ret
```

### 16.1 Argument Passing

Arguments:
- passed by value
- can be literals or addresses

### 16.2 Call Stack

Each call:
- → pushes the current position

`ret`:
- → returns

### 16.3 Nested Calls

Functions can call other functions.

### 16.4 Errors

Errors are reported through system port `5m` (see §4 for its updated read/write semantics).

*(Unchanged from v3.3.10, aside from the §4 clarification.)*

## 17. DLL / FFI Extension

Available only when `#include dllapi.ddl` is present.

| Syntax | Description |
|---|---|
| `dll path` | Registers an external library at path, making it available for subsequent calls. |
| `@using path` | Sets path as the active DLL — subsequent `ffi` calls target it by default. |
| `ffi class method result args` | Calls method of class from the active (or explicitly given) library, passing args; the result is stored in result. |

Note: the `result` argument to `ffi` must be a plain string (`s`) address — unlike most other commands in
v4.4.18, it does **not** accept an `r` result register as its destination. `class`/`method` name arguments,
however, may be literals, `s` addresses, or `r` registers.

Example:
```
#include dllapi.ddl
dll "mathlib.dll"
@using "mathlib.dll"
ffi MathClass Square 0f 5
mov 0m 0f
hlt
```

## 18. Native UI Extension — Window & Controls

Available only when `#include winapi.ddl` is present, unless noted otherwise (see §18.5 for the
`windows.ddl` subset).

### 18.1 Window

| Syntax | Description | Also available via `windows.ddl`? |
|---|---|---|
| `winopen title width height` | Opens a native window with the given title and width×height dimensions. | Yes |
| `winclose` | Closes the current window. | Yes |
| `winwait result timeoutMs` | Blocks execution until a UI event occurs (e.g. a button click, key press, or key release) or timeoutMs elapses. The event is written to result — see below. | Yes |
| `winmouse result timeoutMs` | **New in v4.4.18, `windows.ddl`-only** (see §18.5). Blocks until a raw mouse event occurs or timeoutMs elapses, writing it to result. | Exclusive to `windows.ddl` |

`winwait` return values: result normally holds the name of the triggering control (e.g. a button's name).
For keyboard events it holds one of the following special formats instead:

| Syntax | Description |
|---|---|
| `__keydown__:Key` | A key was pressed down; Key identifies which key. |
| `__keyup__:Key` | A key was released; Key identifies which key. |
| `__keychar__:char` | A character was typed; char is the resulting character. |
| `__closed__` | The window was closed (e.g. via the close button). |

`winmouse` return values (new in v4.4.18):

| Syntax | Description |
|---|---|
| `__mousedown__:button` | A mouse button was pressed; button is one of `left`, `right`, `midle`, `side1`, `side2`. |
| `__mouseup__:button` | A mouse button was released. |
| `__mousemove__:X, Y` | The mouse moved; X and Y are the pointer coordinates within the window. |
| `__timeout__` | timeoutMs elapsed with no mouse event. |

### 18.2 Controls

*(`winapi.ddl` only — none of the commands in this subsection are available through `windows.ddl`.)*

| Syntax | Description |
|---|---|
| `winlbl "name" "text"` | Adds a text label. |
| `winbtn "name" "text"` | Adds a button; on click its name is pushed to winwait. |
| `wintxt "name" "text"` | Adds a single-line text input. |
| `wincheck "name" "text"` | Adds a checkbox. |
| `winradio "name" "text"` | Adds a radio button. |
| `wincombo "name" "item1;item2;item3"` | Adds a dropdown list. |
| `winlist "name" "item1;item2;item3"` | Adds a scrollable list box. |
| `winprogress "name"` | Adds a progress bar. |
| `winslider "name"` | Adds a slider (trackbar). |

### 18.3 Containers

| Syntax | Description | Also available via `windows.ddl`? |
|---|---|---|
| `wingroup "name" "title"` | A titled group box container. | No |
| `winpanel "name"` | A plain panel container. | No |
| `winpbox "name"` | A picture box container for displaying an image. | Yes |

### 18.4 Getters / Setters

| Syntax | Description | Also available via `windows.ddl`? |
|---|---|---|
| `wingettext "name" result` | Reads the text of control name into result. | Yes |
| `winsettext "name" value` | Sets the text of control name. | Yes |
| `wingetchecked "name" result` | Reads the checked state of checkbox/radio name. | No |
| `wingetvalue "name" result` | Reads the value of progress bar/slider name. | No |

### 18.5 Lightweight UI — `windows.ddl` (new in v4.4.18)

`windows.ddl` is a smaller sibling of `winapi.ddl`: it opens a native window and gives you raw drawing/input
primitives (a picture box, text get/set, keyboard events via `winwait`, and the new `winmouse` for raw
mouse polling) **without** pulling in the form-builder control widgets (`winlbl`, `winbtn`, `wintxt`,
`wincheck`, `winradio`, `wincombo`, `winlist`, `winprogress`, `winslider`, `wingroup`, `winpanel`) or a
handful of styling directives. It's intended for programs that draw their own UI (e.g. onto a `winpbox`)
rather than building it out of standard controls.

Commands and directives available with `windows.ddl` (in addition to `winopen`/`winclose`/`winwait`/
`winmouse`/`winpbox`/`wingettext`/`winsettext` listed above):

| Directive | Available via `windows.ddl`? |
|---|---|
| `@place` | Yes |
| `@scale` | Yes |
| `@resizable` | Yes |
| `@getscale` | Yes |
| `@bg` | Yes |
| `@color` | Yes |
| `@font` | Yes |
| `@size` | Yes |
| `@visible` | Yes |
| `@align` | Yes |
| `@parent` | Yes |
| `@grid` | No — `winapi.ddl` only |
| `@icon` | No — `winapi.ddl` only |
| `@img` | No — `winapi.ddl` only |
| `@enabled` | No — `winapi.ddl` only |
| `@border` | No — `winapi.ddl` only |
| `@checked` | No — `winapi.ddl` only |
| `@value` | No — `winapi.ddl` only |
| `@range` | No — `winapi.ddl` only |
| `@additem` | No — `winapi.ddl` only |
| `@clearitems` | No — `winapi.ddl` only |

`winmouse` (§18.1) is notable in the other direction: it requires `windows.ddl` specifically and is **not**
enabled by `winapi.ddl` alone. If you need both the full control set and raw mouse polling, `#include` both
modules.

## 19. Native UI Extension — Style & Layout Directives

Directives address a control or window by name and configure its position, size, and appearance. See §18.5
for which of these are also available through `windows.ddl` alone.

| Syntax | Description |
|---|---|
| `@place "name" x y` | Absolute positioning. |
| `@grid "name" col row` | Positioning on a virtual grid. |
| `@scale "name" width height` | Sets width/height. |
| `@resizable addr/number` | Enables/disables window resizing. |
| `@getscale "name" widthResult heightResult` | Reads the current width/height into widthResult/heightResult. |
| `@bg "name" "#hex"` | Background color. |
| `@color "name" "#hex"` | Font color. |
| `@font "name" "fontFamily"` | Font family. |
| `@size "name" fontSize` | Font size. |
| `@visible "name" true` | Shows/hides a control. |
| `@enabled "name" true` | Enables/disables a control. |
| `@align "name" left` | Alignment: left / center / right. |
| `@border "name" none` | Border: none / single / fixed3d. |
| `@checked "name" true` | Checkbox/radio state. |
| `@icon "name" "path.ico"` | Window or control icon. |
| `@img "name" "path.img"` | Image for a control (e.g. winpbox). |
| `@value "name" number` | Progress bar/slider value. |
| `@range "name" min max` | Progress bar/slider value range. |
| `@additem "name" "item"` | Adds an item to a combo/list box. |
| `@clearitems "name"` | Clears a combo/list box. |
| `@parent "child" "container"` | Reparents child into container (group/panel). |

*(Command set unchanged from v3.3.10 — availability per module is new, see §18.5.)*

## 20. Example

Basic example (core language only):
```
func [0f, 1f] sum
■add 0f 1f 2f
■mov 0m 2f
ret

call [3,4] sum
hlt
```

Native UI example (`winapi.ddl`):
```
#include winapi.ddl
winopen "Demo" 300 150
winbtn "btnOk" "OK"
@place "btnOk" 100 60
loop:
winwait 0s 0
jf 0s == "btnOk" onClick
jmp loop
onClick:
winclose
hlt
```

File System & Strings example (`fsapi.ddl`, `strapi.ddl`):
```
#include fsapi.ddl
#include strapi.ddl
rdl "input.txt" 0s
big 0s 1s
wrt "output.txt" 1s
hlt
```

Lightweight UI + mouse polling example (`windows.ddl`, new in v4.4.18):
```
#include windows.ddl
winopen "Canvas" 400 300
winpbox "canvas"
@scale "canvas" 400 300
loop:
winmouse 0s 0
jf 0s == "__closed__" done
mov 0m 0s / endl
jmp loop
done:
winclose
hlt
```

Result registers + command-line arguments example (new in v4.4.18):
```
#include strapi.ddl
arr [] args
argc 0f
argv args
send 0r "hello"
mov 0m 0r / endl
hlt
```

## 21. Notes

- memory is shared
- functions use registers
- the call stack drives execution
- DLL/FFI, Native UI, Lightweight UI, File System, and Strings & Execution commands are only enabled by an
  explicit `#include` of the corresponding `.ddl` library in the source file itself
- result registers (`r`) are a separate address space from `f`/`s`/`m` and are not bound by `MaxMemory`
  (§3.1, §14.2)

## 22. Changelog

**v4.4.18**
- New address type: **result registers** (`r`), a separate, untyped register bank readable from nearly
  every command and writable via the new `send` command (§3.1, §5.1).
- New command `send dest(r) source` (§5.1).
- New extension module `windows.ddl` (§18.5) — a lightweight window/graphics module offering `winopen`,
  `winclose`, `winwait`, `winpbox`, `wingettext`, `winsettext`, and a subset of `@`-directives, without the
  standard control widgets from `winapi.ddl`.
- New command `winmouse` (§18.1) for polling raw mouse-down/mouse-up/mouse-move events; requires
  `windows.ddl` specifically, independent of `winapi.ddl`.
- New commands `argc` and `argv` (§11.1) for reading command-line arguments passed to the compiled
  program.
- `chr` (§12) is now bidirectional: code→character when the destination is a string register, and
  character→code when the destination is a float register (previously code→character only).
- System port `5m` (§4) semantics clarified/changed: writing now explicitly takes `line`, `code`, and
  `message` arguments to raise a custom error; reading returns the current OS username.
- New in-file compiler directives `#maxmemory`, `#pause`, `#clear`, `#cursor`, `#allowcmd`, and `#ending`
  (§14.3), mirroring the corresponding `config.cfg` keys on a per-file basis.
- System ports `1m`–`8m` now accept `f`/`s`/`r` registers as arguments, not just literals, when used through
  `mov`.
- No changes to the core execution model, arithmetic/bitwise commands, flow control (`jmp`/`jf`/`map`),
  arrays/structures, the DLL/FFI command surface itself (aside from `r`-register support for string
  arguments), or the `winapi.ddl` control/getter/setter command set.

**v3.3.10**
- New extension module `fsapi.ddl` (§10, File System). `wrt`, `rdl`, and `exs` are no longer part of the
  always-available core — they now require `#include fsapi.ddl`.
- New extension module `strapi.ddl` (§9, Strings & Execution). `spt`, `trm`, `low`, `big`, and `exc` are no
  longer part of the always-available core — they now require `#include strapi.ddl`.
- §14.3 `#include` reference updated with entries for `fsapi.ddl` and `strapi.ddl`.
- Fixed subsection numbering under the Native UI Window & Controls section (previously mislabeled
  17.1–17.4, now correctly 18.1–18.4).
- No changes to config.cfg, AditionalContent indexing behavior, or the DLL/FFI and Native UI modules.
- §14.1 now notes that `cdot help` lists all available CLI commands.

**v3.3.9**
- Baseline for this document (initial dllapi.ddl / winapi.ddl release notes not available).

END
