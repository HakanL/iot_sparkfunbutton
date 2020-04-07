// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.SparkfunQwiicButton
{
    /// <summary>
    /// SparkfunQwiicButton Register
    /// </summary>
    public enum Register : byte
    {
        ID = 0x00,
        FIRMWARE_MINOR = 0x01,
        FIRMWARE_MAJOR = 0x02,

        // Bit 0 - User mutable, gets set to 1 when a new event occurs. User is expected to write 0 to clear the flag.
        // Bit 1 - Defaults to zero on POR. Gets set to one when the button gets clicked. Must be cleared by the user.
        // Bit 2 - Gets set to one if button is pushed.
        BUTTON_STATUS = 0x03,
        // Bit 0 - User mutable, set to 1 to enable an interrupt when the button is clicked. Defaults to 0.
        // Bit 1 - User mutable, set to 1 to enable an interrupt when the button is pressed. Defaults to 0.
        INTERRUPT_CONFIG = 0x04,
        BUTTON_DEBOUNCE_TIME = 0x05,
        // Bit 0 - User mutable, user sets to 1 to pop from queue, we pop from queue and set the bit back to zero.
        // Bit 1 - User immutable, returns 1 or 0 depending on whether or not the queue is empty
        // Bit 2 - User immutable, returns 1 or 0 depending on whether or not the queue is full
        PRESSED_QUEUE_STATUS = 0x07,
        PRESSED_QUEUE_FRONT = 0x08,
        PRESSED_QUEUE_BACK = 0x0C,
        // Bit 0 - User mutable, user sets to 1 to pop from queue, we pop from queue and set the bit back to zero.
        // Bit 1 - User immutable, returns 1 or 0 depending on whether or not the queue is empty
        // Bit 2 - User immutable, returns 1 or 0 depending on whether or not the queue is full
        CLICKED_QUEUE_STATUS = 0x10,
        CLICKED_QUEUE_FRONT = 0x11,
        CLICKED_QUEUE_BACK = 0x15,
        LED_BRIGHTNESS = 0x19,
        LED_PULSE_GRANULARITY = 0x1A,
        LED_PULSE_CYCLE_TIME = 0x1B,
        LED_PULSE_OFF_TIME = 0x1D,
        I2C_ADDRESS = 0x1F
    }
}
