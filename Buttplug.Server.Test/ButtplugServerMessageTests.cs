﻿// <copyright file="ButtplugMessageTests.cs" company="Nonpolynomial Labs LLC">
// Buttplug C# Source Code File - Visit https://buttplug.io for more info about the project.
// Copyright (c) Nonpolynomial Labs LLC. All rights reserved.
// Licensed under the BSD 3-Clause license. See LICENSE file in the project root for full license information.
// </copyright>

// Test file, disable ConfigureAwait checking.
// ReSharper disable ConsiderUsingConfigureAwait

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Buttplug.Core.Logging;
using Buttplug.Core.Messages;
using FluentAssertions;
using NUnit.Framework;

namespace Buttplug.Server.Test
{
    [TestFixture]
    public class ButtplugServerMessageTests
    {
        private TestServer _server;

        [SetUp]
        public async Task TestServer()
        {
            _server = new TestServer();
            var msg = await _server.SendMessageAsync(new RequestServerInfo("TestClient"));
            msg.Should().BeOfType<ServerInfo>();
        }

        [Test]
        public async Task TestRepeatedHandshake()
        {
            // Sending RequestServerInfo twice should throw, otherwise weird things like Spec version changes could happen.
            Func<Task> act = async () => await _server.SendMessageAsync(new RequestServerInfo("TestClient"));
            act.Should().Throw<ButtplugServerException>();
        }

        [Test]
        public async Task TestRequestLog()
        {
            var res = await _server.SendMessageAsync(new RequestLog(ButtplugLogLevel.Debug));
            res.Should().BeOfType<Ok>();
        }

        [Test]
        public async Task TestCallStartScanning()
        {
            var dm = new TestDeviceSubtypeManager();
            _server.AddDeviceSubtypeManager(aLogger => { return dm; });
            var r = await _server.SendMessageAsync(new StartScanning());
            r.Should().BeOfType<Ok>();
            dm.StartScanningCalled.Should().BeTrue();
        }

        [ButtplugMessageMetadata("FakeMessage", 0)]
        private class FakeMessage : ButtplugMessage
        {
            public FakeMessage(uint aId)
                : base(aId)
            {
            }
        }

        [Test]
        public async Task TestSendUnhandledMessage()
        {
            Func<Task> r = async () => await _server.SendMessageAsync(new FakeMessage(1));
            r.Should().Throw<ButtplugServerException>();
        }

        [Test]
        public async Task TestStopScanning()
        {
            var dm = new TestDeviceSubtypeManager();
            _server.AddDeviceSubtypeManager(aLogger => dm);
            var r = await _server.SendMessageAsync(new StopScanning());
            r.Should().BeOfType<Ok>();
            dm.StopScanningCalled.Should().BeTrue();
        }

        [Test]
        public async Task TestRequestServerInfo()
        {
            var s = new ButtplugServer("TestServer", 100);
            var r = await s.SendMessageAsync(new RequestServerInfo("TestClient"));

            r.Should().BeOfType<ServerInfo>();
            var info = r as ServerInfo;
            info.MajorVersion.Should().Be(Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.Major);
            info.MinorVersion.Should().Be(Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.Minor);
            info.BuildVersion.Should().Be(Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.Build);
        }

        [Test]
        public async Task TestDoNotRequestServerInfoFirst()
        {
            var s = new ButtplugServer("TestServer", 0);
            Func<Task> act = async () => await s.SendMessageAsync(new Core.Messages.Test("Test"));
            act.Should().Throw<ButtplugServerException>();

            var msg = await s.SendMessageAsync(new RequestServerInfo("TestClient"));
            msg.Should().BeOfType<ServerInfo>();

            msg = await s.SendMessageAsync(new Core.Messages.Test("Test"));
            msg.Should().BeOfType<Core.Messages.Test>();
        }
    }
}
