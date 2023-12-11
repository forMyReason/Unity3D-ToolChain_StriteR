﻿using System;
using System.ComponentModel;
using UnityEngine;

namespace UnityEditor.Extensions.TextureEditor
{
    public enum EChannelOperation
    {
        Constant = 0,
        R = 10, ROneMinus,
        G = 20, GOneMinus,
        B = 30, BOneMinus,
        A = 40, AOneMinus,
        LightmapToLuminance,
    }
    
    [Serializable]
    public struct ChannelCollector
    {
        public EChannelOperation operation;
        [MFoldout(nameof(operation), EChannelOperation.Constant)] [Range(0, 1)] public float constantValue;
        [MFold(nameof(operation),EChannelOperation.Constant)] public Texture2D texture;

        public bool Valid => operation == EChannelOperation.Constant || (texture != null && texture.isReadable);
        
        private Color[] pixels;
        public void Prepare()
        {
            if (operation == EChannelOperation.Constant)
                return;
            pixels = texture.GetPixels();
        }
        
        public float Collect(int _index)
        {
            if (operation == EChannelOperation.Constant)
                return constantValue;

            var color = pixels[_index];
            return operation switch
            {
                EChannelOperation.R => color.r, EChannelOperation.ROneMinus => 1 - color.r,
                EChannelOperation.G => color.g, EChannelOperation.GOneMinus => 1 - color.g,
                EChannelOperation.B => color.b, EChannelOperation.BOneMinus => 1 - color.b,
                EChannelOperation.A => color.a, EChannelOperation.AOneMinus => 1 - color.a,
                EChannelOperation.LightmapToLuminance => UColorTransform.RGBtoLuminance(color.to3()),
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public void End()
        {
            pixels = null;
        }
        
        public static readonly ChannelCollector kDefault = new ChannelCollector()
        {
            texture = null,
            operation = EChannelOperation.R,
        };
    }

}