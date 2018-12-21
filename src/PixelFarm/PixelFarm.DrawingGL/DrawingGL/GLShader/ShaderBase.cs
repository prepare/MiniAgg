﻿//MIT, 2016-present, WinterDev

using System.IO;
using OpenTK.Graphics.ES20;
namespace PixelFarm.DrawingGL
{
    abstract class ShaderBase
    {
        protected readonly ShaderSharedResource _shareRes;
        protected readonly MiniShaderProgram _shaderProgram;
        public ShaderBase(ShaderSharedResource shareRes)
        {
            _shareRes = shareRes;
            _shaderProgram = new MiniShaderProgram();

            EnableProgramBinaryCache = true; //default ???
        }
        /// <summary>
        /// set as current shader
        /// </summary>
        internal void SetCurrent()
        {
            if (_shareRes._currentShader != this)
            {
                _shareRes._currentShader = this;
                _shaderProgram.UseProgram();

                this.OnSwitchToThisShader();
            }
        }
        protected virtual void OnSwitchToThisShader()
        {
        }


        //--------------------------------------------------------
        public bool EnableProgramBinaryCache { get; set; }
        protected bool SaveCompiledShader()
        {
#if DEBUG
            //System.Type type = this.GetType();
            //File.AppendAllText("compiled_shader_logs.txt", type.FullName + " " + type.GUID + "\r\n");
#endif
            return SaveCompiledShader(this.GetType().GUID.ToString() + ".bin_shader");
        }
        protected bool LoadCompiledShader()
        {
            return LoadCompiledShader(this.GetType().GUID.ToString() + ".bin_shader");
        }
        protected bool SaveCompiledShader(string filename)
        {
            if (!EnableProgramBinaryCache) return false;
            //--------------------------------------------

            using (System.IO.Stream s = CachedBinaryShaderIO.InternalGetWriteStream(filename))
            {
                if (s != null)
                {
                    using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(s))
                    {
                        return _shaderProgram.SaveCompiledShader(w);
                    }
                }
            }
            return false;

        }
        protected bool LoadCompiledShader(string filename)
        {
            if (!EnableProgramBinaryCache) return false;
            //--------------------------------------------
            using (System.IO.Stream s = CachedBinaryShaderIO.InternalGetReadStream(filename))
            {
                if (s != null)
                {
                    using (System.IO.BinaryReader r = new System.IO.BinaryReader(s))
                    {
                        return _shaderProgram.LoadCompiledShader(r);
                    }
                }
            }
            return false;
        }
    }


    public abstract class CachedBinaryShaderIO
    {
        static CachedBinaryShaderIO s_defaultLocalFile = new LocalFileCachedBinaryShaderIO("");

        static CachedBinaryShaderIO s_currentImpl = s_defaultLocalFile;

        public static void ClearCurrentImpl()
        {
            s_currentImpl = null;
        }
        public void SetAsCurrentImplementation()
        {
            s_currentImpl = this;
        }

        public abstract Stream GetReadStream(string shaderName);
        public abstract Stream GetWriteStream(string shaderName);
        //--------

        internal static CachedBinaryShaderIO GetBinCacheIO() => s_currentImpl;

        internal static Stream InternalGetReadStream(string shaderName) => s_currentImpl.GetReadStream(shaderName);

        internal static Stream InternalGetWriteStream(string shaderName) => s_currentImpl.GetWriteStream(shaderName);

    }


    //example
    public class LocalFileCachedBinaryShaderIO : CachedBinaryShaderIO
    {
        public LocalFileCachedBinaryShaderIO(string baseDir, bool enabledAbsolutePath = false)
        {
            BaseDir = baseDir;
        }
        public string BaseDir { get; }
        public bool EnableAbsolutePath { get; }
        //
        public override Stream GetReadStream(string shaderName)
        {
            if (Path.IsPathRooted(shaderName))
            {
                if (!EnableAbsolutePath) return null;
            }
            else
            {
                shaderName = Path.Combine(BaseDir, shaderName);
            }

            if (File.Exists(shaderName))
            {
                //read from file,
                //don't forget to delete this stream after use.  
                return new FileStream(shaderName, FileMode.Open);
            }
            return null;
        }
        public override Stream GetWriteStream(string shaderName)
        {
            if (Path.IsPathRooted(shaderName))
            {
                if (!EnableAbsolutePath) return null;
            }
            else
            {
                shaderName = Path.Combine(BaseDir, shaderName);
            }
            //write to
            return new FileStream(shaderName, FileMode.Create); //***
        }
    }


}