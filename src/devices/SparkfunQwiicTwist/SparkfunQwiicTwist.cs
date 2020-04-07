// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.SparkfunQwiicTwist
{
    /// <summary>
    /// Sparkfun's Qwiic button module
    /// </summary>
    public class SparkfunQwiicTwist : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Device Id
        /// </summary>
        public const byte DeviceId = 0x5C;

        /// <summary>
        /// Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x3F;

        /// <summary>
        /// Creates a new instance of the SparkfunQwiicTwist
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public SparkfunQwiicTwist(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;

            if (!IsValidDeviceId())
            {
                throw new Exception("Unable to identify manufacturer/device id");
            }
        }

        /// <summary>
        /// Checks if the device is a Sparkfun Qwiic button
        /// </summary>
        /// <returns>True if device has been correctly detected</returns>
        private bool IsValidDeviceId()
        {
            return GetDeviceId() == DeviceId;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        /// <summary>
        /// Return the firmware version
        /// </summary>
        /// <returns>Firmware version</returns>
        public ushort GetFirmwareVersion()
        {
            return Read16(Register.VERSION);
        }

        /// <summary>
        /// Get the device id
        /// </summary>
        /// <returns>Device Id</returns>
        public byte GetDeviceId()
        {
            return Read8(Register.ID);
        }

        /// <summary>
        /// Clear event bits for button status
        /// </summary>
        public void ClearEventBits()
        {
            Write8(Register.STATUS, 0);
        }

        /// <summary>
        /// Set I2C Address.
        /// Note that you need to dispose this instance and create a new instance to communicate with the new address.
        /// </summary>
        /// <param name="newAddress">New address (between 0x08 and 0x77)</param>
        public void SetI2CAddress(byte newAddress)
        {
            if (newAddress < 0x08 || newAddress > 0x77)
            {
                throw new ArgumentOutOfRangeException("I2C Address out of range");
            }

            Write8(Register.CHANGE_ADDRESS, newAddress);

            // Note that we need to dispose of this class and re-connect to the new address from here on out
        }

        /// <summary>
        /// Get current I2C Address
        /// </summary>
        /// <returns>Current I2C Address</returns>
        public byte GetI2CAddress()
        {
            return Read8(Register.CHANGE_ADDRESS);
        }

        /// <summary>
        /// Is button pressed (right now)
        /// </summary>
        /// <returns>True if pressed</returns>
        public bool IsPressed()
        {
            return (Read8(Register.STATUS) & 0x02) != 0;
        }

        /// <summary>
        /// Has button been clicked (pressed and depressed)
        /// </summary>
        /// <returns>True if clicked</returns>
        public bool HasBeenClicked()
        {
            return (Read8(Register.STATUS) & 0x04) != 0;
        }

        /// <summary>
        /// Returns true if knob has been twisted
        /// </summary>
        /// <returns>Know has twisted</returns>
        public bool HasMoved()
        {
            return (Read8(Register.STATUS) & 0x01) != 0;
        }

        /// <summary>
        /// Returns the number of indents the user has twisted the knob
        /// </summary>
        /// <returns>Number of indents moved. Can be both positive and negative</returns>
        public short GetCount()
        {
            return (short)Read16(Register.COUNT);
        }

        /// <summary>
        /// Set the encoder count to a specific amount
        /// </summary>
        /// <param name="amount">Amount</param>
        public void SetCount(short amount)
        {
            Write16(Register.COUNT, (ushort)amount);
        }

        /// <summary>
        /// Returns the limit of allowed counts before wrapping
        /// </summary>
        /// <returns>Limit. 0 means disabled</returns>
        public ushort GetLimit()
        {
            return Read16(Register.LIMIT);
        }

        /// <summary>
        /// Set the encoder count limit to a specific amount
        /// </summary>
        /// <param name="amount">Limit. 0 means disabled</param>
        public void SetLimit(ushort amount)
        {
            Write16(Register.LIMIT, amount);
        }

        /// <summary>
        /// Returns the number of ticks since last check
        /// </summary>
        /// <param name="clearValue">True if the difference counter should be cleared</param>
        /// <returns>Difference</returns>
        public short GetDiff(bool clearValue = true)
        {
            short difference = (short)Read16(Register.DIFFERENCE);

            // Clear the current value if requested
            if (clearValue == true)
                Write16(Register.DIFFERENCE, 0);

            return difference;
        }

        /// <summary>
        /// Returns the number of milliseconds since the last encoder movement
        /// </summary>
        /// <param name="clearValue">True if the value should be cleared</param>
        /// <returns>ms</returns>
        public ushort TimeSinceLastMovement(bool clearValue = true)
        {
            ushort timeElapsed = Read16(Register.LAST_ENCODER_EVENT);

            // Clear the current value if requested
            if (clearValue == true)
                Write16(Register.LAST_ENCODER_EVENT, 0);

            return (timeElapsed);
        }

        /// <summary>
        /// Returns the number of milliseconds since the last button event (press and release)
        /// </summary>
        /// <param name="clearValue">True if the value should be cleared</param>
        /// <returns>ms</returns>
        public ushort TimeSinceLastPress(bool clearValue = true)
        {
            ushort timeElapsed = Read16(Register.LAST_BUTTON_EVENT);

            //Clear the current value if requested
            if (clearValue == true)
                Write16(Register.LAST_BUTTON_EVENT, 0);

            return (timeElapsed);
        }

        /// <summary>
        /// Sets the color of the encoder LEDs
        /// </summary>
        /// <param name="red">Red</param>
        /// <param name="green">Green</param>
        /// <param name="blue">Blue</param>
        public void SetColor(byte red, byte green, byte blue)
        {
            _i2cDevice.Write(new byte[]
            {
                (byte)Register.RED,
                red,
                green,
                blue
            });
        }

        /// <summary>
        /// Sets the brightness of red
        /// </summary>
        /// <param name="red">Red</param>
        public void SetRed(byte red)
        {
            Write8(Register.RED, red);
        }

        /// <summary>
        /// Sets the brightness of green
        /// </summary>
        /// <param name="green">Green</param>
        public void SetGreen(byte green)
        {
            Write8(Register.GREEN, green);
        }

        /// <summary>
        /// Sets the brightness of blue
        /// </summary>
        /// <param name="blue">Blue</param>
        public void SetBlue(byte blue)
        {
            Write8(Register.BLUE, blue);
        }

        /// <summary>
        /// Return the current value of red
        /// </summary>
        /// <returns>Current brightness</returns>
        public byte GetRed()
        {
            return Read8(Register.RED);
        }

        /// <summary>
        /// Return the current value of green
        /// </summary>
        /// <returns>Current brightness</returns>
        public byte GetGreen()
        {
            return Read8(Register.GREEN);
        }

        /// <summary>
        /// Return the current value of blue
        /// </summary>
        /// <returns>Current brightness</returns>
        public byte GetBlue()
        {
            return Read8(Register.BLUE);
        }

        /// <summary>
        /// Sets the relation between each color and the twisting of the knob
        /// Connect the LED so it changes [amount] with each encoder tick
        /// Negative numbers are allowed (so LED gets brighter the more you turn the encoder down)
        /// </summary>
        /// <param name="red">Amount of red</param>
        /// <param name="green">Amount of green</param>
        /// <param name="blue">Amount of blue</param>
        public void ConnectColor(short red, short green, short blue)
        {
            _i2cDevice.Write(new byte[]
            {
                (byte)Register.CONNECT_RED,
                (byte)(red >> 8),
                (byte)(red & 0xFF),
                (byte)(green >> 8),
                (byte)(green & 0xFF),
                (byte)(blue >> 8),
                (byte)(blue & 0xFF)
            });
        }

        /// <summary>
        /// Sets the relation between each color and the twisting of the knob
        /// Connect the LED so it changes [amount] with each encoder tick
        /// Negative numbers are allowed (so LED gets brighter the more you turn the encoder down)
        /// </summary>
        /// <param name="red">Amount of red</param>
        public void ConnectRed(short red)
        {
            Write16(Register.CONNECT_RED, (ushort)red);
        }

        /// <summary>
        /// Sets the relation between each color and the twisting of the knob
        /// Connect the LED so it changes [amount] with each encoder tick
        /// Negative numbers are allowed (so LED gets brighter the more you turn the encoder down)
        /// </summary>
        /// <param name="green">Amount of green</param>
        public void ConnectGreen(short green)
        {
            Write16(Register.CONNECT_GREEN, (ushort)green);
        }

        /// <summary>
        /// Sets the relation between each color and the twisting of the knob
        /// Connect the LED so it changes [amount] with each encoder tick
        /// Negative numbers are allowed (so LED gets brighter the more you turn the encoder down)
        /// </summary>
        /// <param name="blue">Amount of blue</param>
        public void ConnectBlue(short blue)
        {
            Write16(Register.CONNECT_BLUE, (ushort)blue);
        }

        /// <summary>
        /// Get the connect value for each color
        /// </summary>
        /// <returns>Amount connected</returns>
        public short GetRedConnect()
        {
            return (short)Read16(Register.CONNECT_RED);
        }

        /// <summary>
        /// Get the connect value for each color
        /// </summary>
        /// <returns>Amount connected</returns>
        public short GetGreenConnect()
        {
            return (short)Read16(Register.CONNECT_GREEN);
        }

        /// <summary>
        /// Get the connect value for each color
        /// </summary>
        /// <returns>Amount connected</returns>
        public short GetBlueConnect()
        {
            return (short)Read16(Register.CONNECT_BLUE);
        }

        /// <summary>
        /// Get number of milliseconds that elapse between end of knob turning and interrupt firing
        /// </summary>
        /// <returns>ms</returns>
        public ushort GetIntTimeout()
        {
            return Read16(Register.TURN_INT_TIMEOUT);
        }

        /// <summary>
        /// Set number of milliseconds that elapse between end of knob turning and interrupt firing
        /// </summary>
        /// <param name="timeout">ms</param>
        public void SetIntTimeout(ushort timeout)
        {
            Write16(Register.TURN_INT_TIMEOUT, timeout);
        }

        internal ushort Read16(Register reg)
        {
            Span<byte> writeBuf = stackalloc byte[] { (byte)reg };
            Span<byte> readBuf = stackalloc byte[2];
            _i2cDevice.WriteRead(writeBuf, readBuf);

            return (ushort)(readBuf[1] << 8 | readBuf[0]);
        }

        internal byte Read8(Register reg)
        {
            Span<byte> writeBuf = stackalloc byte[] { (byte)reg };
            Span<byte> readBuf = stackalloc byte[1];
            _i2cDevice.WriteRead(writeBuf, readBuf);
            return readBuf[0];
        }

        internal void Write8(Register reg, byte value)
        {
            _i2cDevice.Write(new byte[]
            {
                (byte)reg,
                value
            });
        }

        internal void Write16(Register reg, ushort value)
        {
            _i2cDevice.Write(new byte[]
            {
                (byte)reg,
                (byte)(value & 0xFF),
                (byte)(value >> 8 & 0xFF)
            });
        }
    }
}
