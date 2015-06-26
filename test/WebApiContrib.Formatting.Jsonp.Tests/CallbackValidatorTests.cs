using NUnit.Framework;

namespace WebApiContrib.Formatting.Jsonp.Tests
{
    [TestFixture]
    class CallbackValidatorTests
    {
        [Test]
        [TestCase("jQuery12332434", true)]
        [TestCase("some.callback.into.deep.object", true)]
        [TestCase("<img src=\"\">", false)]
        [TestCase("alert('x'); //", false)]
        [TestCase("window.location = 'x'", false)]
        public void IsValid(string callback, bool isValid)
        {
            Assert.That(CallbackValidator.IsValid(callback), Is.EqualTo(isValid));
        }
    }
}
