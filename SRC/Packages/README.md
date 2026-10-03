# Packages

A program that is not compiled into the kernel is a package: a `.pkg` file that Aura reads at run
time. The packages in this folder are built into Aura; the same files can be downloaded with `pkg /add`.

Each folder here is one package. The build zips `<Name>/` into `<Name>.pkg` and embeds it in the kernel
(`AuraBuildPackages` in `Aura_OS.csproj`). Aura lists the built-in packages, then those in
`/0/System/Programs/`. An installed package replaces a built-in one of the same name.

## The .pkg file

A zip archive with `package.xml` at its root:

```
SystemInfo/
├── package.xml     the manifest
├── main.lua        the code, plus any other .lua files
└── SystemInfo.xml  an app's window (a layout file), and its images
```

To build one by hand, zip the folder's contents: `cd SystemInfo && zip -r ../SystemInfo.pkg .`

```xml
<Package name="SystemInfo" version="1.0.0" author="Aura Team"
         description="The Aura version, and a check for a newer release."
         main="main.lua" layout="SystemInfo.xml" />
```

| Attribute | |
|---|---|
| `name` | Program name: `run SystemInfo`, and the installed file `Programs/SystemInfo.pkg`. Letters, digits, `-` and `_`. |
| `displayName` | Start menu name, the `name` by default. |
| `version`, `author`, `description` | Shown by the package manager. |
| `main` | Lua file run when the program starts, `main.lua` by default. |
| `layout` | An app's layout file ([Layout README](../Aura_OS/System/Graphics/UI/GUI/Layout/README.md)). Without it the package is a console program. |

**An app** (`layout` given) is in the start menu, and `run` opens its window. Its main file runs once
when the window opens: it fills the controls and gives the code of the layout's events, which then run
on the UI thread. Keep them short; the desktop waits for them. An `Image`'s `src` is a file of the
package, or else one of the kernel's images (`UI/Images/AuraLogo.bmp`).

**A console program** runs in the Terminal that started it, until its main file returns. `arg[1]`... and
`...` are the command line arguments.

The `.lua` files are modules: `require "lib.util"` loads `lib/util.lua`, and `dofile`/`loadfile` read
the package's files first.

## The aura library

The kernel API for packages, a global table (also `require "aura"`). The Lua is 5.5
(Cosmos.Executable.Lua 4.0.1), with its standard libraries and `cosmos.crypto`.

### aura

| | |
|---|---|
| `aura.log(text)` | Writes a line to the OS log (`logs` command), after the package name. |
| `aura.app` | The app's window, `nil` in a console program. |

The modules below (`aura.system`, `aura.display`...) are objects: reading or setting a field they do not
have, or setting a read-only one, is an error. Their functions are called with a dot.

### aura.app

| | |
|---|---|
| `app:find(id)` | The element with that `id` in the layout file: a control, or a container (`Stack`, `Panel`, `Grid`, `Row`) that has only `id` and `visible`. An unknown id is an error. |
| `app:on(event, handler)` | Calls `handler()` on the layout's event of that name (`onClick="event"`, `onChange`, `onEnter`). A later call replaces it. |
| `app:every(milliseconds, handler)` | Calls `handler()` every that many milliseconds (the first time one interval from now), until the app closes. A handler that raises an error stops its timer. |
| `app.title` | The window title, also the taskbar name. |

An error in a handler is written to the OS log with its traceback, and the app goes on. `os.exit()` in
a handler closes the app; in the main file, it stops the window from opening.

### Controls

The properties of the control `app:find` returns. Reading or setting one the control does not have is an
error. A change shows on the next frame.

| Property | Controls | |
|---|---|---|
| `id` | all | Read only. |
| `visible` | all, containers too | A hidden element takes no space: the next ones move up. |
| `text` | Label, Button, TextBox, Checkbox | |
| `color` | Label, Button, Checkbox | Text color: `"#RRGGBB"`, `"#AARRGGBB"` or a name (`red`, `green`, `blue`, `black`, `white`, `gray`, `darkgray`, `lightgray`, `transparent`). Reads as `"#RRGGBB"`. |
| `checked` | Checkbox | |
| `value` | Slider | 0 to 255. |
| `items` | DropDown | A list of strings. Setting it clears the selection. |
| `selectedIndex` | DropDown | From 0, as in the layout file; -1 for none. |
| `selectedItem` | DropDown | Read only, `nil` for none. |
| `title`, `message` | Dialog | |
| `state` | Dialog | Set only: `"information"` or `"error"`. |

