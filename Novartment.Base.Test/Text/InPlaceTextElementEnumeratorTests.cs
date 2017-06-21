using System;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class InPlaceTextElementEnumeratorTests
	{
		[Fact]
		[Trait ("Category", "Text.InPlaceTextElementEnumerator")]
		public void Enumerate ()
		{
			var enumerator = new InPlaceTextElementEnumerator (string.Empty);
			Assert.False (enumerator.MoveNext ());

			var template = new string (new char[]
			{
				'\u0020',
				'\u0073',
				'\u0416',
				'\u0BF5',
				'\uD858', '\uDE18', // "𦈘" один символ из двух char,
				'\u0063', '\u0301', '\u0327', // "ḉ" один символ из трёх char,
				'\uD834', '\uDD1E', // один символ из двух char
				'\u200B',
				'\u200A',
				'\u2009',
				'\u2001',
				'\u00A0',
				'\u0070',
			});
			enumerator = new InPlaceTextElementEnumerator (template, 1, template.Length - 2);

			Assert.True (enumerator.MoveNext ());
			Assert.Equal (1, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (2, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (3, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (4, enumerator.CurrentPosition);
			Assert.Equal (2, enumerator.CurrentLength);

			enumerator.Reset ();
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (1, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (2, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (3, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (4, enumerator.CurrentPosition);
			Assert.Equal (2, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (6, enumerator.CurrentPosition);
			Assert.Equal (3, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (9, enumerator.CurrentPosition);
			Assert.Equal (2, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (11, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (12, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (13, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (14, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.True (enumerator.MoveNext ());
			Assert.Equal (15, enumerator.CurrentPosition);
			Assert.Equal (1, enumerator.CurrentLength);
			Assert.False (enumerator.MoveNext ());
		}
	}
}
