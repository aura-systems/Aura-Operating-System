-- Sizes and text as the window shows them.

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

-- The text in lines of at most that many characters, cut at the spaces.
local function wrap(text, columns)
  local lines = {}

  for paragraph in (text .. "\n"):gmatch("(.-)\n") do
    local line = ""

    for each in paragraph:gmatch("%S+") do
      -- Lua 5.5's loop variables are constants.
      local word = each

      while #word > columns do
        if line ~= "" then
          lines[#lines + 1] = line
          line = ""
        end

        lines[#lines + 1] = word:sub(1, columns)
        word = word:sub(columns + 1)
      end

      if line == "" then
        line = word
      elseif #line + 1 + #word <= columns then
        line = line .. " " .. word
      else
        lines[#lines + 1] = line
        line = word
      end
    end

    lines[#lines + 1] = line
  end

  return table.concat(lines, "\n")
end

-- "1 partition", "3 partitions".
local function plural(count, word)
  return count .. " " .. word .. (count == 1 and "" or "s")
end

return {
  size = size,
  wrap = wrap,
  plural = plural,
}
