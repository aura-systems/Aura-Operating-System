-- Percentages, times, sizes and columns as the window shows them.

-- A share (0 to 1) in percent, one decimal under 10: "0.4%", "12%"; "" for none (a process sampled once).
local function percent(share)
  if not share then
    return ""
  end

  local tenths = math.floor(share * 1000 + 0.5)

  if tenths < 100 then
    return (tenths // 10) .. "." .. (tenths % 10) .. "%"
  end

  return (tenths // 10) .. "%"
end

-- Hours, minutes and seconds: "0:01:05".
local function clock(seconds)
  return string.format("%d:%02d:%02d", seconds // 3600, seconds // 60 % 60, seconds % 60)
end

-- A time in nanoseconds, as a clock.
local function duration(nanoseconds)
  return clock(nanoseconds // 1000000000)
end

-- A size in bytes: "512 B", "1.5 MB", "20 GB".
local function size(bytes)
  if bytes < 1024 then
    return bytes .. " B"
  end

  local units = { "KB", "MB", "GB", "TB" }
  local unit, divisor = 1, 1024

  while unit < #units and bytes >= divisor * 1024 do
    unit, divisor = unit + 1, divisor * 1024
  end

  -- In integers: one decimal under 10.
  local tenths = bytes * 10 // divisor

  if tenths < 100 then
    return (tenths // 10) .. "." .. (tenths % 10) .. " " .. units[unit]
  end

  return (tenths // 10) .. " " .. units[unit]
end

-- A list's row: the values in the columns ({ title = , width = , right = }, a width in characters,
-- right for numbers), cut with "~" when too long. The titles make the header over the list.
local function columns(spec, values)
  local parts = {}

  for i, column in ipairs(spec) do
    local text = values[i] or ""

    if #text > column.width then
      text = text:sub(1, column.width - 1) .. "~"
    end

    local padding = string.rep(" ", column.width - #text)
    parts[i] = column.right and padding .. text or text .. padding
  end

  return (table.concat(parts, " "):gsub("%s+$", ""))
end

local function header(spec)
  local titles = {}

  for i, column in ipairs(spec) do
    titles[i] = column.title
  end

  return columns(spec, titles)
end

return {
  percent = percent,
  clock = clock,
  duration = duration,
  size = size,
  columns = columns,
  header = header,
}
