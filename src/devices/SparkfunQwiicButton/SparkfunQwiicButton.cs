// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.SparkfunQwiicButton
{
    /// <summary>
    /// Sparkfun's Qwiic button module
    /// </summary>
    public class SparkfunQwiicButton : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Device Id
        /// </summary>
        public const byte DeviceId = 0x5D;

        /// <summary>
        /// Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x6F;

        /// <summary>
        /// Creates a new instance of the SparkfunQwiicButton
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public SparkfunQwiicButton(I2cDevice i2cDevice)
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
            byte lsb = Read8(Register.FIRMWARE_MINOR);
            byte msb = Read8(Register.FIRMWARE_MAJOR);

            return (ushort)(msb << 8 | lsb);
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
            byte value = Read8(Register.BUTTON_STATUS);
            value &= 0xF8;      // Clear bits 0-2
            Write8(Register.BUTTON_STATUS, value);
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

            Write8(Register.I2C_ADDRESS, newAddress);

            // Note that we need to dispose of this class and re-connect to the new address from here on out
        }

        /// <summary>
        /// Get current I2C Address
        /// </summary>
        /// <returns>Current I2C Address</returns>
        public byte GetI2CAddress()
        {
            return Read8(Register.I2C_ADDRESS);
        }

        /// <summary>
        /// Is button pressed (right now)
        /// </summary>
        /// <returns>True if pressed</returns>
        public bool IsPressed()
        {
            return (Read8(Register.BUTTON_STATUS) & 0x04) != 0;
        }

        /// <summary>
        /// Has button been clicked (pressed and depressed)
        /// </summary>
        /// <returns>True if clicked</returns>
        public bool HasBeenClicked()
        {
            return (Read8(Register.BUTTON_STATUS) & 0x02) != 0;
        }

        /// <summary>
        /// Get debounce time
        /// </summary>
        /// <returns>Debounce time in ms</returns>
        public ushort GetDebounceTime()
        {
            return Read16(Register.BUTTON_DEBOUNCE_TIME);
        }

        /// <summary>
        /// Set debounce time
        /// </summary>
        /// <param name="time">Debounce in ms</param>
        public void SetDebounceTime(ushort time)
        {
            Write16(Register.BUTTON_DEBOUNCE_TIME, time);
        }

        /// <summary>
        /// Enable interrupt when button is pressed
        /// </summary>
        public void EnablePressedInterrupt()
        {
            byte value = Read8(Register.INTERRUPT_CONFIG);
            value = (byte)(value | 0x02);
            Write8(Register.INTERRUPT_CONFIG, value);
        }

        /// <summary>
        /// Disable interupt when button is pressed
        /// </summary>
        public void DisablePressedInterrupt()
        {
            byte value = Read8(Register.INTERRUPT_CONFIG);
            value = (byte)(value & 0xFD);
            Write8(Register.INTERRUPT_CONFIG, value);
        }

        /// <summary>
        /// Enable interupt when button is clicked (pressed and depressed)
        /// </summary>
        public void EnableClickedInterrupt()
        {
            byte value = Read8(Register.INTERRUPT_CONFIG);
            value = (byte)(value | 0x01);
            Write8(Register.INTERRUPT_CONFIG, value);
        }

        /// <summary>
        /// Disable interupt when button is clicked (pressed and depressed)
        /// </summary>
        public void DisableClickedInterrupt()
        {
            byte value = Read8(Register.INTERRUPT_CONFIG);
            value = (byte)(value & 0xFE);
            Write8(Register.INTERRUPT_CONFIG, value);
        }

        /// <summary>
        /// Get if events are available
        /// </summary>
        /// <returns>True if at least one event is available in the queue</returns>
        public bool EventAvailable()
        {
            return (Read8(Register.BUTTON_STATUS) & 0x01) != 0;
        }

        /// <summary>
        /// Reset interrupt flags
        /// </summary>
        public void ResetInterruptConfig()
        {
            Write8(Register.INTERRUPT_CONFIG, 0x03);
            Write8(Register.BUTTON_STATUS, 0x00);
        }

        /// <summary>
        /// Get if pressed queue is full
        /// </summary>
        /// <returns>True if full</returns>
        public bool IsPressedQueueFull()
        {
            byte value = Read8(Register.PRESSED_QUEUE_STATUS);

            return (value & 0x04) != 0;
        }

        /// <summary>
        /// Get if pressed queue is empty
        /// </summary>
        /// <returns>True if empty</returns>
        public bool IsPressedQueueEmpty()
        {
            byte value = Read8(Register.PRESSED_QUEUE_STATUS);

            return (value & 0x02) != 0;
        }

        /// <summary>
        /// Time since newest time button was pressed
        /// </summary>
        /// <returns>Time in ms</returns>
        public uint TimeSinceNewestPress()
        {
            return Read32(Register.PRESSED_QUEUE_FRONT);
        }

        /// <summary>
        /// Time since oldest time button was pressed
        /// </summary>
        /// <returns>Time in ms</returns>
        public uint TimeSinceOldestPress()
        {
            return Read32(Register.PRESSED_QUEUE_BACK);
        }

        /// <summary>
        /// Pop a value off the pressed queue
        /// </summary>
        /// <returns>Time in ms</returns>
        public uint PopPressedQueue()
        {
            // Grab the oldest value on the queue
            uint tempData = TimeSinceOldestPress();

            byte value = Read8(Register.PRESSED_QUEUE_STATUS);

            value = (byte)(value | 0x01);
            Write8(Register.PRESSED_QUEUE_STATUS, value);
            uint value1 = TimeSinceOldestPress();

            Console.WriteLine($"Read 1 = {tempData:N0}");
            Console.WriteLine($"Read 2 = {value1:N0}");

            //value = (byte)(value & 0xFB);
            //Write8(Register8.PRESSED_QUEUE_STATUS, value);


            // Return the value we popped
            return tempData;
        }

        /// <summary>
        /// Get if clicked queue is full
        /// </summary>
        /// <returns>True if full</returns>
        public bool IsClickedQueueFull()
        {
            byte value = Read8(Register.CLICKED_QUEUE_STATUS);

            return (value & 0x04) != 0;
        }

        /// <summary>
        /// Get if clicked queue is empty
        /// </summary>
        /// <returns>True if empty</returns>
        public bool IsClickedQueueEmpty()
        {
            byte value = Read8(Register.CLICKED_QUEUE_STATUS);
            return (value & 0x02) != 0;
        }

        /// <summary>
        /// Time since newest time button was clicked (pressed and depressed)
        /// </summary>
        /// <returns>Time in ms</returns>
        public uint TimeSinceNewestClick()
        {
            return Read32(Register.CLICKED_QUEUE_FRONT);
        }

        /// <summary>
        /// Time since oldest time button was clicked (pressed and depressed)
        /// </summary>
        /// <returns>Time in ms</returns>
        public uint TimeSinceOldestClick()
        {
            return Read32(Register.CLICKED_QUEUE_BACK);
        }

        /// <summary>
        /// Pop a value off the clicked queue
        /// </summary>
        /// <returns>Time in ms</returns>
        public uint PopClickedQueue()
        {
            // Grab the oldest value on the queue
            uint tempData = TimeSinceOldestClick();

            byte value = Read8(Register.CLICKED_QUEUE_STATUS);
            value = (byte)(value | 0x01);
            Write8(Register.CLICKED_QUEUE_STATUS, value);
            uint value1 = TimeSinceOldestPress();

            // Return the value we popped
            return tempData;
        }

        /// <summary>
        /// Set LED configuration
        /// </summary>
        /// <param name="brightness">Brightness 0-255</param>
        /// <param name="cycleTime">Total pulse cycle in ms, does not include off time, LED pulse disabled if 0</param>
        /// <param name="offTime">Off Time between pulses in ms (Default is 500ms)</param>
        /// <param name="granularity">The amount of steps it takes to get to LED brightness</param>
        public void SetLEDconfig(byte brightness, ushort cycleTime, ushort offTime, byte granularity)
        {
            Write8(Register.LED_BRIGHTNESS, brightness);
            Write8(Register.LED_PULSE_GRANULARITY, granularity);
            Write16(Register.LED_PULSE_CYCLE_TIME, cycleTime);
            Write16(Register.LED_PULSE_OFF_TIME, offTime);
        }

        /// <summary>
        /// Turn off LED
        /// </summary>
        public void LEDoff()
        {
            SetLEDconfig(0, 0, 0, 0);
        }

        /// <summary>
        /// Set LED brightness
        /// </summary>
        /// <param name="brightness">Brightness 0-255 (0 = off)</param>
        public void LEDon(byte brightness)
        {
            SetLEDconfig(brightness, 0, 0, 0);
        }

        internal ushort Read16(Register reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            Span<byte> buf = stackalloc byte[2];
            _i2cDevice.Read(buf);

            return (ushort)(buf[1] << 8 | buf[0]);
        }

        internal uint Read32(Register reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            Span<byte> buf = stackalloc byte[4];
            _i2cDevice.Read(buf);

            return (uint)(buf[3] << 24 | buf[2] << 16 | buf[1] << 8 | buf[0]);
        }

        internal byte Read8(Register reg)
        {
            Span<byte> writeBuf = stackalloc byte[] { (byte)reg };
            Span<byte> readBuf = stackalloc byte[1];
            _i2cDevice.WriteRead(writeBuf, readBuf);
            return readBuf[0];
            //_i2cDevice.WriteByte((byte)reg);

            //return _i2cDevice.ReadByte();
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
