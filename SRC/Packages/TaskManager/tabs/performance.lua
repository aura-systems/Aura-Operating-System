-- The Performance tab: the CPU over the last minute, split between the apps, the other threads and the
-- desktop; the memory in use; and the frame rate, the up time and the counts.

local ui = require "ui"
local format = require "format"
local sampler = require "sampler"
local graph = require "graph"

-- As the legends' colors in TaskManager.xml.
local COLORS = {
  apps = "#00A000",
  threads = "#C08000",
  desktop = "#3050C0",
  memory = "#008080",
}

local history = sampler.history

local function draw()
  graph.draw(ui.cpuGraph, { history.apps, history.threads, history.desktop },
    { COLORS.apps, COLORS.threads, COLORS.desktop }, sampler.HISTORY, history.count)
  graph.draw(ui.memoryGraph, { history.memory }, { COLORS.memory }, sampler.HISTORY, history.count)
end

local function update(sample)
  local cpu = sample.cpu

  ui.cpuApps.text = "Apps " .. format.percent(cpu and cpu.apps)
  ui.cpuThreads.text = "Other threads " .. format.percent(cpu and cpu.threads)
  ui.cpuDesktop.text = "Desktop " .. format.percent(cpu and cpu.desktop)

  local used, total = sample.memory.used, sample.memory.total
  ui.memoryUsed.text = format.size(used) .. " used of " .. format.size(total)
    .. (total > 0 and " (" .. format.percent(used / total) .. ")" or "")

  ui.fps.text = tostring(sample.fps)
  ui.uptime.text = format.clock(sample.uptime // 1000)
  ui.processCount.text = tostring(#sample.processes)
  ui.threadCount.text = tostring(#sample.threads)
  ui.heap.text = format.size(sample.memory.heap)
  ui.collections.text = tostring(sample.collections)

  draw()
end

local function show(sample)
  if sample then
    update(sample)
  end
end

return {
  show = show,
  update = update,
  draw = draw,
}
