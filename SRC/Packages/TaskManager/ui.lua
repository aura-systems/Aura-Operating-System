-- The window's controls (TaskManager.xml), by name.

local app = aura.app

return {
  -- The tabs' buttons and their views, by tab.
  tabs = {
    processes = app:find("processesTab"),
    performance = app:find("performanceTab"),
    threads = app:find("threadsTab"),
  },
  views = {
    processes = app:find("processesView"),
    performance = app:find("performanceView"),
    threads = app:find("threadsView"),
  },

  processesHeader = app:find("processesHeader"),
  processList = app:find("processes"),
  newTask = app:find("newTask"),

  cpuApps = app:find("cpuApps"),
  cpuThreads = app:find("cpuThreads"),
  cpuDesktop = app:find("cpuDesktop"),
  cpuGraph = app:find("cpuGraph"),
  memoryUsed = app:find("memoryUsed"),
  memoryGraph = app:find("memoryGraph"),
  fps = app:find("fps"),
  uptime = app:find("uptime"),
  processCount = app:find("processCount"),
  threadCount = app:find("threadCount"),
  heap = app:find("heap"),
  collections = app:find("collections"),

  threadsHeader = app:find("threadsHeader"),
  threadList = app:find("threads"),

  status = app:find("status"),
  confirm = app:find("confirm"),
}
