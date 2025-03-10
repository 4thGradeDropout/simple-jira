using System;
using System.Text;
using NUnit.Framework;

namespace SimpleJira.Tests
{
    [TestFixture]
    [NonParallelizable]
    public abstract class TestBase
    {
        [SetUp]
        public void ActualSetUp()
        {
            try
            {
                SetUp();
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine(ex);
                    TearDown();
                }
                catch (Exception e)
                {
                    Console.WriteLine("teardown exception: " + e);
                }

                throw;
            }
        }

        [TearDown]
        protected virtual void TearDown()
        {
        }

        static TestBase()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [OneTimeSetUp]
        protected virtual void TestFixtureSetup()
        {
        }

        [OneTimeTearDown]
        protected virtual void TestFixtureTearDown()
        {
        }

        protected virtual void SetUp()
        {
        }
        
    }
}