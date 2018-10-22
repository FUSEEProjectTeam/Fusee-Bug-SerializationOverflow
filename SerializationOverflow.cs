using System;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace FuseeApp
{

    [FuseeApplication(Name = "SerializationOverflow", Description = "Yet another FUSEE App.")]
    public class SerializationOverflow : RenderCanvas
    {
         // Horizontal and vertical rotation Angles for the displayed object 
        private static float _angleHorz = M.PiOver4, _angleVert;
        
        // Horizontal and vertical angular speed
        private static float _angleVelHorz, _angleVelVert;

        // Overall speed factor. Change this to adjust how fast the rotation reacts to input
        private const float RotationSpeed = 7;

        // Damping factor 
        private const float Damping = 0.8f;

        private SceneContainer _rocketScene;
        private SceneRenderer _sceneRenderer;

        private ScenePicker _scenePicker;
        private PickResult _currentPick;
        private bool _pick;
        private float2 _pickPos;
        private float3 _oldColor;

        private float3 _LookAtDefaultPosition;
        private float3 _LookAtPosition;
        private float3 _LookAtPositionLerpFrom;
        private float3 _LookAtPositionLerpTo;
        private float3 _LookFromDefaultPosition;
        private float3 _LookFromPosition;
        private float3 _LookFromPositionLerpFrom;
        private float3 _LookFromPositionLerpTo;
        private float3 _LookUpDefault;

        private float _LerpTimer;
        private float _LerpSpeed;

        private float _DistanceFactor;

        private bool toggle = false;
        
        private bool _keys;

        // Init is called on startup. 
                public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
            RC.ClearColor = new float4(1, 1, 1, 1);

            // Load the model - Monkey.fus for flat shading, MonkeySmooth.fus for smooth shading
            _rocketScene = AssetStorage.Get<SceneContainer>("terrain.fus");

            // Wrap a SceneRenderer around the model.
            _sceneRenderer = new SceneRenderer(_rocketScene);
            _scenePicker = new ScenePicker(_rocketScene);
            
            _LookAtDefaultPosition = float3.Zero;
            _LookFromDefaultPosition = new float3(0, 1, -4);
            _LookUpDefault = float3.UnitY;

            _LookAtPositionLerpFrom = _LookAtDefaultPosition;
            _LookAtPositionLerpTo = _LookAtDefaultPosition;
            _LookFromPositionLerpFrom = _LookFromDefaultPosition;
            _LookFromPositionLerpTo = _LookFromDefaultPosition;

            _LerpTimer = 0;
            _LerpSpeed = 3;

            _DistanceFactor = 1.5f;
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Mouse and keyboard movement
            if (Mouse.LeftButton)
            {
                _pick = true;
                _pickPos = Mouse.Position;
            }
            else
            {
                _pick = false;
                toggle = false;
            }

            _DistanceFactor = _DistanceFactor + Mouse.WheelVel * 0.001f;

            _LookAtPosition = float3.Lerp( _LookAtPositionLerpTo, _LookAtPositionLerpFrom, _LerpTimer);
            _LookFromPosition = float3.Lerp( _LookFromPositionLerpTo, _LookFromPositionLerpFrom, _LerpTimer);

            var mtxCam = float4x4.LookAt(_LookFromPosition * _DistanceFactor, _LookAtPosition, _LookUpDefault);

            if (_pick && !toggle)
            {
                float2 pickPosClip = _pickPos * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);

                _scenePicker.View = mtxCam;
                
                PickResult newPick = _scenePicker.Pick(pickPosClip).ToList().OrderBy(pr => pr.ClipPos.z).FirstOrDefault();
  
          
                if (newPick != null)
                {
                    Diagnostics.Log("PositionCenter: " + GetTriagleCenter(newPick));
                    Diagnostics.Log("PositionBary: " + GetTriagleBarycentric(newPick));
                    Diagnostics.Log("NormalsCenter: " + GetNormalsCenter(newPick));
                    Diagnostics.Log("NormalBary: " + GetNormalsBarycentric(newPick));
                    _LookAtPositionLerpFrom = _LookAtPosition;
                    _LookAtPositionLerpTo = GetTriagleBarycentric(newPick);

                    _LookFromPositionLerpFrom = _LookFromPosition;
                    _LookFromPositionLerpTo = GetTriagleBarycentric(newPick) + GetNormalsBarycentric(newPick);

                    _LerpTimer = 1;

                    toggle = true;
                }
            }

            // Render the scene loaded in Init()
            RC.ModelView = mtxCam;
            _sceneRenderer.Render(RC);
            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();

            if (_LerpTimer > 0)
            {
                _LerpTimer -= (Time.DeltaTime / _LerpSpeed);
            }

            if (_LerpTimer < 0)
            {
                _LerpTimer = 0;
            }
        }

        private InputDevice Creator(IInputDeviceImp device)
        {
            throw new NotImplementedException();
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width/(float) Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
            _scenePicker.Projection = projection;
        }

        private void GetNormals(PickResult pr, out float3 a, out float3 b, out float3 c)
        {
            a = pr.Mesh.Normals[pr.Mesh.Triangles[pr.Triangle + 0]];
            b = pr.Mesh.Normals[pr.Mesh.Triangles[pr.Triangle + 1]];
            c = pr.Mesh.Normals[pr.Mesh.Triangles[pr.Triangle + 2]];
        }

        private float3 GetTriagleCenter(PickResult pr)
        {
            float3 a, b, c;
            pr.GetTriangle(out a, out b, out c);

            return (a + b + c) /3;
        }
        private float3 GetTriagleBarycentric(PickResult pr)
        {
            float3 a, b, c;
            pr.GetTriangle(out a, out b, out c);

            return float3.Barycentric(a, b, c, pr.U, pr.V);
        }

        private float3 GetNormalsCenter(PickResult pr)
        {
            float3 a, b, c;
            GetNormals(pr, out a, out b, out c);

            return (a + b + c) /3;
        }
        private float3 GetNormalsBarycentric(PickResult pr)
        {
            float3 a, b, c;
            GetNormals(pr, out a, out b, out c);

            return float3.Barycentric(a, b, c, pr.U, pr.V);
        }
    }
}