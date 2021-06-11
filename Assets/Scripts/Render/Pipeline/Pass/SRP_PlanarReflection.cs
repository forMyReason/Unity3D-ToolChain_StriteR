using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using System.Collections.Generic;
    using ImageEffect;
    public class SRP_PlanarReflection : ScriptableRenderPass,ISRPBase
    {
        #region ID
        public const int C_MaxReflectionTextureCount = 4;
        const string C_ReflectionTex = "_CameraReflectionTexture";

        static readonly int ID_ReflectionTextureOn = Shader.PropertyToID("_CameraReflectionTextureOn");
        static readonly int ID_ReflectionTextureIndex = Shader.PropertyToID("_CameraReflectionTextureIndex");
        static readonly int ID_ReflectionNormalDistort = Shader.PropertyToID("_CameraReflectionNormalDistort");

        static readonly int ID_ReflectionDepth = Shader.PropertyToID("_CameraReflectionDepthComaparer");
        static readonly RenderTargetIdentifier RT_ID_ReflectionDepth = new RenderTargetIdentifier(ID_ReflectionDepth);

        static readonly int ID_ReflectionTempTexture = Shader.PropertyToID("_CameraReflectionTemp");
        static readonly RenderTargetIdentifier RT_ID_ReflectionTempTexture = new RenderTargetIdentifier(ID_ReflectionTempTexture);

        static readonly int ID_SampleCount = Shader.PropertyToID( "_SAMPLE_COUNT");
        static readonly int ID_Result_TexelSize = Shader.PropertyToID("_Result_TexelSize");
        static readonly int ID_PlaneNormal = Shader.PropertyToID("_PlaneNormal");
        static readonly int ID_PlanePosition = Shader.PropertyToID("_PlanePosition");

        static readonly int ID_Input = Shader.PropertyToID("_Input");
        static readonly int ID_Depth = Shader.PropertyToID("_Depth");
        static readonly int ID_Result = Shader.PropertyToID("_Result");
        #endregion

        ScriptableRenderer m_Renderer;
        SRD_ReflectionPlane m_Plane;
        ComputeShader m_ComputeShader;
        RenderTextureDescriptor m_ResultDescriptor;
        ImageEffect_Blurs m_Blur;
        Int3 m_Kernels;

        public List<ShaderTagId> m_ShaderTagIDs = new List<ShaderTagId>();
        FilteringSettings m_FilterSettings;
        MaterialPropertyBlock m_PropertyBlock;
        int m_ReflectionTexture;
        RenderTargetIdentifier m_ReflectionTextureID;
        public SRP_PlanarReflection()
        {
            m_Blur = new ImageEffect_Blurs();
            m_ShaderTagIDs.FillWithDefaultTags();
            m_FilterSettings = new FilteringSettings(RenderQueueRange.opaque);
            m_PropertyBlock = new MaterialPropertyBlock();
        }
        public void Dispose()
        {
            m_Blur.Destroy();
        }
        public SRP_PlanarReflection Setup(int _index,ScriptableRenderer _renderer, ComputeShader _shader, SRD_ReflectionPlane _plane,bool _lowEnd)
        {
            m_Renderer = _renderer;
            m_Plane = _plane;
            m_ComputeShader = _shader;
            string keyword = _lowEnd ? "Low" : "Medium";
            int groupCount = _lowEnd ? 1 : 8;
            m_Kernels = new Int3(m_ComputeShader.FindKernel("Clear" + keyword),m_ComputeShader.FindKernel("Generate"+ keyword), groupCount);
            m_Blur.OnValidate(_plane.m_BlurParam);
            m_PropertyBlock.SetInt(ID_ReflectionTextureOn, 1);
            m_PropertyBlock.SetInt(ID_ReflectionTextureIndex,_index);
            m_PropertyBlock.SetFloat(ID_ReflectionNormalDistort, m_Plane.m_NormalDistort);
            m_Plane.m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
            m_ReflectionTexture = Shader.PropertyToID( C_ReflectionTex + _index.ToString());
            m_ReflectionTextureID = new RenderTargetIdentifier(m_ReflectionTexture);
            return this;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            m_ResultDescriptor = new RenderTextureDescriptor(cameraTextureDescriptor.width/m_Plane.m_DownSample, cameraTextureDescriptor.height/m_Plane.m_DownSample, RenderTextureFormat.ARGB32, -1) { enableRandomWrite = true };
            cmd.GetTemporaryRT(m_ReflectionTexture, m_ResultDescriptor,FilterMode.Bilinear);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Reflection Texture");
            GPlane planeData = m_Plane.m_PlaneData;
            RenderTargetIdentifier colorResult = m_ReflectionTextureID;

            if (m_Plane.m_EnableBlur)
            {
                cmd.GetTemporaryRT(ID_ReflectionTempTexture, m_ResultDescriptor, FilterMode.Bilinear);
                colorResult = RT_ID_ReflectionTempTexture;
            }

            switch (m_Plane.m_ReflectionType)
            {
                case enum_ReflectionSpace.ScreenSpace:
                    {
                        cmd.GetTemporaryRT(ID_ReflectionDepth, m_ResultDescriptor);
                        cmd.SetRandomWriteTarget(0,colorResult);
                        cmd.SetRenderTarget(colorResult);
                        cmd.SetComputeIntParam(m_ComputeShader, ID_SampleCount, m_Plane.m_Sample);
                        cmd.SetComputeVectorParam(m_ComputeShader, ID_PlaneNormal, planeData.normal.normalized);
                        cmd.SetComputeVectorParam(m_ComputeShader, ID_PlanePosition, planeData.distance * planeData.normal);
                        cmd.SetComputeVectorParam(m_ComputeShader, ID_Result_TexelSize, m_ResultDescriptor.GetTexelSize());

                        int groupX = m_ResultDescriptor.width / m_Kernels.m_Z;
                        int groupY = m_ResultDescriptor.height / m_Kernels.m_Z;

                        cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_X, ID_Depth, RT_ID_ReflectionDepth);
                        cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_X, ID_Result, colorResult);
                        cmd.DispatchCompute(m_ComputeShader, m_Kernels.m_X, groupX, groupY, 1);

                        cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_Y, ID_Input, m_Renderer.cameraColorTarget);
                        cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_Y, ID_Depth, RT_ID_ReflectionDepth);
                        cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_Y, ID_Result, colorResult);
                        cmd.DispatchCompute(m_ComputeShader, m_Kernels.m_Y, groupX, groupY, 1);
                    }
                    break;
                case enum_ReflectionSpace.MirrorSpace:
                    {
                        RenderTextureDescriptor depthDescriptor = new RenderTextureDescriptor(m_ResultDescriptor.width, m_ResultDescriptor.height, RenderTextureFormat.Depth, 16, 0);
                        cmd.GetTemporaryRT(ID_ReflectionDepth, depthDescriptor, FilterMode.Point);
                        cmd.SetRenderTarget(colorResult, RT_ID_ReflectionDepth);
                        cmd.ClearRenderTarget(true, true, Color.black.SetAlpha(0));
                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();

                        CameraData cameraData = renderingData.cameraData;
                        Camera camera = cameraData.camera;
                        
                        Matrix4x4 planeMirroMatrix = planeData.GetMirrorMatrix();
                        Matrix4x4 cullingMatrix = camera.cullingMatrix;
                        camera.cullingMatrix = cullingMatrix * planeMirroMatrix;
                        if (  cameraData.camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
                        {
                            DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIDs, ref renderingData, SortingCriteria.CommonOpaque);
                            Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(), cameraData.IsCameraProjectionMatrixFlipped());
                            Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
                            viewMatrix*= planeMirroMatrix;

                            RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMatrix , projectionMatrix, false);
                            cmd.SetInvertCulling(true);
                            context.ExecuteCommandBuffer(cmd);

                            CullingResults cullResults = context.Cull(ref cullingParameters);
                            context.DrawRenderers(cullResults, ref drawingSettings, ref m_FilterSettings);

                            cmd.Clear();
                            cmd.SetInvertCulling(false);
                            RenderingUtils.SetViewAndProjectionMatrices(cmd, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), false);
                        }
                        camera.ResetCullingMatrix();
                    }
                    break;
            }
            if (m_Plane.m_EnableBlur)
            {
                m_Blur.ExecutePostProcessBuffer(cmd, colorResult, m_ReflectionTextureID, m_ResultDescriptor ,m_Plane.m_BlurParam); 
                cmd.ReleaseTemporaryRT(ID_ReflectionTempTexture);
            }
            cmd.ReleaseTemporaryRT(ID_ReflectionDepth);
            cmd.SetRenderTarget(m_Renderer.cameraColorTarget);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(m_ReflectionTexture);
        }
    }
}