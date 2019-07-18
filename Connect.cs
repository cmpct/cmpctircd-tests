using NUnit.Framework;
using System;
using cmpctircd;

using System.Linq;
using cmpctircd.Configuration;
using System.Configuration;

using cmpctircd.Threading;
using System.Threading;
using System.Threading.Tasks;

using IrcDotNet;

namespace cmpctircd.Tests
{
    public class ConnectTests
    {
        
        Task task;
        IRCd ircd;
        Log log;
        CmpctConfigurationSection config;

        IrcClient irc;

        [SetUp]
        public void Setup()
        {   
            task = Task.Run(() => {
                    using(var sc = new QueuedSynchronizationContext()) {
                        config = CmpctConfigurationSection.GetConfiguration();
                        SynchronizationContext.SetSynchronizationContext(sc);

                        log = new Log();
                        ircd = new IRCd(log, config);

                        log.Initialise(ircd, config.Loggers.OfType<LoggerElement>().ToList());
                        ircd.Run();
                        sc.Run();
                    } 
                }
            );

            // Hack to give the IRCd long enough to listen before we start trying to connect to it
            // TODO: Can we do without this?
            task.Wait(100);
        }

        [TearDown]
        public void Teardown() {
            // TODO: Shut down the ircd/listener
            ircd.Stop();
        }

        [Test]
        public void TestBasicConnectAndRegistration() {
            var client = new StandardIrcClient();

            using(var connectedEvent = new ManualResetEventSlim(false)) {
                var registrationInfo = new IrcUserRegistrationInfo() {
                    NickName = "cmpct_test", 
                    Password = "", 
                    UserName = "cmpct_test", 
                    RealName = "cmpct_test"
                };
                client.Connected += (sender, e) => connectedEvent.Set();
                client.Connect("127.0.0.1", 6667, false, registrationInfo);
                
                if(!connectedEvent.Wait(100)) {
                    client.Dispose();
                    Assert.Fail("ConnectedEvent not called: server not listening?");
                }
                
                using(var registrationEvent = new ManualResetEventSlim(false)) {
                    client.Registered += (sender, e) => registrationEvent.Set();

                    if(!registrationEvent.Wait(500)) {
                        client.Dispose();
                        Assert.Fail("RegistrationEvent not called: not registered?");
                    }
                }

            }
        }

        [Test]
        public void TestJoinChannel() {
            var client = new StandardIrcClient();
            var registrationInfo = new IrcUserRegistrationInfo() {
                NickName = "cmpct_test", 
                Password = "", 
                UserName = "cmpct_test", 
                RealName = "cmpct_test"
            };
            
            client.Connect("127.0.0.1", 6667, false, registrationInfo);
            

            using(var registrationEvent = new ManualResetEventSlim(false)) {
                // Need to include the registration check for the delay
                client.Registered += (sender, e) => registrationEvent.Set();

                if(!registrationEvent.Wait(500)) {
                        client.Dispose();
                        Assert.Fail("RegistrationEvent not called: not registered?");
                    }
                }

                using(var joinedChannelEvent = new ManualResetEventSlim(false)) {
                    client.LocalUser.JoinedChannel += (sender, e) => joinedChannelEvent.Set();
                    client.Channels.Join("#test");

                    if(!joinedChannelEvent.Wait(500)) {
                        client.Dispose();
                        Assert.Fail("JoinedChannelEvent not called: channel not joined?");
                    }
                }
        }
    }
}