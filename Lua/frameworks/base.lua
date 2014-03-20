if (not RSAS) then
	RSAS = {}
end

function RSAS.Print(value)
	_RSAS_Print(value)
end

function RSAS.Execute(cmd, cmdArgs)
	return _RSAS_Execute(cmd, cmdArgs)
end