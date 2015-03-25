﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Paradox Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Paradox.VisualStudio.Package .vsix
// and re-save the associated .pdxfx.
// </auto-generated>

using System;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Materials;
namespace SiliconStudio.Paradox.Effects
{
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxEffectBase  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSource mixin, ShaderMixinContext context)
            {
                context.Mixin(mixin, "ShaderBase");
                context.Mixin(mixin, "ShadingBase");
                if (context.GetParam(MaterialKeys.VertexStageSurfaceShaders) != null)
                {
                    context.Mixin(mixin, "MaterialSurfaceVertexStageCompositor");

                    {
                        var __subMixin = new ShaderMixinSource();
                        context.PushComposition(mixin, "materialVertexStage", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialKeys.VertexStageSurfaceShaders));
                        context.PopComposition();
                    }

                    {
                        var __subMixin = new ShaderMixinSource();
                        context.PushComposition(mixin, "streamInitializerVertexStage", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialKeys.VertexStageStreamInitializer));
                        context.PopComposition();
                    }
                }
                context.Mixin(mixin, "TransformationBase");
                context.Mixin(mixin, "NormalStream");
                context.Mixin(mixin, "TransformationWAndVP");
                if (context.GetParam(MaterialKeys.HasNormalMap))
                {
                    context.Mixin(mixin, "NormalFromNormalMapping");
                }
                else
                {
                    context.Mixin(mixin, "NormalFromMesh");
                }
                if (context.GetParam(MaterialKeys.HasSkinningPosition))
                {
                    if (context.GetParam(MaterialKeys.SkinningBones) > context.GetParam(MaterialKeys.SkinningMaxBones))
                    {
                        context.SetParam(MaterialKeys.SkinningMaxBones, context.GetParam(MaterialKeys.SkinningBones));
                    }
                    mixin.AddMacro("SkinningMaxBones", context.GetParam(MaterialKeys.SkinningMaxBones));
                    context.Mixin(mixin, "TransformationSkinning");
                    if (context.GetParam(MaterialKeys.HasSkinningNormal))
                    {
                        context.Mixin(mixin, "NormalMeshSkinning");
                    }
                    if (context.GetParam(MaterialKeys.HasSkinningTangent))
                    {
                        context.Mixin(mixin, "TangentMeshSkinning");
                    }
                    if (context.GetParam(MaterialKeys.HasSkinningNormal))
                    {
                        if (context.GetParam(MaterialKeys.HasNormalMap))
                        {
                            context.Mixin(mixin, "NormalVSSkinningNormalMapping");
                        }
                        else
                        {
                            context.Mixin(mixin, "NormalVSSkinningFromMesh");
                        }
                    }
                }
                if (context.GetParam(MaterialKeys.TessellationShader) != null)
                {
                    context.Mixin(mixin, context.GetParam(MaterialKeys.TessellationShader));
                    if (context.GetParam(MaterialKeys.DomainStageSurfaceShaders) != null)
                    {
                        context.Mixin(mixin, "MaterialSurfaceDomainStageCompositor");

                        {
                            var __subMixin = new ShaderMixinSource();
                            context.PushComposition(mixin, "materialDomainStage", __subMixin);
                            context.Mixin(__subMixin, context.GetParam(MaterialKeys.DomainStageSurfaceShaders));
                            context.PopComposition();
                        }

                        {
                            var __subMixin = new ShaderMixinSource();
                            context.PushComposition(mixin, "streamInitializerDomainStage", __subMixin);
                            context.Mixin(__subMixin, context.GetParam(MaterialKeys.DomainStageStreamInitializer));
                            context.PopComposition();
                        }
                    }
                }
                var extensionPostVertexStage = context.GetParam(ParadoxEffectBaseKeys.ExtensionPostVertexStageShader);
                if (extensionPostVertexStage != null)
                {
                    context.Mixin(mixin, (extensionPostVertexStage));
                    return;
                }
                if (context.GetParam(MaterialKeys.PixelStageSurfaceShaders) != null)
                {
                    context.Mixin(mixin, "MaterialSurfacePixelStageCompositor");

                    {
                        var __subMixin = new ShaderMixinSource();
                        context.PushComposition(mixin, "materialPixelStage", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialKeys.PixelStageSurfaceShaders));
                        context.PopComposition();
                    }

                    {
                        var __subMixin = new ShaderMixinSource();
                        context.PushComposition(mixin, "streamInitializerPixelStage", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialKeys.PixelStageStreamInitializer));
                        context.PopComposition();
                    }
                    if (context.GetParam(MaterialKeys.PixelStageSurfaceFilter) != null)
                    {
                        context.Mixin(mixin, context.GetParam(MaterialKeys.PixelStageSurfaceFilter));
                    }
                }
                var directLightGroups = context.GetParam(LightingKeys.DirectLightGroups);
                if (directLightGroups != null)
                {
                    foreach(var directLightGroup in directLightGroups)

                    {

                        {
                            var __subMixin = new ShaderMixinSource();
                            context.PushCompositionArray(mixin, "directLightGroups", __subMixin);
                            context.Mixin(__subMixin, (directLightGroup));
                            context.PopComposition();
                        }
                    }
                }
                var environmentLights = context.GetParam(LightingKeys.EnvironmentLights);
                if (environmentLights != null)
                {
                    foreach(var environmentLight in environmentLights)

                    {

                        {
                            var __subMixin = new ShaderMixinSource();
                            context.PushCompositionArray(mixin, "environmentLights", __subMixin);
                            context.Mixin(__subMixin, (environmentLight));
                            context.PopComposition();
                        }
                    }
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxEffectBase", new ParadoxEffectBase());
            }
        }
    }
}
