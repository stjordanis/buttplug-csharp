﻿// <copyright file="MysteryVibeTests.cs" company="Nonpolynomial Labs LLC">
// Buttplug C# Source Code File - Visit https://buttplug.io for more info about the project.
// Copyright (c) Nonpolynomial Labs LLC. All rights reserved.
// Licensed under the BSD 3-Clause license. See LICENSE file in the project root for full license information.
// </copyright>

// Test file, disable ConfigureAwait checking.
// ReSharper disable ConsiderUsingConfigureAwait

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Buttplug.Core.Messages;
using Buttplug.Devices;
using Buttplug.Devices.Protocols;
using Buttplug.Test.Devices.Protocols.Utils;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Buttplug.Test.Devices.Protocols
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Test classes can skip documentation requirements")]
    [TestFixture]
    public class MysteryVibeTests
    {
        [NotNull]
        private ProtocolTestUtils testUtil;

        [SetUp]
        public async Task Init()
        {
            testUtil = new ProtocolTestUtils();
            await testUtil.SetupTest<MysteryVibeProtocol>("MV Crescendo");
        }

        [Test]
        public void TestAllowedMessages()
        {
            testUtil.TestDeviceAllowedMessages(new Dictionary<System.Type, uint>()
            {
                { typeof(StopDeviceCmd), 0 },
                { typeof(SingleMotorVibrateCmd), 0 },
                { typeof(VibrateCmd), 6 },
            });
        }

        // StopDeviceCmd noop test handled in GeneralDeviceTests

        [Test]
        public async Task TestStopDeviceCmd()
        {
            var expected =
                new List<(byte[], string)>()
                {
                    (Enumerable.Repeat((byte)(MysteryVibeProtocol.MaxSpeed * 0.5), 6).ToArray(), Endpoints.TxVibrate),
                };

            await testUtil.TestDeviceMessage(new SingleMotorVibrateCmd(4, 0.5), expected, false);

            expected =
                new List<(byte[], string)>()
                {
                    (MysteryVibeProtocol.NullSpeed, Endpoints.TxVibrate),
                };

            await testUtil.TestDeviceMessageOnWrite(new StopDeviceCmd(4), expected, false);
        }

        [Test]
        public async Task TestSingleMotorVibrateCmd()
        {
            var expected =
                new List<(byte[], string)>()
                {
                    (Enumerable.Repeat((byte)(MysteryVibeProtocol.MaxSpeed * 0.5), 6).ToArray(), Endpoints.TxVibrate),
                };

            await testUtil.TestDeviceMessage(new SingleMotorVibrateCmd(4, 0.5), expected, false);
        }

        [Test]
        public async Task TestVibrateCmd()
        {
            var expected =
                new List<(byte[], string)>()
                {
                    (Enumerable.Repeat((byte)(MysteryVibeProtocol.MaxSpeed * 0.5), 6).ToArray(), Endpoints.TxVibrate),
                };

            await testUtil.TestDeviceMessage(VibrateCmd.Create(4, 1, 0.5, 6), expected, false);
        }

        [Test]
        public void TestInvalidVibrateCmd()
        {
            testUtil.TestInvalidVibrateCmd(6);
        }
    }
}