if (not RSAS) then
	RSAS = {}
end

RSAS.Timer = {}
RSAS.Timer.Hooks = {}

function RSAS.Timer.Hook(id, delay, callback)
	if (not RSAS.Timer.Hooks[id]) then
		RSAS.Timer.Hooks[id] = {}
	end

	table.insert(RSAS.Timer.Hooks[id], callback)
	_RSAS_Timer_Register(id, delay)
end

function RSAS.Timer.TriggerCallback(trigger)
	if (not RSAS.Timer.Hooks[trigger]) then return end

	for _, v in pairs(RSAS.Timer.Hooks[trigger]) do
		v()
	end
	
end