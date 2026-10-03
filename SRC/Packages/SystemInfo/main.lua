-- SystemInfo: the running Aura version, and a check against the last release.

local app = aura.app
local system = aura.system

local running = system.version .. "-" .. system.revision

app:find("version").text = "[version " .. running .. "]"

-- The update check result, and whether it is bad news (shown in red).
local function updateStatus()
  if not aura.network.isConfigured() then
    return "Aura [version " .. running .. "]", false
  end

  -- Synchronous on the UI thread. The download fails until os.json is served over plain HTTP
  -- (no TLS on gen3).
  local ok, version, revision = pcall(system.latestRelease)

  if not ok then
    aura.log("Update check failed: " .. tostring(version))
    return "Failed to check for updates.", false
  end

  if system.version == "" or system.revision == "" or not version or version == "" or not revision or revision == "" then
    return "Failed to parse os.json.", false
  end

  local latest = version .. "-" .. revision
  local versions = system.compareVersions(system.version, version)

  if versions > 0 then
    return "You are on a dev version (last release is " .. latest .. ").", false
  elseif versions < 0 then
    return "Your version is outdated (last release is " .. latest .. ").", true
  end

  local revisions = system.compareRevisions(system.revision, revision)

  if revisions > 0 then
    return "You are on a dev version (last release is " .. latest .. ").", false
  elseif revisions < 0 then
    return "Your revision is outdated (last release is " .. latest .. ").", true
  end

  return "You are up to date.", false
end

-- The result takes the button's place.
app:on("checkUpdate", function()
  local status, outdated = updateStatus()
  local label = app:find("updateStatus")

  label.text = status
  label.color = outdated and "red" or "green"
  label.visible = true
  app:find("checkUpdate").visible = false
end)
