-- The window's controls (DiskManager.xml), by name.

local app = aura.app

return {
  diskBox = app:find("disks"),
  bar = app:find("bar"),
  list = app:find("partitions"),
  header = app:find("header"),
  headerPlain = app:find("headerPlain"),
  status = app:find("status"),
  mountButton = app:find("mount"),
  unmountButton = app:find("unmount"),

  -- The panel: its title, a row of fields for each thing an action asks, its text, OK and Cancel.
  panel = app:find("panel"),
  panelTitle = app:find("panelTitle"),
  panelText = app:find("panelText"),
  fields = app:find("fields"),
  okButton = app:find("ok"),
  cancelButton = app:find("cancel"),

  rows = {
    before = app:find("beforeRow"),
    size = app:find("sizeRow"),
    filesystem = app:find("filesystemRow"),
    label = app:find("labelRow"),
    table = app:find("tableRow"),
  },

  beforeBox = app:find("before"),
  sizeBox = app:find("size"),
  filesystemBox = app:find("filesystem"),
  labelBox = app:find("label"),
  tableBox = app:find("tableType"),

  confirm = app:find("confirm"),

  -- The pending operations, under the list.
  operations = app:find("operations"),
  operationsTitle = app:find("operationsTitle"),
  operationList = app:find("operationList"),
}
