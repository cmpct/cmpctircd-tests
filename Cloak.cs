using NUnit.Framework;
using System;
using cmpctircd;
using cmpctircd.Configuration;

using System.Linq;
using System.Threading;
using cmpctircd.Threading;
using cmpctircd.Configuration;
using System.Configuration;

namespace cmpctircd.Tests
{
    public class CloakTests
    {
        IRCd ircd;
        Log log;
        CmpctConfigurationSection config;

        [SetUp]
        public void Setup()
        {
            config = CmpctConfigurationSection.GetConfiguration();
            log = new Log();
            ircd = new IRCd(log, config);
        }

        [Test]
        public void TestCloakDomain() {
            String host  = "test.cmpct.info";
            String cloak = cmpctircd.Cloak.GetCloak(host, null, ircd.CloakKey, false);
            Assert.AreEqual("cmpct-dltnfc.cmpct.info", cloak);
        }

        [Test]
        public void TestCloakIPv4() {
            // TODO: Test with more IPs (non-loopback)
            var IP    = System.Net.IPAddress.Loopback;
            var cloak = cmpctircd.Cloak.InspCloakIPv4(IP, ircd.CloakKey, true);
            Assert.AreEqual("cmpct-7t0.v64.q8dsol.IP", cloak);
        }

        [Test]
        public void TestCloakIPv6() {
            // TODO: Test with more IPs (non-loopback)
            var IP    = System.Net.IPAddress.IPv6Loopback;
            var cloak = cmpctircd.Cloak.GetCloak(null, IP, ircd.CloakKey, true);
            Assert.AreEqual("cmpct-g7hb88.479u.kc7f.truk2h.IP", cloak);
        }
    }
}