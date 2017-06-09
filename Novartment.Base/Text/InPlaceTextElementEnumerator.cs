using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Перечисляет текстовые элементы строки.
	/// Текстовый элемент - это единица текста которая отображается как одиночный символ (графема).
	/// </summary>
	/// <remarks>
	/// Отличие от библиотечного TextElementEnumerator в отсутствии генерации строк для каждого найденного элемента.
	/// </remarks>
	public class InPlaceTextElementEnumerator
	{
		private readonly string _source;
		private readonly int _startIndex;
		private readonly int _endIndex;
		private int _currentIndex;
		private UnicodeCategory _currentCategory;
		private int _currentCharCount;
		private int _currentElementLength;

		/// <summary>
		/// Инициализирует новый экземпляр класса InPlaceTextElementEnumerator для перечисления элементов указанной строки.
		/// </summary>
		/// <param name="source">Строка, элементы которой будут перечислены.</param>
		public InPlaceTextElementEnumerator (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			_source = source;
			_startIndex = 0;
			_endIndex = source.Length;
			Reset ();
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса InPlaceTextElementEnumerator для перечисления элементов указанной строки.
		/// </summary>
		/// <param name="source">Строка, элементы которой будут перечислены.</param>
		/// <param name="index">Начальная позиция в строке для перечисления.</param>
		/// <param name="count">Количество элементов в строке для перечисления.</param>
		public InPlaceTextElementEnumerator (string source, int index, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if ((index < 0) || (index > source.Length) || ((index == source.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}

			if ((count < 0) || ((index + count) > source.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}

			Contract.EndContractBlock ();

			_source = source;
			_startIndex = index;
			_endIndex = index + count;
			Reset ();
		}

		/// <summary>
		/// Получает позицию текущего элемента строки.
		/// </summary>
		public int CurrentPosition
		{
			get
			{
				if (_currentIndex == _startIndex)
				{
					throw new InvalidOperationException ("Can not get current element of enumeration because it not started.");
				}

				if (_currentIndex == (_endIndex + 1))
				{
					throw new InvalidOperationException ("Can not get current element of enumeration because it already ended.");
				}

				return _currentIndex - _currentElementLength;
			}
		}

		/// <summary>
		/// Получает размер (количество символов) текущего элемента строки.
		/// </summary>
		public int CurrentLength
		{
			get
			{
				if (_currentIndex == _startIndex)
				{
					throw new InvalidOperationException ("Can not get current element of enumeration because it not started.");
				}

				if (_currentIndex == (_endIndex + 1))
				{
					throw new InvalidOperationException ("Can not get current element of enumeration because it already ended.");
				}

				return _currentElementLength;
			}
		}

		/// <summary>
		/// Перемещает перечислитель к следующему элементу строки.
		/// </summary>
		/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
		/// false, если перечислитель достиг конца строки.</returns>
		public bool MoveNext ()
		{
			if (_currentIndex >= _endIndex)
			{
				_currentIndex = _endIndex + 1;
				return false;
			}

			var currentIndex = _currentIndex;
			if ((currentIndex + _currentCharCount) == _endIndex)
			{
				_currentElementLength = _currentCharCount;
			}
			else
			{
				var nextCharCount = char.IsSurrogatePair (_source, currentIndex + _currentCharCount) ? 2 : 1;
				var nextCategory = CharUnicodeInfo.GetUnicodeCategory (_source, currentIndex + _currentCharCount);
				if ((
					(!IsCombiningCategory (nextCategory) || IsCombiningCategory (_currentCategory)) ||
					((_currentCategory == UnicodeCategory.Format) || (_currentCategory == UnicodeCategory.Control))) ||
					((_currentCategory == UnicodeCategory.OtherNotAssigned) || (_currentCategory == UnicodeCategory.Surrogate)))
				{
					var oldCharCount = _currentCharCount;
					_currentCategory = nextCategory;
					_currentCharCount = nextCharCount;
					_currentElementLength = oldCharCount;
				}
				else
				{
					var startIndex = currentIndex;
					currentIndex += _currentCharCount + nextCharCount;
					while (currentIndex < _endIndex)
					{
						nextCharCount = char.IsSurrogatePair (_source, currentIndex) ? 2 : 1;
						nextCategory = CharUnicodeInfo.GetUnicodeCategory (_source, currentIndex);
						var isCombiningCategory = IsCombiningCategory (nextCategory);
						if (!isCombiningCategory)
						{
							_currentCategory = nextCategory;
							_currentCharCount = nextCharCount;
							break;
						}

						currentIndex += nextCharCount;
					}

					_currentElementLength = currentIndex - startIndex;
				}
			}

			_currentIndex += _currentElementLength;
			return true;
		}

		/// <summary>
		/// Устанавливает перечислитель в его начальное положение, перед первым элементом строки.
		/// </summary>
		public void Reset ()
		{
			_currentIndex = _startIndex;
			if (_currentIndex < _endIndex)
			{
				_currentCharCount = char.IsSurrogatePair (_source, _currentIndex) ? 2 : 1;
				_currentCategory = CharUnicodeInfo.GetUnicodeCategory (_source, _currentIndex);
			}
		}

		private static bool IsCombiningCategory (UnicodeCategory category)
		{
			if ((category != UnicodeCategory.NonSpacingMark) && (category != UnicodeCategory.SpacingCombiningMark))
			{
				return category == UnicodeCategory.EnclosingMark;
			}

			return true;
		}
	}
}
