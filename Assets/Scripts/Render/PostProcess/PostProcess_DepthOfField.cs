﻿using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.PostProcess
{
    public class PostProcess_DepthOfField : PostProcessComponentBase<PPCore_DepthOfField, PPData_DepthOfField>
    {
        public override bool m_OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.Default;
    }

    [Serializable]
    public struct PPData_DepthOfField 
    {
        [Range(0.01f, 1f)] public float m_DOFStart;
        [Range(.01f, .3f)] public float m_DOFLerp;
        public PPData_Blurs m_BlurParams;
        [MTitle] public bool m_DepthBlurSample;
        [MFoldout(nameof(m_DepthBlurSample),true), Range(.25f, 1.25f)] public float m_DepthBlurSize;
        public static readonly PPData_DepthOfField m_Default = new PPData_DepthOfField()
        {
            m_DOFStart = 0.1f,
            m_DOFLerp = .1f,
            m_BlurParams = PPData_Blurs.m_Default,
            m_DepthBlurSample = true,
            m_DepthBlurSize = .5f,
        };
}
    public class PPCore_DepthOfField : PostProcessCore<PPData_DepthOfField>
    {
        #region ShaderID
        static readonly int RT_ID_Blur = Shader.PropertyToID("_BlurTex");
        static readonly RenderTargetIdentifier RT_Blur = new RenderTargetIdentifier(RT_ID_Blur);

        static readonly int ID_FocalStart = Shader.PropertyToID("_FocalStart");
        static readonly int ID_FocalLerp = Shader.PropertyToID("_FocalLerp");
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        const string KW_UseBlurDepth = "_UseBlurDepth";
        #endregion
        PPCore_Blurs m_CoreBlur;
        public PPCore_DepthOfField():base() 
        {
            m_CoreBlur = new PPCore_Blurs();
        }
        public override void Destroy()
        {
            base.Destroy();
            m_CoreBlur.Destroy();
        }
        public override void OnValidate(ref PPData_DepthOfField _ssaoData)
        {
            base.OnValidate(ref _ssaoData);
            m_Material.SetFloat(ID_FocalStart, _ssaoData.m_DOFStart);
            m_Material.SetFloat(ID_FocalLerp, _ssaoData.m_DOFLerp);
            m_Material.EnableKeyword(KW_UseBlurDepth, _ssaoData.m_DepthBlurSample);
            m_Material.SetFloat(ID_BlurSize, _ssaoData.m_DepthBlurSize);
            m_CoreBlur.OnValidate(ref _ssaoData.m_BlurParams);
        }
        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor, ref PPData_DepthOfField _data)
        {
            base.ExecutePostProcessBuffer(_buffer, _src, _dst, _descriptor,ref  _data);
            _buffer.GetTemporaryRT(RT_ID_Blur, _descriptor.width, _descriptor.height, 0, FilterMode.Bilinear, _descriptor.colorFormat);
            m_CoreBlur.ExecutePostProcessBuffer(_buffer, _src, RT_ID_Blur,_descriptor,ref  _data.m_BlurParams);
            _buffer.Blit(_src, _dst, m_Material);
            _buffer.ReleaseTemporaryRT(RT_ID_Blur);
        }

    }
}