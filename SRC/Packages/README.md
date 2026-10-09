# Packages

A program that is not compiled into the kernel is a package: a `.pkg` file that Aura reads at run
time. The packages in this folder are built into Aura, except the download-only ones
(`AuraDownloadOnlyPackage` in `Aura_OS.csproj`: Snake), which only the online repository has. All of
them can be downloaded with `pkg /add`, or with the Package Manager app.

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
| `menu` | `false` keeps an app out of the start menu: one that needs a file to open, as the Editor. `true` by default. |

**An app** (`layout` given) is in the start menu, and `run` opens its window. Its main file runs once
when the window opens: it fills the controls and gives the code of the layout's events, which then run
on the UI thread. Keep them short; the desktop waits for them. `arg[1]`... and `...` are the arguments
of `run` (`run Editor notes.txt`), or the file the app was opened with. An `Image`'s `src` is a file of the
package, or else one of the kernel's images (`UI/Images/AuraLogo.bmp`).

**A console program** runs in the Terminal that started it, until its main file returns. `arg[1]`... and
`...` are the command line arguments.

The `.lua` files are modules: `require "lib.util"` loads `lib/util.lua`, and `dofile`/`loadfile` read
the package's files first. Modules cannot require one another in a circle: `require` then fails with
"C stack overflow". The Disk Manager splits its code in folders this way (`model/`, `view/`, `actions/`).

## The online repository

`pkg /update` reads the package list at `https://aura.valentin.bzh/repository.json`, and `pkg /add {name}`
downloads and installs one of its packages. The Package Manager app does both, and `pkg /remove`.

`pkg /repository {url}`, or the Package Manager, switches to another repository: an address whose
`repository.json` is the list, or the address of the list itself (ending with `.json`). An installed
Aura keeps it in `settings.ini` (`packageRepository`); `pkg /repository` alone shows it.

The list is an array of entries whose values are all strings:

```json
[
  {
    "name": "Snake",
    "display-name": "Snake",
    "description": "The snake game: ...",
    "author": "Aura Team",
    "version": "1.0.0",
    "link": "https://aura.valentin.bzh/packages/Snake.pkg"
  }
]
```

`publish.sh` builds both from this folder, each package zipped as the build does and the list from their
`package.xml`, then copies them to the server's document root (a directory or an rsync target), leaving
anything else there (`os.json`, the latest release for the update check) as it is:

