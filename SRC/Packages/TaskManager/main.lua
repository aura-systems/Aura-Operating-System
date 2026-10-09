-- Task Manager: the running apps and processes and their share of the CPU, the CPU and the memory over
-- the last minute, and the kernel's threads. It ends an app, brings one to the front, or opens one.
--
-- Every Aura process runs on the main loop's thread, which draws the desktop frame after frame and never
-- waits: the CPU is always busy, and the shares say where its time goes. The kernel times each
-- process's updates and drawing, and each thread (aura.processes).
--
-- The code, each module requiring only those listed before it:
--   ui.lua        the window's controls (TaskManager.xml)
--   format.lua    percentages, times, sizes and columns
--   sampler.lua   the figures, a sample each second, and their history for the graphs
--   status.lua    the status line
--   graph.lua     a history drawn on a Canvas
--   tabs/         processes (End task, Switch to, New task), performance (the graphs), threads
-- This file shows one tab at a time, gives the layout's events and the keys to the tabs, and samples
-- each second.

local app = aura.app

local ui = require "ui"
local sampler = require "sampler"
local status = require "status"
local processes = require "tabs.processes"
local performance = require "tabs.performance"
local threads = require "tabs.threads"

local REFRESH_MS = 1000

local TABS = {
  processes = processes,
  performance = performance,
  threads = threads,
}

-- The tab shown.
local shown

local function show(name)
  shown = name

  -- The view shown first: showing a Stack shows all its elements.
  for each in pairs(TABS) do
    ui.views[each].visible = each == name
    ui.tabs[each].color = each == name and "blue" or "black"
  end

  TABS[name].show(sampler.current())
end

local function refresh()
  local sample = sampler.sample()
  TABS[shown].update(sample)
  status.summarize(sample)
end

app:on("processesTab", function() show("processes") end)
app:on("performanceTab", function() show("performance") end)
app:on("threadsTab", function() show("threads") end)
app:on("endTask", processes.endTask)
app:on("switchTo", processes.switchTo)
app:on("newTask", processes.newTask)
app:on("confirmOk", processes.confirmOk)
app:on("closeConfirm", processes.closeConfirm)

-- The keys the lists do not use.
app:onKey(function(key)
  local name = key.name

  if ui.confirm.visible then
    if name == "enter" then
      processes.confirmOk()
    elseif name == "escape" then
      processes.closeConfirm()
    end
  elseif name == "delete" and shown == "processes" then
    processes.endTask()
  elseif name == "f5" then
    refresh()
  end
end)

-- A new size clears the graphs.
app:onResize(function()
  if shown == "performance" then
    performance.draw()
  end
end)

show("processes")
refresh()
ui.processList:focus()
app:every(REFRESH_MS, refresh)
