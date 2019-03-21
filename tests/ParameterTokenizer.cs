using Rql.NET;
using Xunit;

namespace tests
{
    public class TokenizerTests
    {
        [Fact]
        public void IndexTokenizer()
        {
            var tokenizer = new IndexTokenizer();
            var t1 = tokenizer.GetToken("t", null);
            Assert.Equal(t1, "@1");

            var t2 = tokenizer.GetToken("t", null);
            Assert.Equal(t2, "@2");
        }

        [Fact]
        public void NamedTokenizer()
        {
            var tokenizer = new NamedTokenizer();
            var t1 = tokenizer.GetToken("t", null);
            Assert.Equal(t1, "@t");

            var t2 = tokenizer.GetToken("t", null);
            Assert.Equal(t2, "@t2");
        }
    }
}