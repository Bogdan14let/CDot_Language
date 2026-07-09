# CDot Language

> VM-based programming language with manual memory management, DLL/FFI support and Native UI.

![Version](https://img.shields.io/badge/version-3.3.10-blue)
![Platform](https://img.shields.io/badge/platform-Windows-success)
![Runtime](https://img.shields.io/badge/runtime-VM-orange)

---

## Overview

CDot is a virtual machine programming language designed for:

* Script development
* Interactive applications
* Games
* Custom DSL creation

The language combines a lightweight runtime, manual memory management and optional extension modules for native Windows UI, DLL interoperability, file system access and string operations.

---

## Features

* Virtual Machine execution
* Manual memory management
* Registers and shared memory
* Functions & Call Stack
* Arrays and structures
* Random utilities
* Native Windows UI
* DLL / FFI support
* File system API
* String utilities
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

> **Tip:** run `cdot help` at any time to see the full list of available CLI commands and options.

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

### Input

* Blocking input
* Asynchronous input

---

## Native Extensions

> Since v3.3.10, all extension commands — including string operations and file system access — are only available once the corresponding `.ddl` library is `#include`-d in the source file. Nothing in this section is part of the always-available core.

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

### File System

Available through:

```text
#include fsapi.ddl
```

Commands:

* wrt — write to file
* rdl — read from file
* exs — execute external file

### Strings

Available through:

```text
#include strapi.ddl
```

Commands:

* spt — split
* trm — trim
* low — lowercase
* big — uppercase
* exc — execute command / evaluate math expression

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

**CDot Language v3.3.10**

Core Language + DLL/FFI + Native UI + File System + Strings.

### Changelog

**v3.3.10**
* `wrt`, `rdl`, `exs` moved out of the core into the new `fsapi.ddl` extension.
* `spt`, `trm`, `low`, `big`, `exc` moved out of the core into the new `strapi.ddl` extension.
* All four extensions (`dllapi.ddl`, `winapi.ddl`, `fsapi.ddl`, `strapi.ddl`) now follow the same `#include`-gated model.