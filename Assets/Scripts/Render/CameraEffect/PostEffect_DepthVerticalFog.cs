﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public class PostEffect_DepthVerticalFog:PostEffectBase<CameraEffect_DepthVerticalFog>
    {
        public CameraEffectParam_DepthFog m_Param;
        protected override CameraEffect_DepthVerticalFog OnGenerateRequiredImageEffects() => new CameraEffect_DepthVerticalFog(()=>m_Param);
    }

    [System.Serializable]
    public class CameraEffectParam_DepthFog:ImageEffectParamBase
    {
        public Color m_FogColor;
        public float m_FogDensity = 1;
        public float m_FogVerticalStart = -2f;
        public float m_FogVerticalOffset = 2f;
        public Texture2D m_NoiseTexure;
        public float m_NoiseScale = 15f;
        public float m_NoiseSpeedX = .1f;
        public float m_NoiseSpeedY=.1f;
    }
    public class CameraEffect_DepthVerticalFog:ImageEffectBase<CameraEffectParam_DepthFog>
    {
        #region ShaderProeprties
        static readonly int ID_FogColor = Shader.PropertyToID("_FogColor");
        static readonly int ID_FogDensity = Shader.PropertyToID("_FogDensity");
        static readonly int ID_FogVerticalStart = Shader.PropertyToID("_FogVerticalStart");
        static readonly int ID_FogVerticalOffset = Shader.PropertyToID("_FogVerticalOffset");
        static readonly int ID_NoiseTexure = Shader.PropertyToID("_NoiseTex");
        static readonly int ID_NoiseScale = Shader.PropertyToID("_NoiseScale");
        static readonly int ID_NoiseSpeedX = Shader.PropertyToID("_NoiseSpeedX");
        static readonly int ID_NoiseSpeedY = Shader.PropertyToID("_NoiseSpeedY");
        #endregion
        public CameraEffect_DepthVerticalFog(Func<CameraEffectParam_DepthFog> _GetParam):base(_GetParam)
        {

        }
        protected override void OnValidate(CameraEffectParam_DepthFog _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetColor(ID_FogColor, _params.m_FogColor);
            _material.SetFloat(ID_FogDensity, _params.m_FogDensity);
            _material.SetFloat(ID_FogVerticalStart, _params.m_FogVerticalStart);
            _material.SetFloat(ID_FogVerticalOffset, _params.m_FogVerticalOffset);
            _material.SetTexture(ID_NoiseTexure, _params.m_NoiseTexure);
            _material.SetFloat(ID_NoiseScale, _params.m_NoiseScale);
            _material.SetFloat(ID_NoiseSpeedX, _params.m_NoiseSpeedX);
            _material.SetFloat(ID_NoiseSpeedY, _params.m_NoiseSpeedY);
        }
    }


}