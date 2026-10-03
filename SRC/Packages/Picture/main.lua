-- Picture: shows a BMP file, the window fitted to it. Its argument is the file's path (pic {file},
-- run Picture {file}, a .bmp opened from the desktop).

local app = aura.app
local path = ...

if not path or path == "" then
  error("no picture to show: run Picture {file.bmp}", 0)
end

path = aura.fs.resolve(path)

local shown, why = app:find("image"):load(path)

if not shown then
  error("cannot open '" .. path .. "': " .. why, 0)
end

-- The file name as the title, before fit: the window is at least as wide as its title.
app.title = path:match("[^/]*$")
app:fit()
