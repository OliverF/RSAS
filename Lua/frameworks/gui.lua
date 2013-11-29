if (not RSAS) then
	RSAS = {}
end

RSAS.GUI = {}

--GUI Control class
RSAS.GUI.Control = {}

--redirect calls to this table
RSAS.GUI.Control.__index = RSAS.GUI.Control

--allow instances of this table to be created when this table is called as a function (__call) E.G. RSAS.GUI.Control()
setmetatable(RSAS.GUI.Control, {
	__call = function(thisTable, ...)
				return thisTable.New(...)
			end
	})

RSAS.GUI.Control.ControlType = ""

function RSAS.GUI.Control.New(controlType)
	local newControl = {}
	--give newControl the RSAS.GUI.Control metatable
	setmetatable(newControl, RSAS.GUI.Control)
	newControl.ControlType = controlType
	return newControl
end