-- The status line: what an action did, for a few seconds; else the last sample in short.

local ui = require "ui"
local format = require "format"

local system = aura.system

-- How long an action's message stays, in milliseconds.
local MESSAGE_MS = 5000

-- aura.system.uptime until which the message stays.
local shownUntil = 0

local function tell(text, color)
  ui.status.text = text
  ui.status.color = color or "black"
  shownUntil = system.uptime + MESSAGE_MS
end

-- "7 processes   CPU: apps 12%, threads 0.4%   Memory: 45 MB of 512 MB"; nothing while a message stays.
local function summarize(sample)
  if system.uptime < shownUntil then
    return
  end

  local text = #sample.processes .. " processes"

  if sample.cpu then
    text = text .. "   CPU: apps " .. format.percent(sample.cpu.apps) .. ", threads " .. format.percent(sample.cpu.threads)
  end

  ui.status.text = text .. "   Memory: " .. format.size(sample.memory.used) .. " of " .. format.size(sample.memory.total)
  ui.status.color = "black"
end

return {
  tell = tell,
  summarize = summarize,
}
