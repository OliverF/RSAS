if (not RSAS) then
	RSAS = {}
end

RSAS.GUI = {}

--Track controls
RSAS.GUI.Controls = {}

--Handle input callbacks
function RSAS.GUI.Trigger(controlID, callback)
	local control = RSAS.GUI.Controls[controlID]

	if (not control) then return end

	local callback = control[callback]

	if (callback) then
		callback()
	end
end

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

	RSAS.GUI.Controls[controlID] = newControl

	--assign specific class methods
	if (controlType == "chart") then
		setmetatable(newControl, RSAS.GUI.Chart)
	elseif(controlType == "label") then
		setmetatable(newControl, RSAS.GUI.Label)
	elseif(controlType == "container") then
		setmetatable(newControl, RSAS.GUI.Container)
	elseif(controlType == "button") then
		setmetatable(newControl, RSAS.GUI.Button)
	end

	return newControl
end

function RSAS.GUI.Control:GetID()
	return self.ControlID
end

function RSAS.GUI.Control:GetType()
	return self.ControlType
end

function RSAS.GUI.Control:SetParent(parent)
	_RSAS_GUI_Control_SetParent(self.ControlID, parent.ControlID)
end

function RSAS.GUI.Control:GetParent()
	local parentID = _RSAS_GUI_Control_GetParent(self.ControlID)

	if (parentID and RSAS.GUI.Controls[parentID]) then
		return RSAS.GUI.Controls[parentID]
	else
		return false
	end
end

function RSAS.GUI.Control:SetLocation(x, y)
	_RSAS_GUI_Control_SetLocation(self.ControlID, x, y)
end

function RSAS.GUI.Control:GetLocation()
	return _RSAS_GUI_Control_GetLocation(self.ControlID)
end

function RSAS.GUI.Control:SetSize(width, height)
	_RSAS_GUI_Control_SetSize(self.ControlID, width, height)
end

function RSAS.GUI.Control:GetSize()
	return _RSAS_GUI_Control_GetSize(self.ControlID)
end

function RSAS.GUI.Control:Remove()
	_RSAS_GUI_Control_Remove(self.ControlID)
end



local Chart = {}
Chart.__index = Chart

function Chart:SetXY(data, seriesName)
	_RSAS_GUI_Chart_SetXY(self.ControlID, data, seriesName)
end

function Chart:CreateSeries(seriesName, seriesType)
	_RSAS_GUI_Chart_CreateSeries(self.ControlID, seriesName, seriesType)
end

function Chart:SetAxesLimits(xAxisMin, xAxisMax, yAxisMin, yAxisMax)
	_RSAS_GUI_Chart_SetAxesLimits(self.ControlID, xAxisMin, xAxisMax, yAxisMin, yAxisMax)
end

--give methods from Control to Chart
setmetatable(Chart, RSAS.GUI.Control)

RSAS.GUI.Chart = Chart



local Label = {}
Label.__index = Label

function Label:SetText(text)
	_RSAS_GUI_Label_SetText(self.ControlID, text)
end

setmetatable(Label, RSAS.GUI.Control)

RSAS.GUI.Label = Label



local Container = {}
Container.__index = Container

setmetatable(Container, RSAS.GUI.Control)

RSAS.GUI.Container = Container



local Button = {}
Button.__index = Button

function Button:SetText(text)
	_RSAS_GUI_Button_SetText(self.ControlID, text)
end

setmetatable(Button, RSAS.GUI.Control)

RSAS.GUI.Button = Button