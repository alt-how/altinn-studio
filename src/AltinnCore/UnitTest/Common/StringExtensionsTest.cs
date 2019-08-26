using System;
using System.Collections.Generic;
using System.Text;
using AltinnCore.Common.Helpers.Extensions;
using Xunit;

namespace AltinnCore.UnitTest.Common
{
    /// <summary>
    /// String Extensions related tests
    /// </summary>
    public class StringExtensionsTest
    {
        /// <summary>
        /// Proper santization when using ToFileName without exceptions.
        /// </summary>
        [Fact]
        public void ToFileNameReplaceSlashWithDash()
        {
            var invalidFileNames = new string[] { "/test/", "\\test\\" };

            foreach (var invalidFileName in invalidFileNames)
            {
                var sanitizedName = invalidFileName.ToFileName(throwExceptionOnInvalidCharacters: false);
                Assert.Equal("-test-", sanitizedName);
            }
        }

        /// <summary>
        /// Throws exception when an invalid character is encountered.
        /// </summary>
        [Fact]
        public void ToFileNameThrowsException()
        {
            var invalidFileNames = new string[] { "/test/", "\\test\\" };

            foreach (var invalidFileName in invalidFileNames)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => invalidFileName.ToFileName());
            }
        }
    }
}
