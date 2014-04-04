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

	--track the source of the script requesting the callback so that errors in the callbacks can be correctly blamed
	table.insert(RSAS.Networking.Hooks[trigger], {callback = callback, source = _RSAS_Source})
end

function RSAS.Networking.TriggerCallback(trigger)
	if (not RSAS.Networking.Hooks[trigger]) then return end

	for _, v in pairs(RSAS.Networking.Hooks[trigger]) do
		local data = {}
		RSAS.Networking.GetTable(trigger, data)
		--ensure calls beyond this are attributed to the source script which requested the callback
		_RSAS_SetSource(v.source)
		v.callback(data)
	end
	
end