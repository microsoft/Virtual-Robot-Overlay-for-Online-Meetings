using System;
using System.Numerics;
using WindowsInput;

namespace VROOM_RobotController
{
    class Program
    {
        static InputSimulator input = new InputSimulator();
        static NetworkEvents networkEvents;

        static float walkingThreshold = 0.8f;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            networkEvents = new NetworkEvents("RobotController", "vr", "http://10.190.102.112:3000/", false, 5);
            networkEvents.AddHandler("ThumbstickPositionUpdate", thumbstickPositionUpdate);

            while (true)
            {
                ///
            }
        }

        static void thumbstickPositionUpdate(string data)
        {
            Console.WriteLine(data);

            Vector3 thumbstickPos = eventStringToVector3(data);

            if (thumbstickPos.X < -walkingThreshold)
            {
                input.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.LEFT);
            }
            else
            {
                input.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.LEFT);
            }

            if (thumbstickPos.X > walkingThreshold)
            {
                input.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.RIGHT);
            }
            else
            {
                input.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.RIGHT);
            }

            if (thumbstickPos.Y < -walkingThreshold)
            {
                input.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.DOWN);
            }
            else
            {
                input.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.DOWN);
            }

            if (thumbstickPos.Y > walkingThreshold)
            {
                input.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.UP);
            }
            else
            {
                input.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.UP);
            }
        }

        static Vector3 eventStringToVector3(string s)
        {
            string[] axis = s.Split(',');
            return new Vector3(float.Parse(axis[0]), float.Parse(axis[1]), float.Parse(axis[2]));
        }
    }
}
