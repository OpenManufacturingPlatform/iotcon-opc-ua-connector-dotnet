using System;
using NUnit.Framework;
using OMP.Connector.Domain.Extensions;

namespace OMP.Connector.Domain.Tests.Extensions
{
    [TestFixture]
    public class ExceptionExtensionsTest
    {
        [Test]
        public void GetMessage_Should_Return_All_InnerException_Messages()
        {
            var outerMsg = "outer";
            var inner1Msg = "inner1";
            var inner2Msg = "inner2";
            var inner3Msg = "inner3";
            var expectedMessage = $"{outerMsg}{Constants.ErrorMessageSeparator}{inner1Msg}{Constants.ErrorMessageSeparator}{inner2Msg}{Constants.ErrorMessageSeparator}{inner3Msg}";

            var inner3Exception = new Exception(inner3Msg);
            var inner2Exception = new Exception(inner2Msg, inner3Exception);
            var inner1Exception = new Exception(inner1Msg, inner2Exception);
            var outerException = new Exception(outerMsg, inner1Exception);

            var actualMessage = outerException.GetMessage();

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [Test]
        public void GetMessage_Should_Only_Return_OuterException_Message_If_Null_InnerException()
        {
            var outerMsg = "outer";

            var outerException = new Exception(outerMsg);
            var actualMessage = outerException.GetMessage();

            Assert.AreEqual(outerMsg, actualMessage);
        }
    }
}