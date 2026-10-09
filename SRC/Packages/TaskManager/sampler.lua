-- The figures, a sample each second. The kernel gives times that only grow (aura.processes): a share of
-- the CPU is how much one grew since the previous sample, over how much the clock did.
--
-- The CPU is split three ways. The other threads (the servers, the drivers') take it from the main
-- loop now and then; the rest is the main loop's, which runs the apps (their updates, their drawing)
-- and the desktop (Explorer, the mouse and the keyboard, and the system's time: putting the frame on
-- the screen, collecting memory). It never waits, so the three always make the whole CPU.

local processes = aura.processes
local memory = aura.memory
local system = aura.system
local desktop = aura.desktop

-- How many samples the graphs keep: a minute.
local HISTORY = 60

-- The last sample's times, to compare the next with: { clock = , system = , threads = , processes = ,
-- threadTimes = }, the last two by id.
local previous

-- The last sample (see sample), nil before the first.
local current

-- The shares over the last samples, oldest first, for the graphs; count is how many were ever taken.
local history = { apps = {}, threads = {}, desktop = {}, memory = {}, count = 0 }

local function push(list, value)
  list[#list + 1] = value

  if #list > HISTORY then
    table.remove(list, 1)
  end
end

-- How much a time grew, over the time elapsed: from 0 to 1, nil without a previous time.
local function grown(now, before, elapsed)
  if not before or elapsed <= 0 then
    return nil
  end

  return math.max(0, math.min(1, (now - before) / elapsed))
end

-- Takes a sample: { processes = , system = , systemTime = , threads = , cpu = { apps = , threads = ,
-- desktop = }, memory = { used = , total = , heap = }, fps = , uptime = , collections = }.
-- processes is aura.processes.list(), each with its share as cpu; threads is aura.processes.threads(),
-- the same. The shares are nil in the first sample.
local function sample()
  local times = processes.times()
  local list = processes.list()
  local threads = processes.threads()

  local elapsed = previous and times.clock - previous.clock or 0
  local threadsShare = grown(times.threads, previous and previous.threads, elapsed)

  -- The main loop's times count the moments the other threads took the CPU from it: take that out.
  local mainShare = 1 - (threadsShare or 0)

  local function share(now, before)
    local part = grown(now, before, elapsed)
    return part and part * mainShare
  end

  local processTimes, threadTimes = {}, {}
  local apps, others = 0, 0

  for _, process in ipairs(list) do
    process.cpu = share(process.cpuTime, previous and previous.processes[process.id])
    processTimes[process.id] = process.cpuTime

    if process.app then
      apps = apps + (process.cpu or 0)
    else
      others = others + (process.cpu or 0)
    end
  end

  local systemShare = share(times.system, previous and previous.system)

  for _, thread in ipairs(threads) do
    thread.cpu = grown(thread.cpuTime, previous and previous.threadTimes[thread.id], elapsed)
    threadTimes[thread.id] = thread.cpuTime
  end

  previous = { clock = times.clock, system = times.system, threads = times.threads,
    processes = processTimes, threadTimes = threadTimes }

  local totalPages, freePages, pageSize = memory.totalPages, memory.freePages, memory.pageSize

  current = {
    processes = list,
    system = systemShare,
    systemTime = times.system,
    threads = threads,
    cpu = threadsShare and { apps = apps, threads = threadsShare, desktop = others + (systemShare or 0) },
    memory = { used = (totalPages - freePages) * pageSize, total = totalPages * pageSize, heap = memory.liveHeap },
    fps = desktop.fps,
    uptime = system.uptime,
    collections = memory.collections,
  }

  if current.cpu then
    push(history.apps, current.cpu.apps)
    push(history.threads, current.cpu.threads)
    push(history.desktop, current.cpu.desktop)
    push(history.memory, current.memory.total > 0 and current.memory.used / current.memory.total or 0)
    history.count = history.count + 1
  end

  return current
end

return {
  HISTORY = HISTORY,
  history = history,
  sample = sample,
  current = function() return current end,
}
