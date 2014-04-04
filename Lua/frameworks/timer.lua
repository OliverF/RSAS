if (not RSAS) then
	RSAS = {}
end

RSAS.Timer = {}
RSAS.Timer.Hooks = {}

function RSAS.Timer.Hook(id, delay, callback)
	if (not RSAS.Timer.Hooks[id]) then
		RSAS.Timer.Hooks[id] = {}
	end

	--track the source of the script requesting the callback so that errors in the callbacks can be correctly blamed
	table.insert(RSAS.Timer.Hooks[id], {callback = callback, source = _RSAS_Source})

	_RSAS_Timer_Register(id, delay)
end

function RSAS.Timer.TriggerCallback(trigger)
	if (not RSAS.Timer.Hooks[trigger]) then return end

	for _, v in pairs(RSAS.Timer.Hooks[trigger]) do
		--ensure calls beyond this are attributed to the source script which requested the callback
		_RSAS_SetSource(v.source)
		v.callback()
	end
	
end