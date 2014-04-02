if (not RSAS) then
	RSAS = {}
end

RSAS.IO = {}

function RSAS.IO.Read(path)
	return _RSAS_IO_Read(path)
end

function RSAS.IO.Write(path, content, append)
	_RSAS_IO_Write(path, content, append)
end

function RSAS.IO.DeleteFile(path)
	_RSAS_IO_DeleteFile(path)
end

function RSAS.IO.DeleteDirectory(path, recurse)
	_RSAS_IO_DeleteDirectory(path, recurse)
end

function RSAS.IO.CreateDirectory(path)
	_RSAS_IO_CreateDirectory(path)
end

function RSAS.IO.FileExists(path)
	return _RSAS_IO_FileExists(path)
end

function RSAS.IO.DirectoryExists(path)
	return _RSAS_IO_DirectoryExists(path)
end