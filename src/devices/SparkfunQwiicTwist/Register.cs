// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.SparkfunQwiicTwist
{
    /// <summary>
    /// SparkfunQwiicButton Register
    /// </summary>
    public enum Register : byte
    {
        ID = 0x00,
        STATUS = 0x01, //2 - button clicked, 1 - button pressed, 0 - encoder moved
        VERSION = 0x02,
        ENABLE_INTS = 0x04, //1 - button interrupt, 0 - encoder interrupt
        COUNT = 0x05,
        DIFFERENCE = 0x07,
        LAST_ENCODER_EVENT = 0x09, //Millis since last movement of knob
        LAST_BUTTON_EVENT = 0x0B,  //Millis since last press/release

        RED = 0x0D,
        GREEN = 0x0E,
        BLUE = 0x0F,

        CONNECT_RED = 0x10, //Amount to change red LED for each encoder tick
        CONNECT_GREEN = 0x12,
        CONNECT_BLUE = 0x14,

        TURN_INT_TIMEOUT = 0x16,
        CHANGE_ADDRESS = 0x18,
        LIMIT = 0x19
    }
}
