if (not RSAS) then
	RSAS = {}
end

RSAS.Networking = {}
RSAS.Networking.Hooks = {}

function RSAS.Networking.SendTable(identifier, data)
	_RSAS_Networking_SendTable(identifier, data)
end

function RSAS.Networking.GetTable(identifier, data)
	_RSAS_Networking_GetTable(identifier, data)
end

function RSAS.Networking.Hook(trigger, callback)
	if (not RSAS.Networking.Hooks[trigger]) then
		RSAS.Networking.Hooks[trigger] = {}
	end

	table.insert(RSAS.Networking.Hooks[trigger], callback)
end

function RSAS.Networking.TriggerCallback(trigger)
	if (not RSAS.Networking.Hooks[trigger]) then return end

	for _, v in pairs(RSAS.Networking.Hooks[trigger]) do
		local data = {}
		RSAS.Networking.GetTable(trigger, data)
		v(data)
	end
	
end