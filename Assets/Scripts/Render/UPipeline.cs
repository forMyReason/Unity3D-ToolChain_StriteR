﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Rendering
{
    public static class UPipeline
    {
        public static Material CreateMaterial(Type _type)
        {
            Shader _shader = Shader.Find("Hidden/" + _type.Name);

            if (_shader == null)
                throw new NullReferenceException("Invalid ImageEffect Shader Found:" + _type.Name);

            if (!_shader.isSupported)
                throw new NullReferenceException("Shader Not Supported:" + _type.Name);

            return new Material(_shader) { hideFlags = HideFlags.HideAndDontSave };
        }
        public static T GetDefaultPostProcessData<T>() where T : struct => (T)typeof(T).GetField("m_Default", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null);
        public static bool IsEnabled(this CameraOverrideOption _override,bool _default)=>_override == CameraOverrideOption.On || (_override == CameraOverrideOption.UsePipelineSettings && _default);
        public static Vector4 GetTexelSize(this RenderTextureDescriptor _descriptor) => new Vector4(1f/_descriptor.width,1f/_descriptor.height,_descriptor.width,_descriptor.height);
    }
}