```sh
RSYNC_RSH="ssh -i ~/.ssh/<key>" SRC/Packages/publish.sh <user>@<server>:<document root>
```

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
| `app:on(event, handler)` | Calls `handler()` on the layout's event of that name (`onClick="event"`, `onChange`, `onEnter`, `onActivate`, `onPaste`, `onMove`, `onRelease`). A later call replaces it. |
| `app:every(milliseconds, handler)` | Calls `handler()` every that many milliseconds (the first time one interval from now), until the app closes. A handler that raises an error stops its timer. |
| `app:after(milliseconds, handler)` | Calls `handler()` once, that many milliseconds from now (0 is fine) and after the window was drawn: what the caller changed shows first. For slow work, a download: say what is going on, then do it here. |
| `app:onKey(handler)` | Calls `handler(key)` with each key typed while the app is focused, but those a control with the keys uses (a TextBox all of them, a ListBox its moves and Enter): `key.name` for a key that types no character (`enter`, `backspace`, `tab`, `escape`, `up`, `down`, `left`, `right`, `home`, `end`, `pageUp`, `pageDown`, `insert`, `delete`, `f1` to `f12`), `key.char` for one that does (UTF-8; with Ctrl held, the key's letter: `"c"` for Ctrl+C), and `key.ctrl`, `key.shift`, `key.alt`. A later call replaces it. |
| `app:onResize(handler)` | Calls `handler()` once the window was resized and its elements placed again. A later call replaces it. |
| `app:fit()` | Resizes the window to its elements, at least as wide as its title, and no larger than the room the screen has right of and under it (and 999 pixels). |
| `app.title` | The window title, also the taskbar name. |
| `app.focused` | True while the window is the focused one (it gets the keys). Read only. |

An error in a handler is written to the OS log with its traceback, and the app goes on. `os.exit()` in
a handler closes the app; in the main file, it stops the window from opening.

### Controls

The properties of the control `app:find` returns. Reading or setting one the control does not have is an
error. A change shows on the next frame.

| Property | Controls | |
|---|---|---|
| `id` | all | Read only. |
| `visible` | all, containers too | A hidden element takes no space: the next ones move up. |
| `width`, `height` | all controls | Read only, in pixels. |
| `text` | Label, Button, TextBox, Checkbox | A Label's `\n` starts a new line. |
| `color` | Label, Button, Checkbox | Text color: `"#RRGGBB"`, `"#AARRGGBB"` or a name (`red`, `green`, `blue`, `black`, `white`, `gray`, `darkgray`, `lightgray`, `transparent`). Reads as `"#RRGGBB"`. |
| `checked` | Checkbox | |
| `value` | Slider | 0 to 255. |
| `items` | DropDown, ListBox | A list of strings. A ListBox's item can also be `{ text = , icon = }`, with an icon name (`"16-folder.bmp"`, 16 x 16): the texts then start after the icons. Setting it clears the selection. Reads as the texts. |
| `selectedIndex` | DropDown, ListBox | From 0, as in the layout file; -1 for none. A ListBox scrolls to it. |
| `selectedItem` | DropDown, ListBox | Read only, `nil` for none. |
| `top` | ListBox | The row at the top of the view, from 0; setting it scrolls as far as the rows go. Setting `items` goes back to the top: set `top` again after it to keep the view of a list refreshed while it is looked at. |
| `title`, `message` | Dialog | |
| `state` | Dialog | Set only: `"information"` or `"error"`. |
| `foreground` | Console | Color of the text written next: `black`, `darkBlue`, `darkGreen`, `darkCyan`, `darkRed`, `darkMagenta`, `darkYellow`, `gray`, `darkGray`, `blue`, `green`, `cyan`, `red`, `magenta`, `yellow`, `white`. |
| `input` | Console | The line being typed, drawn before the cursor and not written yet; the cursor moves with it. Emptied when the console is. |
| `inputHidden` | Console | Hides the input line and the cursor (while a command runs). |
| `selection` | Console | The text selected with the mouse, `nil` for none; read only. A line the console wrapped goes on with the next one, the others end with `\n`. |
| `clickX`, `clickY` | Canvas | Where the last click on it was (its `onClick`), in pixels from its top left corner; read only. |
| `mouseX`, `mouseY` | Canvas | Where the mouse is, in pixels from its top left corner: below 0 or past its size when it is off the Canvas. Read only. |
| `pressed` | Canvas | The left button went down on it (its `onClick`) and is still down; read only. |
| `cursor` | Canvas | The cursor shown while the mouse is over it, and while the button pressed on it is down: `"normal"`, `"resizeHorizontal"`, `"resizeVertical"` or `"grab"`. |
| `focused` | TextBox, ListBox | It has the keys; read only. |

A TextBox and a ListBox have a method: `control:focus()` gives them the keys, as a click on them does
(a single line TextBox puts its cursor at the end).

An Image has a method: `image:load(path)` shows that BMP file (24 or 32 bits per pixel, bottom-up), and
the Image takes its size: `true`, or `false` and why. An `<Image>` without `src` or `icon` starts empty.

A Canvas is drawn on with methods, in pixels from its top left corner; what falls outside it is
clipped, and a drawing stays until drawn over: `canvas:clear([color])` (its `background` by default),
`canvas:fillRect(x, y, width, height, color)` and `canvas:drawText(text, x, y, color)` (the system
font, 8 x 16 pixels a character). A press of the left button on it is its `onClick` event, `clickX` and
`clickY` say where; then, while the button is down, each move of the mouse (even off the Canvas) is its
`onMove` event and the button going up its `onRelease`, `mouseX` and `mouseY` saying where. A move over
it with the button up is an `onMove` too, so the app can set its `cursor` there. The Disk Manager
selects the partition under a click, and resizes or moves the one dragged.

A Console also has methods: `console:write(text)`, `console:writeLine([text])`, `console:clear()`,
`console:scrollUp()`, `console:scrollDown()` (one line back or forward through the lines that went off
the top), `console:scrollToEnd()` and `console:clearSelection()`. A new size empties it. Dragging the
mouse over its text selects it, a double click a word (up to the spaces); the app copies it on its keys
(Terminal: Ctrl+C). A right click opens Copy, which copies the selection, and Paste, the Console's
`onPaste` event: the app types `aura.clipboard.text` (gray without `onPaste`).

### aura.system

| | |
|---|---|
| `aura.system.version`, `aura.system.revision` | The running Aura (`"0.8.0"`, `"03102026"`). |
| `aura.system.installed` | False in live mode (nothing installed, no `settings.ini`). |
| `aura.system.uptime` | Milliseconds since Aura started. |
| `aura.system.computerName` | Read and set; also the DNS host name. |
| `aura.system.latestRelease()` | Version, revision and URL of the last release (os.json). An error when it cannot be downloaded: call it with `pcall`. |
| `aura.system.compareVersions(a, b)` | -1, 0 or 1. |
| `aura.system.compareRevisions(a, b)` | -1, 0 or 1; 0 when either is not a number. |

### aura.user

| | |
|---|---|
| `aura.user.name` | The logged in user's name, read and set. |
| `aura.user.level` | The sign of the user's level, as the prompt shows it. |
| `aura.user.directory` | The user's folder (`cd ~`), `nil` when there is none (live mode). |

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
| `aura.desktop.fps` | The frames the desktop drew in the last second. |
| `aura.desktop.open(path)` | Opens a folder in the File Explorer, a file in its app (a BMP in Picture, any other file in the Editor), as the desktop does: `true`, or `false` and why. |
| `aura.desktop.start(name, ...)` | Opens the app package with that name (`"Terminal"`), with those arguments: `true`, or `false` and why. |

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

A relative path starts from the current directory, for these functions and for Lua's `io` library. `io`
reads a file a byte at a time: `readText` is faster for a whole file.

| | |
|---|---|
| `aura.fs.currentDirectory` | The shell's current directory (`cd`), ending with `/`. Setting it is a `cd`: an error for a folder that does not exist. |
| `aura.fs.resolve(path)` | The absolute path, from the current directory; gen2 paths (`0:\Users`) are converted. |
| `aura.fs.fileExists(path)`, `aura.fs.directoryExists(path)` | True when that file, or that folder, exists. `/` and the volumes (`/0/`) are folders. |
| `aura.fs.readText(path)` | The whole file as text (UTF-8, without its BOM), or `nil` and why. |
| `aura.fs.writeText(path, text)` | Replaces the file with the text (UTF-8), creating it: `true`, or `false` and why. |
| `aura.fs.list(path)` | The entries of a folder, in no order: a list of `{ name = , directory = , size = }` (bytes, 0 for a folder); or `nil` and why. At `/`, the volumes. FAT keeps no dates. |
| `aura.fs.volumes()` | The mounted volumes: a list of `{ path = "/0/", filesystem = "FAT32", label = }` (`""` for none). |
| `aura.fs.space(path)` | The free and total bytes of the volume holding the path, `nil` for none. It reads the volume's whole FAT: keep the result. |
| `aura.fs.createDirectory(path)` | Creates the folder and its missing parents: `true`, or `false` and why. |
| `aura.fs.delete(path)` | Deletes a file, or a folder and everything in it (not a volume): `true`, or `false` and why. |
| `aura.fs.copy(source, destination)` | Copies a file, or a folder and everything in it, to `destination`, a path that does not exist yet: `true`, or `false` and why. |
| `aura.fs.move(source, destination)` | Moves or renames a file or a folder to `destination`, a path that does not exist yet (a change of case only is fine), to another volume too: `true`, or `false` and why. |
| `aura.fs.clipboard` | The path the desktop's and the File Explorer's Copy keep, `nil` for none; read and set. The copied text is `aura.clipboard.text`. |

### aura.clipboard

The text Ctrl+C copies, in a TextBox or the Terminal, and Ctrl+V pastes. The taskbar's clipboard
button lists the last ones.

| | |
|---|---|
| `aura.clipboard.text` | The text Ctrl+V pastes, `nil` for none. Setting it copies a text, as Ctrl+C does. |
| `aura.clipboard.history()` | The last texts copied (10 at most), newest first: a list of strings, `text` first. |

### aura.disks

The disks and their partitions, as the Disk Manager and `vol` change them. A disk is named by its
device name (`"sata0"`, `"nvme0n1"`, `"usb0"`), a partition by its disk and the sector it starts at:
the partitions' names (`"sata0p1"`) follow their order on the disk, and change when one is added
before them. The actions run on the UI thread (the desktop waits for them, see `app:after`); each one
unmounts what it changes first and mounts the volumes again after: FAT ones, and ext2 ones of 2 or
4 KB blocks with no ext3 or ext4 feature (Cosmos's driver writes the others wrong). None of them
changes the partition Aura runs from (an installed system's volume): they return `false` and why.

| | |
|---|---|
| `aura.disks.list()` | The disks: a list of `{ name = , size = , sectors = , sectorSize = , table = , usableStart = , usableEnd = , primaries = , extended = , error = , partitions = }`. `size` is in bytes; `table` is `"MBR"`, `"GPT"`, `"none"`, or `"whole"` for a filesystem on the whole disk (with no table: its one partition starts at sector 0). New partitions go from `usableStart` to `usableEnd` (excluded). `primaries` counts an MBR's entries in use (4 at most); `extended` is an MBR's extended partition, `{ start = , sectors = }`, `nil` for none; `error` says why the disk could not be read. |
| partitions | In disk order: `{ name = , start = , sectors = , size = , volumeSectors = , filesystem = , label = , mountPoint = , system = , logical = , boot = , mbrType = , gptType = }`. `volumeSectors` is what its volume takes, which a resize cannot go under: a FAT or ext volume's size, 0 for none (`"unformatted"`), the whole partition for another filesystem. `filesystem` is `"FAT32"`, `"FAT16"`, `"FAT12"`, `"ext2"`, `"ext3"`, `"ext4"`, `"NTFS"`, `"exFAT"`, `"linux-swap"`, `"unformatted"` (only zeros at its start) or `"unknown"`; `label` is `""` for none; `mountPoint` (`"/1/"`) is `nil` when not mounted; `system` is true for the partition Aura runs from; `logical` for one in the extended partition; `boot` is the MBR's active flag; `mbrType` the MBR system ID (`0x0C`) and `gptType` the GPT type GUID (`"EBD0A0A2-..."`), `nil` on the other table. |
| `aura.disks.createTable(disk, table)` | Writes a new, empty `"MBR"` or `"GPT"`: every partition of the disk is lost. `true`, or `false` and why. |
| `aura.disks.create(disk, start, sectors, filesystem[, label])` | Adds a partition in free space and formats it: `filesystem` is `"FAT32"` (33 MB or more), `"FAT16"` (3 MB to 2 GB), `"FAT12"` (up to 127 MB), `"ext2"` (1 MB or more) or `"unformatted"`; `label` is the volume's label (as `setLabel`). Its type is the filesystem's: the MBR system ID `0x0C`, `0x0E`, `0x01` or `0x83` (Linux, for ext2 and unformatted), the GPT type Basic data, or Linux filesystem for ext2. On an MBR disk, free space in the extended partition gets a logical partition, after the last one (after its EBR, the sector before it). `true` and the sector it starts at, or `false` and why: a partition that cannot be formatted is not left behind. |
| `aura.disks.delete(disk, start)` | Removes a partition from the table: `true`, or `false` and why. |
| `aura.disks.format(disk, start, filesystem[, label])` | Writes a new filesystem (as `create`), and gives the partition its type (a logical one's stays): its files are lost. `true`, or `false` and why. |
| `aura.disks.resize(disk, start, newStart, newSectors)` | Moves the partition (its sectors are copied: slow for a large one) and resizes it (only the table changes). Its volume keeps its size: the partition can grow, not shrink below `volumeSectors`. `true`, or `false` and why. |
| `aura.disks.setLabel(disk, start, label)` | Changes a volume's label, `""` for none: a FAT one's has up to 11 letters, digits, spaces, `-` and `_`, written in capitals; an ext2 one's (on a volume Aura mounts) up to 16 ASCII letters, digits, spaces and punctuation. `true`, or `false` and why. |
| `aura.disks.mount(disk, start)` | Mounts a FAT or ext2 partition at the next free `/N/`: `true` and its mount point, or `false` and why. |
| `aura.disks.unmount(disk, start)` | Unmounts a partition, its last writes flushed (a USB stick can then be pulled out), until a reboot or the next change to its disk: `true`, or `false` and why. |

Aura writes the GPT's checksums and its backup copy at the end of the disk after each change, which
Cosmos leaves out, so other systems read the table Aura wrote. It writes ext2 as mke2fs does (4 KB
blocks, backup superblocks, a lost+found folder): Linux reads it, and Cosmos's driver its files.

### aura.shell

| | |
|---|---|
| `aura.shell.open(console)` | A shell in one of the app's Console controls, one per app. While the app is focused, `Console.Out` writes into that console: the commands' output, and anything else written to it. |
| `shell.execute(line)` | Runs a command line (`help` lists the commands) and returns once it is done. `clear` empties the console, `exit` closes the app once the handler returns. |

### aura.packages

The packages Aura has and the online repository's, as `pkg` manages them. The downloads run on the UI
thread: the desktop waits for them (see `app:after`).

| | |
|---|---|
| `aura.packages.repository` | The repository's address. Read only: see `setRepository`. |
| `aura.packages.defaultRepository` | The repository Aura comes with. |
| `aura.packages.setRepository(url)` | Switches to that repository (an `http://` or `https://` address), and forgets the package list of the previous one: `true`, or `false` and why. Kept in `settings.ini` on an installed Aura. |
| `aura.packages.list()` | The packages Aura has: a list of `{ name = , displayName = , version = , author = , description = , builtIn = , app = , menu = }`. `builtIn` is false for a downloaded one, which may replace a built-in one of the same name; `menu` is false for an app kept out of the start menu (`menu="false"`: one that needs a file to open). |
| `aura.packages.update()` | Downloads the repository's package list: `true`, or `false` and why. |
| `aura.packages.available()` | The repository's packages, as the last `update()` read them (none before): a list of `{ name = , displayName = , version = , author = , description = , link = }`. |
| `aura.packages.add(name)` | Downloads a package of that list and installs it, replacing the one of the same name (a built-in one too): `true`, or `false` and why. An app shows in the start menu at once. |
| `aura.packages.remove(name)` | Removes a downloaded package; the built-in one it replaced is back: `true`, or `false` and why. |

### aura.processes

Aura's processes and the kernel's threads, as the Task Manager shows them. Every process runs on the
main loop's thread, which draws the desktop frame after frame and never waits. The kernel times each
process's updates and the drawing of its window, and counts the main loop's other time (putting the
frame on the screen, collecting memory) as the system's. The times are in nanoseconds and only grow:
a share of the CPU is how much one grew between two reads, over how much `times().clock` did.

| | |
|---|---|
| `aura.processes.list()` | The processes, in their start order: a list of `{ id = , name = , app = , running = , focused = , package = , icon = , cpuTime = }`. `id` is the one `kill` takes; `name` is an app's window title; `app` is true for an app (it has a window); `running` is false for a minimized app, whose process stops; `package` is an app's package name and `icon` its window's icon, `nil` for none; `cpuTime` is the main loop's time in the process. |
| `aura.processes.times()` | `{ clock = , system = , threads = }`, read together: the clock (since boot), the main loop's time in no process, and the time the other threads ran. Those take the CPU from the main loop now and then, so the processes' and the system's times count it too. |
| `aura.processes.threads()` | The kernel's threads: a list of `{ id = , name = , state = , main = , managed = , cpuTime = , stack = , priority = }`. `name` is the one a thread Aura started gave itself (`"HTTP server"`), `nil` for the kernel's; `state` is `"created"`, `"ready"`, `"running"`, `"blocked"` or `"sleeping"`; `main` is true for the main loop's thread, `managed` for one a `System.Threading.Thread` started; `cpuTime` grows a scheduler tick (10 ms) at a time; `stack` is in bytes; `priority` is the scheduler's (the stride scheduler's tickets: more gets more of the CPU), `nil` when it gives none. |
| `aura.processes.close(id)` | Closes an app, as its close button does: what it has not saved is lost. The calling app closes once its handler returns. `true`, or `false` and why: the desktop's processes (Explorer, the mouse's and the keyboard's) have no window, and run as long as Aura. |
| `aura.processes.switchTo(id)` | Brings an app's window over the others, focused, restored first when minimized: `true`, or `false` and why. |

## Limits

- A handler that never returns (`while true do end`) freezes the desktop: nothing stops it yet (the
  interpreter's hook is not public, see `lua-host` in GEN3-GAPS.md).
- Packages are not signed: Aura installs what the repository serves.
