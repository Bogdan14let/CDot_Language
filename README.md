# CDot Language

> VM-based programming language with manual memory management, DLL/FFI support and Native UI.

![Version](https://img.shields.io/badge/version-3.3.9-blue)
![Platform](https://img.shields.io/badge/platform-Windows-success)
![Runtime](https://img.shields.io/badge/runtime-VM-orange)

---

## Overview

CDot is a virtual machine programming language designed for:

* Script development
* Interactive applications
* Games
* Custom DSL creation

The language combines a lightweight runtime, manual memory management and optional extension modules for native Windows UI and DLL interoperability.

---

## Features

* Virtual Machine execution
* Manual memory management
* Registers and shared memory
* Functions & Call Stack
* Arrays and structures
* String utilities
* File system API
* Random utilities
* Native Windows UI
* DLL / FFI support
* Configurable runtime

---

## Project Structure

```text
project/
├── main.cdt
├── config.cfg
├── libs/
└── assets/
```

Create a new project:

```bash
cdot new
```

---

## Memory Model

Memory is represented as:

```text
object[] memory
```

Supported address types:

| Type | Description |
| ---- | ----------- |
| `f`  | Float       |
| `s`  | String      |
| `m`  | System      |

Memory allocation is handled through `made`, while `not` performs logical inversion on stored values.

---

## Language Features

### Arithmetic

* add
* sub
* mul
* div
* xor
* inc
* dec
* exc

### Flow Control

* Labels
* `jmp`
* `jf`
* `call`
* `ret`
* `cmd`
* `map`

### Arrays

* get
* set
* push
* pop
* len
* Dynamic indexing
* Nested structures

### Strings

* split
* trim
* lowercase
* uppercase

### File System

* Read files
* Write files
* Execute external files

### Input

* Blocking input
* Asynchronous input

---

## Native Extensions

### DLL / FFI

Available through:

```text
#include dllapi.ddl
```

Commands:

* dll
* @using
* ffi

### Native UI

Available through:

```text
#include winapi.ddl
```

Supports:

* Windows
* Buttons
* Labels
* TextBoxes
* CheckBoxes
* RadioButtons
* ComboBoxes
* ListBoxes
* ProgressBars
* Sliders
* Panels
* GroupBoxes
* PictureBoxes

---

## Configuration

`config.cfg` allows configuring:

* Entry file
* Window title
* Icon
* Maximum memory
* Console behavior
* CMD access
* Cursor visibility
* Additional libraries

---

## Example

```cdot
func [0f, 1f] sum
■add 0f 1f 2f
■mov 0m 2f
ret

call [3,4] sum
hlt
```

---

## Version

**CDot Language v3.3.9**

Core Language + DLL/FFI + Native UI.
