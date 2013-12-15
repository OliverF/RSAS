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
RSAS.GUI.Control.ControlID = ""

function RSAS.GUI.Control.New(controlType, controlID)
	local newControl = {}
	--give newControl the RSAS.GUI.Control metatable by default
	setmetatable(newControl, RSAS.GUI.Control)
	newControl.ControlType = controlType
	newControl.ControlID = controlID

	--create the control with controlID handle
	_RSAS_GUI_CreateControl(controlType, controlID)

	--assign specific class methods
	if (controlType == "chart") then
		setmetatable(newControl, RSAS.GUI.Chart)
	end

	return newControl
end

function RSAS.GUI.Control:GetID()
	return self.ControlID
end

function RSAS.GUI.Control:GetType()
	return self.ControlType
end



local Chart = {}
Chart.__index = Chart

function Chart:SetXY(data, seriesName)
	_RSAS_GUI_Chart_SetXY(self.ControlID, data, seriesName)
end

function Chart:CreateSeries(seriesName, seriesType)
	_RSAS_GUI_Chart_CreateSeries(self.ControlID, seriesName, seriesType)
end

--give methods from Control to Chart
setmetatable(Chart, RSAS.GUI.Control)

RSAS.GUI.Chart = Chart