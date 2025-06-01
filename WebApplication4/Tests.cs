using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using WebApplication4.Models;
using WebApplication4.Services;

namespace EmailProcessing.Tests
{
    [TestClass]
    public class MessageProcessorTests
    {
        private readonly MessageProcessor _processor = new MessageProcessor();

        [TestMethod]
        public void ProcessMessage_NoTargetDomains_ReturnsOriginalMessage()
        {
            var message = new Message
            {
                To = "test@gmail.com",
                Copy = "123@ya.ru"
            };

            var result = _processor.ProcessMessage(message);

            Assert.AreEqual(message.To, result.To);
            Assert.AreEqual(message.Copy, result.Copy);
        }

        [TestMethod]
        public void ProcessMessage_WithTargetDomainNoExceptions_AddsRequiredEmails()
        {
            var message = new Message
            {
                To = "user@alfa.com",
                Copy = "other@business.com"
            };

            var result = _processor.ProcessMessage(message);

            Assert.IsTrue(result.Copy.Contains("v.vladislavovich@alfa.com"));
            Assert.IsTrue(result.Copy.Contains("other@business.com"));
        }

        [TestMethod]
        public void ProcessMessage_WithExceptionEmail_RemovesAddedEmails()
        {
            var message = new Message
            {
                To = "i.ivanov@tbank.ru", 
                Copy = "t.tbankovich@tbank.ru; norm@mail.com"
            };

            var result = _processor.ProcessMessage(message);

            Assert.IsFalse(result.Copy.Contains("t.tbankovich@tbank.ru"));
            Assert.IsTrue(result.Copy.Contains("norm@mail.com"));
        }

        [TestMethod]
        public void ProcessMessage_EmailInTo_NotAddedToCopy()
        {
            var message = new Message
            {
                To = "v.vladislavovich@alfa.com", 
                Copy = "other@mail.com"
            };

            var result = _processor.ProcessMessage(message);

            Assert.IsFalse(result.Copy.Contains("v.vladislavovich@alfa.com"));
            Assert.IsTrue(result.Copy.Contains("other@mail.com"));
        }

        [TestMethod]
        public void ProcessMessage_MultipleDomains_AllCorrect()
        {
            var message = new Message
            {
                To = "user1@alfa.com; user2@vtb.ru",
                Copy = "user3@tbank.ru"
            };

            var result = _processor.ProcessMessage(message);

            Assert.IsTrue(result.Copy.Contains("v.vladislavovich@alfa.com"));
            Assert.IsTrue(result.Copy.Contains("a.aleksandrov@vtb.ru"));
            Assert.IsTrue(result.Copy.Contains("user3@tbank.ru"));
        }

        [TestMethod]
        public void ProcessMessage_WithExceptionInCopy_RemovesAddedEmails()
        {
            var message = new Message
            {
                To = "normal@mail.com",
                Copy = "s.sergeev@alfa.com" 
            };

            var result = _processor.ProcessMessage(message);

            Assert.IsFalse(result.Copy.Contains("v.vladislavovich@alfa.com"));
            Assert.AreEqual("normal@mail.com", result.To);
        }

        [TestMethod]
        public void ProcessMessage_EmptyCopy_AddsEmail()
        {
            var message = new Message
            {
                To = "user@alfa.com",
                Copy = null
            };

            var result = _processor.ProcessMessage(message);

            Assert.AreEqual("v.vladislavovich@alfa.com", result.Copy);
        }

        [TestMethod]
        public void ProcessMessage_MixedCase_CaseInsensetive()
        {
            var message = new Message
            {
                To = "USER@ALFA.COM", 
                Copy = "other@mail.com"
            };

            var result = _processor.ProcessMessage(message);

            Assert.IsTrue(result.Copy.Contains("v.vladislavovich@alfa.com"));
        }
    }
}