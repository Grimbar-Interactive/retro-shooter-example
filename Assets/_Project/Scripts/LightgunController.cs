using System;
using System.IO.Ports;
using System.Threading;
using GI.UnityToolkit.Utilities;
using UnityEngine;

public class LightgunController : MonoBehaviour
{
    private static class Commands
    {
        /// <summary>
        /// Starts external control mode.
        /// </summary>
        public const string ENTER_EXTERNAL_CONTROL_MODE = "ZS";
        
        /// <summary>
        /// Returns lightgun to default self-control mode.
        /// </summary>
        public const string EXIT_EXTERNAL_CONTROL_MODE = "ZX";

        /// <summary>
        /// Enables LED auto control mode.
        /// </summary>
        public const string LED_AUTO_CONTROL_MODE = "ZR";
        
        /// <summary>
        /// Slide returns to standard position; 5 orange LED.
        /// </summary>
        public const string SLIDE_RETURN = "Z6";
        
        /// <summary>
        /// Rotates rumble motor once.
        /// </summary>
        public const string RUMBLE = "ZZ";

        /// <summary>
        /// Aspect ratio commands.
        /// </summary>
        public static class ScreenMode
        {
            /// <summary>
            /// 16:9 aspect ratio
            /// </summary>
            public const string WIDESCREEN = "ZW";

            /// <summary>
            /// 4:3 aspect ratio
            /// </summary>
            public const string STANDARD = "ZN";
        }

        /// <summary>
        /// Control mode commands.
        /// </summary>
        public static class ControlMode
        {
            /// <summary>
            /// Joystick control mode
            /// </summary>
            public const string JOYSTICK = "ZJ";
            
            /// <summary>
            /// Keyboard + mouse control mode
            /// </summary>
            public const string KEYBOARD_MOUSE = "ZM";
        }
        

        /// <summary>
        /// Returns "Z0" - "Z5" based on remaining ammo.
        /// </summary>
        /// <param name="count">Remaining ammo count</param>
        /// <returns>A string command to cause recoil and set LEDs ("Z0" - "Z5").</returns>
        public static string GetBulletCommandFromRemainingAmmoCount(int count)
        {
            return $"Z{Mathf.Clamp(count, 0, 5):N0}";
        }

        /// <summary>
        /// Returns "Z0" - "Z5" based on remaining ammo percent.
        /// </summary>
        /// <param name="percent">Remaining ammo percent</param>
        /// <returns>A string command to cause recoil and set LEDs ("Z0" - "Z5").</returns>
        public static string GetBulletCommandFromRemainingAmmoPercent(float percent)
        {
            percent = Mathf.Clamp01(percent);
            return percent == 0 ? "Z0" : GetBulletCommandFromRemainingAmmoCount(Mathf.CeilToInt(percent * 5f));
        }
    }
    
    [SerializeField] private bool logDebugMessages = false;
    
    private const string PortName = "COM4";
    private const int BaudRate = 115200;
    
    private SerialPort _serialPort;
    private Thread _readThread;

    private void OnEnable()
    {
        ConnectToLightgun();
        _serialPort.Write(Commands.ENTER_EXTERNAL_CONTROL_MODE);
    }

    private void OnDisable()
    {
        DisconnectFromLightgun();
    }

    private void ConnectToLightgun()
    {
        _serialPort ??= new SerialPort(PortName, BaudRate);

        if (_serialPort.IsOpen)
        {
            this.LogWarning("Attempted to connect to lightgun when connection is already established!");
            return;
        }

        try
        {
            _serialPort.Open();
            if (logDebugMessages) this.Log("Serial port opened.");
            _readThread = new Thread(ReadData);
            _readThread.Start();
        }
        catch (Exception e)
        {
            this.LogError($"Error opening serial port: {e.Message}");
        }

        return;

        void ReadData()
        {
            while (_serialPort.IsOpen)
            {
                if (_serialPort.BytesToRead <= 0) continue;
                var message = _serialPort.ReadLine();
                this.Log($"Message received from serial port: {message}");
            }    
        }
    }

    private void DisconnectFromLightgun()
    {
        if (_serialPort.IsOpen == false) return;
        _serialPort.Write(Commands.EXIT_EXTERNAL_CONTROL_MODE);
        _serialPort.Close();
        if (_readThread.IsAlive) _readThread.Join();
        if (logDebugMessages) this.Log("Serial port closed.");
    }

    public void Fire(int remainingAmmo)
    {
        if (_serialPort.IsOpen == false) return;
        _serialPort.Write(Commands.GetBulletCommandFromRemainingAmmoCount(remainingAmmo));
    }

    public void Fire(float remainingAmmoPercent)
    {
        if (_serialPort.IsOpen == false) return;
        _serialPort.Write(Commands.GetBulletCommandFromRemainingAmmoPercent(remainingAmmoPercent));
    }

    public void Reload()
    {
        if (_serialPort.IsOpen == false) return;
        _serialPort.Write(Commands.SLIDE_RETURN);
    }

    public void Rumble()
    {
        if (_serialPort.IsOpen == false) return;
        _serialPort.Write(Commands.RUMBLE);
    }
}
