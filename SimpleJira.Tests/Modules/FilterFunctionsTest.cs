using NUnit.Framework;
using SimpleJira.Fakes.Impl.Jql.Compiler;

namespace SimpleJira.Tests.Modules
{
    public class FilterFunctionsTest : TestBase
    {
        [Test]
        public void Search()
        {
            Assert.That(FilterFunctions.Search("строка", "строка"));
            Assert.That(FilterFunctions.Search("стРОка", "СТроКА"));
            Assert.That(FilterFunctions.Search("строка", "строкд"), Is.False);
            Assert.That(FilterFunctions.Search("строка", "строк"));
            Assert.That(FilterFunctions.Search("одна строка", "строка"));
            Assert.That(FilterFunctions.Search("одна строка", "за строка"));
            Assert.That(FilterFunctions.Search("одна строка", "за"), Is.False);
            Assert.That(FilterFunctions.Search("одна строка", "одн*"));
            Assert.That(FilterFunctions.Search("одна строка", "одн* строк"));
            Assert.That(FilterFunctions.Search("одна строка", "одн* строк"));
            Assert.That(FilterFunctions.Search("одна строка", "/одна, строк"));
            Assert.That(FilterFunctions.Search("одна строка", "одна,строк"));
            Assert.That(FilterFunctions.Search("одна строка", "одна.строк"), Is.False);
        }

        [Test]
        public void SearchDirectly()
        {
            Assert.That(FilterFunctions.SearchDirectly("строка", "строка"));
            Assert.That(FilterFunctions.SearchDirectly("стРОка", "СТроКА"));
            Assert.That(FilterFunctions.SearchDirectly("строка", "строк"), Is.False);
            Assert.That(FilterFunctions.SearchDirectly("строка число", "строка число"));
            Assert.That(FilterFunctions.SearchDirectly("строка число", "строка,число"));
            Assert.That(FilterFunctions.SearchDirectly("строка,число", "строка число"));
            Assert.That(FilterFunctions.SearchDirectly("строка число", "строка числ"), Is.False);
            Assert.That(FilterFunctions.SearchDirectly("строка число", "строка за число"), Is.False);
        }
    }
}