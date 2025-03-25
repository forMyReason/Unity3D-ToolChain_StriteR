﻿using System;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController.Inputs.Touch
{
    [Serializable]
    public struct FPlayerInputMultiplier
    {
        [Range(-2,2)] public float kPitchMultiplier;
        [Range(-2,2)] public float kYawMultiplier;
        [Range(-2,2)] public float kPinchMultiplier;
        public static FPlayerInputMultiplier kDefaultPixels = new() { kPitchMultiplier = -180f / 1000f, kYawMultiplier = 180f / 1000f, kPinchMultiplier = 1f / 1000, };
        public static FPlayerInputMultiplier kOne = new() { kPitchMultiplier = 1f, kYawMultiplier = 1f, kPinchMultiplier = 1f, };
        public static FPlayerInputMultiplier operator *(FPlayerInputMultiplier _a, FPlayerInputMultiplier _b) => new() {
                kPitchMultiplier = _a.kPitchMultiplier * _b.kPitchMultiplier,
                kYawMultiplier = _a.kYawMultiplier * _b.kYawMultiplier,
                kPinchMultiplier = _a.kPinchMultiplier * _b.kPinchMultiplier,
            };
    }
    
    public interface IControllerPlayerTouchInput : IPlayerInput
    {
        public FPlayerInputMultiplier Sensitive { get; }
        public float2 PlayerDrag { get; set; }
        public float PlayerPinch { get; set; }

        public void TouchInputClear()
        {
            this.PlayerInputClear();
            PlayerDrag = 0;
            PlayerPinch = 0;
        }
    }
}