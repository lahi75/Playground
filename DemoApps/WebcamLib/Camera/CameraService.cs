using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace Phoebit.Vision.Camera
{
    public static class CameraService
    {
        private static Phoebit.WebcamGrabber.CameraMethods _cameraMethods;

        private static Phoebit.WebcamGrabber.CameraMethods CameraMethods
        {
            get
            {
                if (_cameraMethods == null)
                {
                    _cameraMethods = new Phoebit.WebcamGrabber.CameraMethods();
                }

                return _cameraMethods;
            }
        }

        [Export(ExportInterfaceNames.DefaultCamera)]
        public static Camera DefaultCamera
        {
            get { return AvailableCameras.FirstOrDefault(); }
        }

        private static List<Camera> _availableCameras;
        public static IEnumerable<Camera> AvailableCameras
        {
            get
            {
                if (_availableCameras == null)
                {
                    _availableCameras = BuildCameraList().ToList();
                }

                return _availableCameras;
            }
        }

        private static IEnumerable<Camera> BuildCameraList()
        {
            for (int i = 0; i < CameraMethods.Count; i++)
            {
                Phoebit.WebcamGrabber.CameraInfo cameraInfo = CameraMethods.GetCameraInfo(i);
                yield return new Camera(CameraMethods, cameraInfo.name, cameraInfo.index);
            }
        }
    }
}
