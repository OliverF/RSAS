using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lua4Net;
using Lua4Net.Types;

namespace RSAS.Plugins.Frameworks
{
    public class IO : PluginFramework
    {
        public IO()
        {
            this.frameworkScriptNames.Add("io.lua");

            this.registerEvents.Add(delegate(ThreadSafeLua lua)
            {
                lua.RegisterGlobalFunction("_RSAS_IO_Read", Read);

                lua.RegisterGlobalFunction("_RSAS_IO_Write", Write);

                lua.RegisterGlobalFunction("_RSAS_IO_DeleteFile", DeleteFile);

                lua.RegisterGlobalFunction("_RSAS_IO_DeleteDirectory", DeleteDirectory);

                lua.RegisterGlobalFunction("_RSAS_IO_CreateDirectory", CreateDirectory);

                lua.RegisterGlobalFunction("_RSAS_IO_FileExists", FileExists);

                lua.RegisterGlobalFunction("_RSAS_IO_DirectoryExists", DirectoryExists);
            });
        }

        void Read(LuaManagedFunctionArgs args)
        {
            LuaString filePath = args.Input.ElementAtOrDefault(0) as LuaString;

            if (filePath == null)
                return;

            if (!System.IO.File.Exists(filePath.Value))
                return;

            string content = "";
            try
            {
                StreamReader sr = new StreamReader(filePath.Value);
                content = sr.ReadToEnd();
                sr.Close();
            }
            catch (IOException e)
            {
                throw new LuaRuntimeErrorException("File could not be read.", e);
            }

            args.Output.Add(new LuaString(content));
        }

        void Write(LuaManagedFunctionArgs args)
        {
            LuaString filePath = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaString content = args.Input.ElementAtOrDefault(1) as LuaString;
            LuaBoolean append = args.Input.ElementAtOrDefault(2) as LuaBoolean;

            if (filePath == null || content == null)
                return;

            if (append == null)
                append = new LuaBoolean(false);

            try
            {
                if (!Directory.Exists(Directory.GetParent(filePath.Value).FullName))
                    Directory.CreateDirectory(Directory.GetParent(filePath.Value).FullName);
            }
            catch (IOException e)
            {
                throw new LuaRuntimeErrorException("Implicit directory creation failed.", e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new LuaRuntimeErrorException("Implicit directory creation failed due to insufficient access privileges.", e);
            }

            try
            {
                StreamWriter sw = new StreamWriter(filePath.Value, append.Value);
                sw.Write(content);
                sw.Close();
            }
            catch (IOException e)
            {
                throw new LuaRuntimeErrorException("Writing failed.", e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new LuaRuntimeErrorException("Writing failed due to insufficient access privileges.", e);
            }
        }

        void DeleteFile(LuaManagedFunctionArgs args)
        {
            LuaString filePath = args.Input.ElementAtOrDefault(0) as LuaString;

            if (filePath == null)
                return;

            try
            {
                if (File.Exists(filePath.Value))
                    File.Delete(filePath.Value);
            }
            catch (IOException e)
            {
                throw new LuaRuntimeErrorException("File deletion failed.", e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new LuaRuntimeErrorException("File deletion failed due to insufficient access privileges.", e);
            }
        }

        void DeleteDirectory(LuaManagedFunctionArgs args)
        {
            LuaString dirPath = args.Input.ElementAtOrDefault(0) as LuaString;
            LuaBoolean recurse = args.Input.ElementAtOrDefault(1) as LuaBoolean;

            if (dirPath == null)
                return;

            if (recurse == null)
                recurse = new LuaBoolean(false);

            try
            {
                if (Directory.Exists(dirPath.Value))
                    Directory.Delete(dirPath.Value, recurse.Value);
            }
            catch (IOException e)
            {
                throw new LuaRuntimeErrorException("Directory deletion failed.", e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new LuaRuntimeErrorException("Directory deletion failed due to insufficient access privileges.", e);
            } 
        }

        void CreateDirectory(LuaManagedFunctionArgs args)
        {
            LuaString dirPath = args.Input.ElementAtOrDefault(0) as LuaString;

            if (dirPath == null)
                return;

            try
            {
                Directory.CreateDirectory(dirPath.Value);
            }
            catch (IOException e)
            {
                throw new LuaRuntimeErrorException("Directory creation failed.", e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new LuaRuntimeErrorException("Directory creation failed due to insufficient access privileges.", e);
            } 
        }

        void DirectoryExists(LuaManagedFunctionArgs args)
        {
            LuaString dirPath = args.Input.ElementAtOrDefault(0) as LuaString;

            if (dirPath == null)
                return;

            args.Output.Add(new LuaBoolean(Directory.Exists(dirPath.Value)));
        }

        void FileExists(LuaManagedFunctionArgs args)
        {
            LuaString filePath = args.Input.ElementAtOrDefault(0) as LuaString;

            if (filePath == null)
                return;

            args.Output.Add(new LuaBoolean(File.Exists(filePath.Value)));
        }
    }
}
