using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class InputAbstraction
    {
        public static bool isFake { get; set; }

        static Fake fake = new Fake();

        public static Fake GetFake() { return fake; }

        public static float GetAxis(string axisName)
        {
            if (isFake)
                return fake.GetAxis(axisName);

            else
                return Input.GetAxis(axisName);
        }
    }

    public class Fake
    {
        public float verticalAxis { get; set; }
        public float horizontalAxis { get; set; }

        public float GetAxis(string axisName)
        {
            switch (axisName)
            {
                case "Vertical": return verticalAxis;
                case "Horizontal": return horizontalAxis;
            }
            return 0f;
        }

        public void pressKey(string key)
        {
            switch (key)
            {
                case "w": verticalAxis = 1f; break;
                case "s": verticalAxis = -1f; break;
                case "d": horizontalAxis = 1f; break;
                case "a": horizontalAxis = -1f; break;
                default: throw new Exception($"Unknown key {key} in inputfaker");
            }
        }

        public void releaseKey(string key)
        {
            switch (key)
            {
                case "w":
                case "s":
                    verticalAxis = 0f;
                    break;
                case "d":
                case "a":
                    horizontalAxis = 0f;
                    break;
                default: throw new Exception($"Unknown key {key} in inputfaker");
            }
        }
    }
}