### aura.system

| | |
|---|---|
| `aura.system.version`, `aura.system.revision` | The running Aura (`"0.8.0"`, `"03102026"`). |
| `aura.system.installed` | False in live mode (nothing installed, no `settings.ini`). |
| `aura.system.computerName` | Read and set; also the DNS host name. |
| `aura.system.latestRelease()` | Version, revision and URL of the last release (os.json). An error when it cannot be downloaded: call it with `pcall`. |
| `aura.system.compareVersions(a, b)` | -1, 0 or 1. |
| `aura.system.compareRevisions(a, b)` | -1, 0 or 1; 0 when either is not a number. |

### aura.user

| | |
|---|---|
| `aura.user.name` | The logged in user's name, read and set. |

### aura.network

| | |
|---|---|
| `aura.network.isConfigured()` | True when a network card has an address. |

### aura.memory

Each read gives the current value.

| | |
|---|---|
| `aura.memory.totalPages`, `aura.memory.freePages`, `aura.memory.pageSize` | The page allocator: its pool (the largest usable memory region only) and its page size in bytes. |
| `aura.memory.liveHeap` | Bytes of live objects. |
| `aura.memory.collections` | Garbage collections so far. |
| `aura.memory.objectsFreed` | Objects the collections freed so far. |
| `aura.memory.gcTimePercent` | Share of time spent collecting. |
| `aura.memory.lastFreed` | Objects the kernel's last periodic collection freed. |
| `aura.memory.lastCollection()` | The last collection's `{ heapSize = , committed = , fragmented = , pinnedObjects = }`. It allocates: read it again only when `collections` changed. |

### aura.display

| | |
|---|---|
| `aura.display.width`, `aura.display.height` | The display mode, before the scale divides it for the UI. |
| `aura.display.scale` | The UI scale in percent. |
| `aura.display.modes()` | The display's modes: a list of `{ width = , height = }`. Only VMware SVGA II lists more than the running one. |
| `aura.display.scales()` | The scales: `{ 100, 150, 200 }`. |
| `aura.display.setMode(width, height, scale)` | Switches now: `true`, or `false` and why (scale too large for the mode, not enough memory). |

### aura.desktop

| | |
|---|---|
| `aura.desktop.wallpaper` | Path of the wallpaper, `nil` for the default one. |
| `aura.desktop.setWallpaper(path)` | Shows that BMP. An error when the file cannot be loaded: call it with `pcall`. A missing file shows the default wallpaper. |
| `aura.desktop.windowsAlpha`, `aura.desktop.taskbarAlpha` | Opacity of the windows and the taskbar, 0 to 255, read and set. |
| `aura.desktop.guiDebug` | The window manager's debug drawing, read and set. |

### aura.theme

| | |
|---|---|
| `aura.theme.bmpPath`, `aura.theme.xmlPath` | The theme files, read and set. Loaded at boot: a change shows after a reboot. A kernel file is `"embedded:UI/Themes/..."`. |

### aura.settings

`settings.ini`, which only an installed Aura has.

| | |
|---|---|
| `aura.settings.get(key)` | The value, `nil` when unset or in live mode. |
| `aura.settings.save({ key = value, ... })` | Writes the pairs in one go. Numbers and booleans are written as text (`true`/`false`). An error in live mode. |

### aura.fs

The files themselves are read and written with Lua's `io` library.

| | |
|---|---|
| `aura.fs.resolve(path)` | The absolute path, from the current directory; gen2 paths (`0:\Users`) are converted. |
| `aura.fs.fileExists(path)` | True when that file exists. |

## Limits

- A handler that never returns (`while true do end`) freezes the desktop: nothing stops it yet (the
  interpreter's hook is not public, see `lua-host` in GEN3-GAPS.md).
- Packages are downloaded over plain HTTP, unsigned: there is no TLS in gen3 yet.
- An app added with `pkg /add` opens with `run`, and shows in the start menu after a reboot.
