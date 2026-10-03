-- The filesystems Aura formats: the partitions each takes, the labels it writes, the partition type it
-- gets. And the partition types by name.

-- The filesystems, as the panel's drop-down lists them.
local NAMES = { "FAT32", "FAT16", "FAT12", "ext2", "unformatted" }

-- The smallest and largest partition each filesystem takes, in MB, as its formatter needs it.
local MINIMUM_MB = { FAT32 = 33, FAT16 = 3, FAT12 = 1, ext2 = 1, unformatted = 1 }
local MAXIMUM_MB = { FAT16 = 2047, FAT12 = 127 }

-- What they take, for the panel.
local SIZES = "FAT32 takes 33 MB or more, FAT16 3 MB to 2 GB, FAT12 up to 127 MB, ext2 1 MB or more."

-- What a label holds, for the panel.
local LABEL_RULES = {
  FAT = "Up to 11 letters, digits, spaces, - and _: FAT writes them in capitals. Empty for no label.",
  ext2 = "Up to 16 ASCII letters, digits, spaces and punctuation. Empty for no label.",
}

-- The MBR system ID and the GPT type Aura gives each filesystem.
local MBR_IDS = { FAT32 = 0x0C, FAT16 = 0x0E, FAT12 = 0x01, ext2 = 0x83, unformatted = 0x83 }
local BASIC_DATA = "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"
local LINUX_FILESYSTEM = "0FC63DAF-8483-4772-8E79-3D69D8477DE4"

-- MBR system IDs and GPT type GUIDs, by name; a GPT type's flag, as GParted shows it.
local MBR_TYPES = {
  [0x01] = "FAT12", [0x04] = "FAT16 (< 32 MB)", [0x05] = "Extended", [0x06] = "FAT16",
  [0x07] = "NTFS or exFAT", [0x0B] = "FAT32", [0x0C] = "FAT32 (LBA)", [0x0E] = "FAT16 (LBA)",
  [0x0F] = "Extended (LBA)", [0x82] = "Linux swap", [0x83] = "Linux", [0x85] = "Linux extended",
  [0x8E] = "Linux LVM", [0xEE] = "GPT protective", [0xEF] = "EFI System",
}

local GPT_TYPES = {
  ["C12A7328-F81F-11D2-BA4B-00A0C93EC93B"] = { "EFI System", "esp" },
  [BASIC_DATA] = { "Basic data" },
  [LINUX_FILESYSTEM] = { "Linux filesystem" },
  ["21686148-6449-6E6F-744E-656564454649"] = { "BIOS boot", "bios_grub" },
  ["E3C9E316-0B5C-4DB8-817D-F92DF00215AE"] = { "Microsoft reserved", "msftres" },
  ["DE94BBA4-06D1-4D40-A16A-BFD50179D6AC"] = { "Windows recovery" },
  ["0657FD6D-A4AB-43C4-84E5-0933C84B4F4F"] = { "Linux swap" },
  ["E6D6D379-F507-44C2-A23C-238F2A3DF928"] = { "Linux LVM" },
}

local function isFat(filesystem)
  return filesystem:sub(1, 3) == "FAT"
end

-- Whether Aura writes a label on a volume of that filesystem.
local function canLabel(filesystem)
  return isFat(filesystem) or filesystem == "ext2"
end

-- The label as the filesystem writes it: a FAT one in capitals, none on an unformatted partition.
local function labelAs(filesystem, label)
  label = (label or ""):match("^%s*(.-)%s*$")

  if isFat(filesystem) then
    return label:upper()
  end

  return filesystem == "ext2" and label or ""
end

-- Why that label does not go on a volume of that filesystem, nil when it does.
local function labelProblem(filesystem, label)
  label = label:match("^%s*(.-)%s*$")

  if isFat(filesystem) then
    if #label > 11 or label:find("[^%w _%-]") then
      return "A FAT label has up to 11 letters, digits, spaces, - and _."
    end
  elseif filesystem == "ext2" then
    if #label > 16 or label:find("[^\32-\126]") then
      return "An ext2 label has up to 16 ASCII letters, digits, spaces and punctuation."
    end
  end

  return nil
end

-- What a label of that filesystem holds, for the panel.
local function labelRules(filesystem)
  if isFat(filesystem) then
    return LABEL_RULES.FAT
  end

  return LABEL_RULES[filesystem] or "An unformatted partition has no label."
end

-- Why a partition of that many MB cannot take that filesystem, nil when it can.
local function sizeProblem(filesystem, megabytes)
  if megabytes < MINIMUM_MB[filesystem] then
    return filesystem .. " takes " .. MINIMUM_MB[filesystem] .. " MB or more."
  elseif MAXIMUM_MB[filesystem] and megabytes > MAXIMUM_MB[filesystem] then
    return filesystem .. " takes up to " .. MAXIMUM_MB[filesystem] .. " MB."
  end

  return nil
end

-- The index (from 0) in NAMES of the largest FAT that fits in that many MB.
local function defaultFilesystem(megabytes)
  if megabytes >= MINIMUM_MB.FAT32 then
    return 0
  end

  return megabytes >= MINIMUM_MB.FAT16 and 1 or 2
end

local function mbrIdOf(filesystem)
  return MBR_IDS[filesystem]
end

local function gptTypeOf(filesystem)
  return filesystem == "ext2" and LINUX_FILESYSTEM or BASIC_DATA
end

-- A partition's type by name: "0x0C FAT32 (LBA)", "Basic data"; nil for none.
local function typeOf(partition)
  if partition.mbrType then
    local name = MBR_TYPES[partition.mbrType]
    return string.format("0x%02X", partition.mbrType) .. (name and " " .. name or "")
  elseif partition.gptType then
    local gptType = GPT_TYPES[partition.gptType]
    return gptType and gptType[1] or partition.gptType
  end

  return nil
end

-- A partition's flags, as GParted shows them: "boot", "esp".
local function flagsOf(partition)
  local flags = {}

  if partition.boot then
    flags[#flags + 1] = "boot"
  end

  local gptType = partition.gptType and GPT_TYPES[partition.gptType]

  if gptType and gptType[2] then
    flags[#flags + 1] = gptType[2]
  end

  return table.concat(flags, ", ")
end

return {
  NAMES = NAMES,
  MINIMUM_MB = MINIMUM_MB,
  MAXIMUM_MB = MAXIMUM_MB,
  SIZES = SIZES,
  canLabel = canLabel,
  labelAs = labelAs,
  labelProblem = labelProblem,
  labelRules = labelRules,
  sizeProblem = sizeProblem,
  defaultFilesystem = defaultFilesystem,
  mbrIdOf = mbrIdOf,
  gptTypeOf = gptTypeOf,
  typeOf = typeOf,
  flagsOf = flagsOf,
}
