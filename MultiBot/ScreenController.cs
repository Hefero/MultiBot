using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enigma.D3.MemoryModel.Core;

namespace EnvControllers
{
    public static class ScreenController
    {
        public static void ToScreenCoordinate(ACD target, ACD myself, int screenWidth, int screenHeight, out float RX, out float RY)
        {
            try
            {
                float X = target.Position.X;
                float Y = target.Position.Y;
                float Z = target.Position.Z;


                float xd = X - myself.Position.X;
                float yd = Y - myself.Position.Y;
                float zd = Z - myself.Position.Z;

                /*
                float w = -0.515f * xd - 0.514f * yd - 0.686f * zd + 97.985f;
                if (w < 1.0f) w = 1.0f;
                float rX = (-1.682f * xd + 1.683f * yd + 0.007045f) / w;
                float rY = (-1.540f * xd - 1.539f * yd + 2.307f * zd + 6.161f) / w;
                */

                float w = -0.515f * xd + -0.514f * yd + -0.686f * zd + 97.985f;
                float rX = (-1.682f * xd + 1.683f * yd + 0.0f * zd + 7.045e-3f) / w;
                float rY = (-1.54f * xd + -1.539f * yd + 2.307f * zd + 6.161f) / w;


                //float a = (float)Engine.Current.VideoPreferences.x0C_DisplayMode.x20_Width / (float)Engine.Current.VideoPreferences.x0C_DisplayMode.x24_Height;
                //float D3ClientWindowApect = a * 600.0f / 800.0f;


                //int video_width = Engine.Current.VideoPreferences.x0C_DisplayMode.x20_Width;
                //int video_height = Engine.Current.VideoPreferences.x0C_DisplayMode.x24_Height;

                int video_width = screenWidth;
                int video_height = screenHeight;


                double D3ClientWindowApect = (double)((double)video_width / (double)video_height) / (double)(4.0f / 3.0f); // 4:3 = default aspect ratio



                rX /= (float)D3ClientWindowApect;

                RX = (rX + 1.0f) / 2.0f * video_width;
                RY = (1.0f - rY) / 2.0f * video_height;
            }
            catch { RX = 0; RY = 0; }

        }
    }
}
