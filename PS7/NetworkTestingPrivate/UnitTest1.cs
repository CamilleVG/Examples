// Author: Daniel Kopta, May 2019
// Unit testing examples for CS 3500 networking library (part of final project)
// University of Utah


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkUtil {

    [TestClass]
    public class NetworkTests {
        // When testing network code, we have some necessary global state,
        // since open sockets are system-wide (managed by the OS)
        // Therefore, we need some per-test setup and cleanup
        private TcpListener testListener;
        private SocketState testLocalSocketState, testRemoteSocketState;


        [TestInitialize]
        public void Init() {
            testListener = null;
            testLocalSocketState = null;
            testRemoteSocketState = null;
        }


        [TestCleanup]
        public void Cleanup() {
            StopTestServer(testListener, testLocalSocketState, testRemoteSocketState);
        }


        private void StopTestServer(TcpListener listener, SocketState socket1, SocketState socket2) {
            try {
                // '?.' is just shorthand for null checks
                listener?.Stop();
                socket1?.TheSocket?.Shutdown(SocketShutdown.Both);
                socket1?.TheSocket?.Close();
                socket2?.TheSocket?.Shutdown(SocketShutdown.Both);
                socket2?.TheSocket?.Close();
            }
            // Do nothing with the exception, since shutting down the server will likely result in 
            // a prematurely closed socket
            // If the timeout is long enough, the shutdown should succeed
            catch (Exception) { }
        }



        public void SetupTestConnections(bool clientSide,
          out TcpListener listener, out SocketState local, out SocketState remote, int port) {
            if (clientSide) {
                NetworkTestHelper.SetupSingleConnectionTest(
                  out listener,
                  out local,    // local becomes client
                  out remote,// remote becomes server
                  port);
            }
            else {
                NetworkTestHelper.SetupSingleConnectionTest(
                  out listener,
                  out remote,   // remote becomes client
                  out local,// local becomes server
                  port);
            }

            Assert.IsNotNull(local);
            Assert.IsNotNull(remote);
        }


        /*** Begin Basic Connectivity Tests ***/
        [TestMethod]
        public void TestConnect() {
            NetworkTestHelper.SetupSingleConnectionTest(out testListener, out testLocalSocketState, out testRemoteSocketState, 2112);

            Assert.IsTrue(testRemoteSocketState.TheSocket.Connected);
            Assert.IsTrue(testLocalSocketState.TheSocket.Connected);

            Assert.AreEqual("127.0.0.1:2112", testLocalSocketState.TheSocket.RemoteEndPoint.ToString());
        }


        [TestMethod]
        public void TestConnectNoServer() {
            bool isCalled = false;

            void saveClientState(SocketState x) {
                isCalled = true;
                testLocalSocketState = x;
            }

            // Try to connect without setting up a server first.
            Networking.ConnectToServer(saveClientState, "localhost", 2126);
            NetworkTestHelper.WaitForOrTimeout(() => isCalled, NetworkTestHelper.timeout);

            Assert.IsTrue(isCalled);
            Assert.IsTrue(testLocalSocketState.ErrorOccured);
        }


        [TestMethod]
        public void TestConnectTimeout() {
            bool isCalled = false;

            void saveClientState(SocketState x) {
                isCalled = true;
                testLocalSocketState = x;
            }

            Networking.ConnectToServer(saveClientState, "google.com", 2112);

            // The connection should timeout after 3 seconds. NetworkTestHelper.timeout is 5 seconds.
            NetworkTestHelper.WaitForOrTimeout(() => isCalled, NetworkTestHelper.timeout);

            Assert.IsTrue(isCalled);
            Assert.IsTrue(testLocalSocketState.ErrorOccured);
        }


        [TestMethod]
        public void TestConnectCallsDelegate() {
            bool serverActionCalled = false;
            bool clientActionCalled = false;

            void saveServerState(SocketState x) {
                testLocalSocketState = x;
                serverActionCalled = true;
            }

            void saveClientState(SocketState x) {
                testRemoteSocketState = x;
                clientActionCalled = true;
            }

            testListener = Networking.StartServer(saveServerState, 2112);
            Networking.ConnectToServer(saveClientState, "localhost", 2112);
            NetworkTestHelper.WaitForOrTimeout(() => serverActionCalled, NetworkTestHelper.timeout);
            NetworkTestHelper.WaitForOrTimeout(() => clientActionCalled, NetworkTestHelper.timeout);

            Assert.IsTrue(serverActionCalled);
            Assert.IsTrue(clientActionCalled);
        }


        /// <summary>
        /// This is an example of a parameterized test. 
        /// DataRow(true) and DataRow(false) means this test will be 
        /// invoked once with an argument of true, and once with false.
        /// This way we can test your Send method from both
        /// client and server sockets. In theory, there should be no 
        /// difference, but odd things can happen if you save static
        /// state (such as sockets) in your networking library.
        /// </summary>
        /// <param name="clientSide"></param>
        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestDisconnectLocalThenSend(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2116);

            testLocalSocketState.TheSocket.Shutdown(SocketShutdown.Both);

            // No assertions, but the following should not result in an unhandled exception
            Networking.Send(testLocalSocketState.TheSocket, "a");
        }

        /*** End Basic Connectivity Tests ***/


        /*** Begin Send/Receive Tests ***/

        // In these tests, "local" means the SocketState doing the sending,
        // and "remote" is the one doing the receiving.
        // Each test will run twice, swapping the sender and receiver between
        // client and server, in order to defeat statically-saved SocketStates
        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestSendTinyMessage(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2117);

            // Set the action to do nothing
            testLocalSocketState.OnNetworkAction = x => { };
            testRemoteSocketState.OnNetworkAction = x => { };

            Networking.Send(testLocalSocketState.TheSocket, "a");

            Networking.GetData(testRemoteSocketState);

            // Note that waiting for data like this is *NOT* how the networking library is 
            // intended to be used. This is only for testing purposes.
            // Normally, you would provide an OnNetworkAction that handles the data.
            NetworkTestHelper.WaitForOrTimeout(() => testRemoteSocketState.GetData().Length > 0, NetworkTestHelper.timeout);

            Assert.AreEqual("a", testRemoteSocketState.GetData());
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestNoEventLoop(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2115);

            int calledCount = 0;

            // This OnNetworkAction will not ask for more data after receiving one message,
            // so it should only ever receive one message
            testLocalSocketState.OnNetworkAction = (x) => calledCount++;

            Networking.Send(testRemoteSocketState.TheSocket, "a");
            Networking.GetData(testLocalSocketState);
            // Note that waiting for data like this is *NOT* how the networking library is 
            // intended to be used. This is only for testing purposes.
            // Normally, you would provide an OnNetworkAction that handles the data.
            NetworkTestHelper.WaitForOrTimeout(() => testLocalSocketState.GetData().Length > 0, NetworkTestHelper.timeout);

            // Send a second message (which should not increment calledCount)
            Networking.Send(testRemoteSocketState.TheSocket, "a");
            NetworkTestHelper.WaitForOrTimeout(() => false, NetworkTestHelper.timeout);

            Assert.AreEqual(1, calledCount);
        }


        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestDelayedSends(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2118);

            // Set the action to do nothing
            testLocalSocketState.OnNetworkAction = x => { };
            testRemoteSocketState.OnNetworkAction = x => { };

            Networking.Send(testLocalSocketState.TheSocket, "a");
            Networking.GetData(testRemoteSocketState);
            // Note that waiting for data like this is *NOT* how the networking library is 
            // intended to be used. This is only for testing purposes.
            // Normally, you would provide an OnNetworkAction that handles the data.
            NetworkTestHelper.WaitForOrTimeout(() => testRemoteSocketState.GetData().Length > 0, NetworkTestHelper.timeout);

            Networking.Send(testLocalSocketState.TheSocket, "b");
            Networking.GetData(testRemoteSocketState);
            // Note that waiting for data like this is *NOT* how the networking library is 
            // intended to be used. This is only for testing purposes.
            // Normally, you would provide an OnNetworkAction that handles the data.
            NetworkTestHelper.WaitForOrTimeout(() => testRemoteSocketState.GetData().Length > 1, NetworkTestHelper.timeout);

            Assert.AreEqual("ab", testRemoteSocketState.GetData());
        }


        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestEventLoop(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2119);

            int calledCount = 0;

            // This OnNetworkAction asks for more data, creating an event loop
            testLocalSocketState.OnNetworkAction = (x) => {
                if (x.ErrorOccured)
                    return;
                calledCount++;
                Networking.GetData(x);
            };

            Networking.Send(testRemoteSocketState.TheSocket, "a");
            Networking.GetData(testLocalSocketState);
            NetworkTestHelper.WaitForOrTimeout(() => calledCount == 1, NetworkTestHelper.timeout);

            Networking.Send(testRemoteSocketState.TheSocket, "a");
            NetworkTestHelper.WaitForOrTimeout(() => calledCount == 2, NetworkTestHelper.timeout);

            Assert.AreEqual(2, calledCount);
        }


        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestChangeOnNetworkAction(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2120);

            int firstCalledCount = 0;
            int secondCalledCount = 0;

            // This is an example of a nested method (just another way to make a quick delegate)
            void firstOnNetworkAction(SocketState state) {
                if (state.ErrorOccured)
                    return;
                firstCalledCount++;
                state.OnNetworkAction = secondOnNetworkAction;
                Networking.GetData(testLocalSocketState);
            }

            void secondOnNetworkAction(SocketState state) {
                secondCalledCount++;
            }

            // Change the OnNetworkAction after the first invokation
            testLocalSocketState.OnNetworkAction = firstOnNetworkAction;

            Networking.Send(testRemoteSocketState.TheSocket, "a");
            Networking.GetData(testLocalSocketState);
            NetworkTestHelper.WaitForOrTimeout(() => firstCalledCount == 1, NetworkTestHelper.timeout);

            Networking.Send(testRemoteSocketState.TheSocket, "a");
            NetworkTestHelper.WaitForOrTimeout(() => secondCalledCount == 1, NetworkTestHelper.timeout);

            Assert.AreEqual(1, firstCalledCount);
            Assert.AreEqual(1, secondCalledCount);
        }



        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestReceiveRemovesAll(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2121);

            StringBuilder localCopy = new StringBuilder();

            void removeMessage(SocketState state) {
                if (state.ErrorOccured)
                    return;
                localCopy.Append(state.GetData());
                state.RemoveData(0, state.GetData().Length);
                Networking.GetData(state);
            }

            testLocalSocketState.OnNetworkAction = removeMessage;

            // Start a receive loop
            Networking.GetData(testLocalSocketState);

            for (int i = 0; i < 10000; i++) {
                char c = (char)('a' + (i % 26));
                Networking.Send(testRemoteSocketState.TheSocket, "" + c);
            }

            NetworkTestHelper.WaitForOrTimeout(() => localCopy.Length == 10000, NetworkTestHelper.timeout);

            // Reconstruct the original message outside the send loop
            // to (in theory) make the send operations happen more rapidly.
            StringBuilder message = new StringBuilder();
            for (int i = 0; i < 10000; i++) {
                char c = (char)('a' + (i % 26));
                message.Append(c);
            }

            Assert.AreEqual(message.ToString(), localCopy.ToString());
        }


        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestReceiveRemovesPartial(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2122);

            const string toSend = "abcdefghijklmnopqrstuvwxyz";

            // Use a static seed for reproducibility
            Random rand = new Random(0);

            StringBuilder localCopy = new StringBuilder();

            void removeMessage(SocketState state) {
                if (state.ErrorOccured)
                    return;
                int numToRemove = rand.Next(state.GetData().Length);
                localCopy.Append(state.GetData().Substring(0, numToRemove));
                state.RemoveData(0, numToRemove);
                Networking.GetData(state);
            }

            testLocalSocketState.OnNetworkAction = removeMessage;

            // Start a receive loop
            Networking.GetData(testLocalSocketState);

            for (int i = 0; i < 1000; i++) {
                Networking.Send(testRemoteSocketState.TheSocket, toSend);
            }

            // Wait a while
            NetworkTestHelper.WaitForOrTimeout(() => false, NetworkTestHelper.timeout);

            localCopy.Append(testLocalSocketState.GetData());

            // Reconstruct the original message outside the send loop
            // to (in theory) make the send operations happen more rapidly.
            StringBuilder message = new StringBuilder();
            for (int i = 0; i < 1000; i++) {
                message.Append(toSend);
            }

            Assert.AreEqual(message.ToString(), localCopy.ToString());
        }



        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestReceiveHugeMessage(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2123);

            testLocalSocketState.OnNetworkAction = (x) => {
                if (x.ErrorOccured)
                    return;
                Networking.GetData(x);
            };

            Networking.GetData(testLocalSocketState);

            StringBuilder message = new StringBuilder();
            message.Append('a', (int)(SocketState.BufferSize * 7.5));

            Networking.Send(testRemoteSocketState.TheSocket, message.ToString());

            NetworkTestHelper.WaitForOrTimeout(() => testLocalSocketState.GetData().Length == message.Length, NetworkTestHelper.timeout);

            Assert.AreEqual(message.ToString(), testLocalSocketState.GetData());
        }

        /*** End Send/Receive Tests ***/


        // Our tests
        [TestMethod]
        public void TestStartServer() {
            SocketState state = new SocketState(null, null);
            void toCall(SocketState s) {
            }

            bool passed = false;
            try {
                TcpListener listener = Networking.StartServer(toCall, 10000000);
                NetworkTestHelper.WaitForOrTimeout(() => false, 2000);
            }
            catch (Exception e) {
                passed = true;
            }

            Assert.IsTrue(passed);

        }

        [TestMethod]
        public void TestConnectToServerError1() {
            SocketState state = new SocketState(null, null);

            void toCall(SocketState s) {
                state = s;
            }
            Networking.ConnectToServer(toCall, "bjknnjkljkn;sfghj[{{{{{adf <><>asdfdeb098u]]]]]xxx.<>", 2130);

            Assert.IsTrue(state.ErrorOccured);
        }

        [TestMethod]
        public void TestConnectToServerError2() {
            SocketState state = new SocketState(null, null);

            void toCall(SocketState s) {
                state = s;
            }

            Networking.ConnectToServer(toCall, null, 2131);

            Assert.IsTrue(state.ErrorOccured);
        }

        [TestMethod]
        public void TestConnectToServerError3() {

            string message = "this message here";
            bool firstTime = true;
            int times = 0;
            void toCall(SocketState s) {
                times++;
                testLocalSocketState = s;
                if (firstTime) {
                    firstTime = false;
                    throw new Exception(message);
                }

            }
            testListener = Networking.StartServer(s => { }, 2134);
            Networking.ConnectToServer(toCall, "localhost", 2134);

            NetworkTestHelper.WaitForOrTimeout(() => false, 3000);
            Assert.IsTrue(testLocalSocketState.ErrorOccured);
            Assert.IsTrue(testLocalSocketState.ErrorMessage.Equals(message));

        }

        [TestMethod]
        public void TestConnectToServerErrorNoServerExists() {

            void toCall(SocketState s) {
                testLocalSocketState = s;
            }
            Networking.ConnectToServer(toCall, "localhost", 2134);

            NetworkTestHelper.WaitForOrTimeout(() => false, 3000);
            Assert.IsTrue(testLocalSocketState.ErrorOccured);
        }

        [TestMethod]
        public void TestMultipleClientConnections() {
            SocketState state = new SocketState(null, null);
            int i = 0;
            int j = 0;
            void toServerCall(SocketState s) {
                i++;
            }
            void toClientCall(SocketState s) {
                j++;
                NetworkTestHelper.WaitForOrTimeout(() => false, 2000);
                Assert.IsTrue(i == j);
            }
            TcpListener listener = Networking.StartServer(toServerCall, 2112);

            Networking.ConnectToServer(toClientCall, "localhost", 2112);
            Networking.ConnectToServer(toClientCall, "localhost", 2112);
            Networking.ConnectToServer(toClientCall, "localhost", 2112);
            Networking.ConnectToServer(toClientCall, "localhost", 2112);
            Networking.ConnectToServer(toClientCall, "localhost", 2112);
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestCloseLocalThenSend(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2151);

            testLocalSocketState.TheSocket.Close();

            // No assertions, but the following should not result in an unhandled exception
            Assert.IsFalse(Networking.Send(testLocalSocketState.TheSocket, "a"));
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestSendAndClose(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2151);

            Assert.IsTrue(Networking.SendAndClose(testLocalSocketState.TheSocket, "a"));
            // No assertions, but the following should not result in an unhandled exception
            Assert.IsFalse(Networking.Send(testLocalSocketState.TheSocket, "a"));
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void TestCloseThenSendAndClose(bool clientSide) {
            SetupTestConnections(clientSide, out testListener, out testLocalSocketState, out testRemoteSocketState, 2151);

            testLocalSocketState.TheSocket.Close();

            Assert.IsFalse(Networking.SendAndClose(testLocalSocketState.TheSocket, "a"));
        }


        [TestMethod]
        public void TestStopServer() {

            NetworkTestHelper.SetupSingleConnectionTest(
                out testListener,
                out testRemoteSocketState,
                out testLocalSocketState, 2200);

            Networking.StopServer(testListener);

            SocketState newState = new SocketState(null, null);

            Networking.ConnectToServer(s => newState = s, "localhost", 2200);

            NetworkTestHelper.WaitForOrTimeout(() => false, 3000);

            Assert.IsTrue(newState.ErrorOccured);

        }

        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public void testGetDataConcurrency(bool clientSide) {
            SetupTestConnections(
                clientSide,
                out testListener,
                out testRemoteSocketState,
                out testLocalSocketState, 2356);

            string data = "";
            void waitForABit(SocketState s) {
                if (s.ErrorOccured)
                    return;

                data += s.GetData() + " ";
                s.RemoveData(0, s.GetData().Length);
                Networking.GetData(testRemoteSocketState);
            }

            testRemoteSocketState.OnNetworkAction = waitForABit;

            Networking.GetData(testRemoteSocketState);

            Networking.Send(testLocalSocketState.TheSocket, "HelloThere");
            //The first GetData call will take 5000
            //It will recieve the first message "HelloThere" and will concat that to the string
            //In that time another message "Pan" is sent that alters the socket state buffer

            NetworkTestHelper.WaitForOrTimeout(() => false, 500);

            Networking.Send(testLocalSocketState.TheSocket, "Pan");

            NetworkTestHelper.WaitForOrTimeout(() => false, 2000);

            Assert.AreEqual("HelloThere Pan ", data);
        }

        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public void testMultipleSends(bool clientSide) {
            SetupTestConnections(
                clientSide,
                out testListener,
                out testRemoteSocketState,
                out testLocalSocketState, 2380);

            string data = "";
            void ProcessMessages(SocketState s) {
                data += s.GetData() + " ";
                s.RemoveData(0, s.GetData().Length);
                if (!s.ErrorOccured) {
                    Networking.GetData(testRemoteSocketState);
                }
            }
            testRemoteSocketState.OnNetworkAction = ProcessMessages;
            bool firstSent;
            bool secondSent;
            firstSent = Networking.Send(testLocalSocketState.TheSocket, "HelloThere");
            secondSent = Networking.Send(testLocalSocketState.TheSocket, "Pan");

            Networking.GetData(testRemoteSocketState);
            NetworkTestHelper.WaitForOrTimeout(() => false, 3000);
            Assert.IsTrue(firstSent && secondSent);
            Assert.AreEqual("HelloThere Pan ", data);
        }

        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public void GetDataErrorCatching(bool clientSide) {
            SetupTestConnections(
                clientSide,
                out testListener,
                out testRemoteSocketState,
                out testLocalSocketState, 2380);

            string data = "";
            void ProcessMessages(SocketState s) {
                testRemoteSocketState = s;

                if (s.ErrorOccured)
                    return;

                data += s.GetData() + " ";
                s.RemoveData(0, s.GetData().Length);
            }
            testRemoteSocketState.OnNetworkAction = ProcessMessages;
            bool firstSent;
            bool secondSent;
            firstSent = Networking.Send(testLocalSocketState.TheSocket, "HelloThere");
            secondSent = Networking.Send(testLocalSocketState.TheSocket, "Pan");

            testRemoteSocketState.TheSocket.Close();
            Networking.GetData(testRemoteSocketState);
            NetworkTestHelper.WaitForOrTimeout(() => false, 3000);

            Assert.IsTrue(testRemoteSocketState.ErrorOccured);
        }
    }
}
