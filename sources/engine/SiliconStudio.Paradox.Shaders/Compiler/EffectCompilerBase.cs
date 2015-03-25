﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.IO;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// Base class for implementations of <see cref="IEffectCompiler"/>, providing some helper functions.
    /// </summary>
    public abstract class EffectCompilerBase : IEffectCompiler
    {
        protected EffectCompilerBase()
        {
        }

        /// <summary>
        /// Gets or sets the database file provider, to use for loading effects and shader sources.
        /// </summary>
        /// <value>
        /// The database file provider.
        /// </value>
        public abstract DatabaseFileProvider FileProvider { get; set; }

        public virtual ObjectId GetShaderSourceHash(string type)
        {
            return ObjectId.Empty;
        }

        /// <summary>
        /// Remove cached files for modified shaders
        /// </summary>
        /// <param name="modifiedShaders"></param>
        public virtual void ResetCache(HashSet<string> modifiedShaders)
        {
        }

        public CompilerResults Compile(ShaderSource shaderSource, CompilerParameters compilerParameters)
        {
            ShaderMixinSource mixinToCompile;
            var shaderMixinGeneratorSource = shaderSource as ShaderMixinGeneratorSource;

            if (shaderMixinGeneratorSource != null)
            {
                mixinToCompile = ShaderMixinManager.Generate(shaderMixinGeneratorSource.Name, compilerParameters);
            }
            else
            {
                mixinToCompile = shaderSource as ShaderMixinSource;
                var shaderClassSource = shaderSource as ShaderClassSource;

                if (shaderClassSource != null)
                {
                    mixinToCompile = new ShaderMixinSource { Name = shaderClassSource.ClassName };
                    mixinToCompile.Mixins.Add(shaderClassSource);
                }

                if (mixinToCompile == null)
                {
                    throw new ArgumentException("Unsupported ShaderSource type [{0}]. Supporting only ShaderMixinSource/pdxfx, ShaderClassSource", "shaderSource");
                }
                if (string.IsNullOrEmpty(mixinToCompile.Name))
                {
                    throw new ArgumentException("ShaderMixinSource must have a name", "shaderSource");
                }
            }

            // Copy global parameters to used Parameters by default, as it is used by the compiler
            mixinToCompile.UsedParameters.Set(CompilerParameters.GraphicsPlatformKey, compilerParameters.Platform);
            mixinToCompile.UsedParameters.Set(CompilerParameters.GraphicsProfileKey, compilerParameters.Profile);
            mixinToCompile.UsedParameters.Set(CompilerParameters.DebugKey, compilerParameters.Debug);

            // Compile the whole mixin tree
            var compilerResults = new CompilerResults { Module = string.Format("EffectCompile [{0}]", mixinToCompile.Name) };
            var bytecode = Compile(mixinToCompile, compilerParameters);

            // Since bytecode.Result is a struct, we check if any of its member has been set to know if it's valid
            if (bytecode.Result.CompilationLog != null || bytecode.Task != null)
            {
                if (bytecode.Result.CompilationLog != null)
                {
                    bytecode.Result.CompilationLog.CopyTo(compilerResults);
                }
                compilerResults.Bytecode = bytecode;
                compilerResults.UsedParameters = mixinToCompile.UsedParameters;
            }
            return compilerResults;
        }

        /// <summary>
        /// Compiles the ShaderMixinSource into a platform bytecode.
        /// </summary>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="compilerParameters">The compiler parameters.</param>
        /// <returns>The platform-dependent bytecode.</returns>
        public abstract TaskOrResult<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, CompilerParameters compilerParameters);

        public static readonly string DefaultSourceShaderFolder = "shaders";

        public static string GetStoragePathFromShaderType(string type)
        {
            if (type == null) throw new ArgumentNullException("type");
            // TODO: harcoded values, bad bad bad
            return DefaultSourceShaderFolder + "/" + type + ".pdxsl";
        }
    }
}