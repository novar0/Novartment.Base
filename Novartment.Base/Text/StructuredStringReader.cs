using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Последовательный считыватель элементов строки, позволяющий читать одиночные знаки,
	/// последовательности знаков определённого класса,
	/// либо последовательности ограниченные определёнными знаками.
	/// </summary>
	/// <remarks>
	/// Все члены, принимающие или возвращающие отдельные знаки, используют кодировку UTF-32.
	/// Суррогатная пара (char, char) считается одним знаком.
	/// </remarks>
	public class StructuredStringReader
	{
		private readonly string _source;
		private readonly int _endPos = 0;
		private int _currentPos = 0;

		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredStringReader на основе указанной строки.
		/// </summary>
		/// <param name="source">Строка для разбора.</param>
		public StructuredStringReader (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			_source = source;
			_currentPos = 0;
			_endPos = source.Length;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredStringReader на основе указанной строки.
		/// </summary>
		/// <param name="source">Строка для разбора.</param>
		/// <param name="index">Начальная позиция в строке для разбора.</param>
		/// <param name="count">Количество символов в строке для разбора.</param>
		public StructuredStringReader (string source, int index, int count)
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
			_currentPos = index;
			_endPos = index + count;
		}

		/// <summary>Исходная разбираемая строка.</summary>
		public string Source => _source;

		/// <summary>Текущая позиция в разбираемой строке.</summary>
		public int Position => _currentPos;

		/// <summary>True если достигнут конец разбираемой строки, иначе False.</summary>
		public bool IsExhausted => _currentPos >= _endPos;

		/// <summary>Получает код знака (в UTF-32) в текущей позиции в разбираемой строке.
		/// Получает -1 если достигнут конец разбираемой строки.</summary>
		public int NextCodePoint
		{
			get
			{
				var pos = _currentPos;
				return GetCodePoint (ref pos);
			}
		}

		/// <summary>Получает код знака (в UTF-32) в следующей за текущей позицией в разбираемой строке.
		/// Получает -1 если достигнут конец разбираемой строки.</summary>
		public int NextNextCodePoint
		{
			get
			{
				var pos = _currentPos;
				GetCodePoint (ref pos);
				return GetCodePoint (ref pos);
			}
		}

		/// <summary>
		/// Пропустить знак.
		/// </summary>
		/// <returns>Код знака (в UTF-32) если он пропущен, иначе -1 если достигнут конец строки.</returns>
		public int SkipCodePoint ()
		{
			return GetCodePoint (ref _currentPos);
		}

		/// <summary>
		/// Проверяет что знак в текущей позиции совпадает с указанным и пропускает его.
		/// Если символ не совпадает, то генерируется исключение.
		/// </summary>
		/// <param name="utf32CodePoint">Код знака (в UTF-32) для проверки.</param>
		public void EnsureCodePoint (int utf32CodePoint)
		{
			if (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var currentCodePoint = GetCodePoint (ref pos);
				if (currentCodePoint == utf32CodePoint)
				{
					_currentPos = pos;
				}
			}

			throw new FormatException (FormattableString.Invariant (
				$"Expected char code '{utf32CodePoint}' not found at position {_currentPos} in string '{(_source.Length > 100 ? _source.Substring (0, 100) + "..." : _source)}'."));
		}

		/// <summary>
		/// Пропускает элемент с указанными параметрами.
		/// Если указанный элемент не найден, то генерируется исключение.
		/// </summary>
		/// <param name="delimitedElement">Параметры пропускаемого элемента.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int EnsureDelimitedElement (DelimitedElement delimitedElement)
		{
			if (delimitedElement == null)
			{
				throw new ArgumentNullException (nameof (delimitedElement));
			}

			Contract.EndContractBlock ();

			if (_currentPos >= _endPos)
			{
				throw new FormatException ("Expected element not found to the end of string.");
			}

			if (delimitedElement.FixedLength > 0)
			{
				if (this.NextCodePoint != delimitedElement.StartChar)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Expected char code '{delimitedElement.StartChar}' not found at position {_currentPos} in string '{(_source.Length > 100 ? _source.Substring (0, 100) + "..." : _source)}'."));
				}

				var newOffset = _currentPos + delimitedElement.FixedLength;
				if (newOffset >= _endPos)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Unexpected end of fixed-length element {delimitedElement.FixedLength} at position {_currentPos} in string '{(_source.Length > 100 ? _source.Substring (0, 100) + "..." : _source)}'."));
				}

				_currentPos = newOffset;
			}
			else
			{
				EnsureCodePoint (delimitedElement.StartChar);
				var start = _currentPos;
				int nestingLevel = 0;
				var ignoreElement = delimitedElement.IgnoreElement;
				while (nestingLevel >= 0)
				{
					if (_currentPos >= _endPos)
					{
						throw new FormatException (FormattableString.Invariant (
							$"Ending char '{delimitedElement.EndChar}' not found in element, started at position {start} in string '{(_source.Length > 100 ? _source.Substring (0, 100) + "..." : _source)}'."));
					}

					if ((ignoreElement != null) && (this.NextCodePoint == ignoreElement.StartChar))
					{
						EnsureDelimitedElement (ignoreElement);
					}
					else
					{
						var isStartNested = delimitedElement.AllowNesting && (this.NextCodePoint == delimitedElement.StartChar);
						if (isStartNested)
						{
							SkipCodePoint ();
							nestingLevel++;
						}
						else
						{
							var nextChar = this.SkipCodePoint ();
							if (nextChar == delimitedElement.EndChar)
							{
								nestingLevel--;
							}
						}
					}
				}
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает символы указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipClassChars (IReadOnlyList<bool> charClassTable)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			if (_currentPos >= _endPos)
			{
				throw new FormatException ("Chars of specified class not found to the end of string.");
			}

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character >= charClassTable.Count) || !charClassTable[character])
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает символы указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Пропускаемый тип символов.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipClassChars (IReadOnlyList<byte> charClassTable, byte suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			if (_currentPos >= _endPos)
			{
				throw new FormatException ("Chars of specified class not found to the end of string.");
			}

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character >= charClassTable.Count) || ((charClassTable[character] & suitableClassMask) == 0))
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает символы указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Пропускаемый тип символов.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipClassChars (IReadOnlyList<short> charClassTable, short suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			if (_currentPos >= _endPos)
			{
				throw new FormatException ("Chars of specified class not found to the end of string.");
			}

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character >= charClassTable.Count) || ((charClassTable[character] & suitableClassMask) == 0))
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает символы указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Пропускаемый тип символов.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipClassChars (IReadOnlyList<int> charClassTable, int suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			if (_currentPos >= _endPos)
			{
				throw new FormatException ("Chars of specified class not found to the end of string.");
			}

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character >= charClassTable.Count) || ((charClassTable[character] & suitableClassMask) == 0))
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает символы указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Пропускаемый тип символов.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipClassChars (IReadOnlyList<long> charClassTable, long suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			if (_currentPos >= _endPos)
			{
				throw new FormatException ("Chars of specified class not found to the end of string.");
			}

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character >= charClassTable.Count) || ((charClassTable[character] & suitableClassMask) == 0))
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает все символы кроме указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipNotClassChars (IReadOnlyList<bool> charClassTable)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character < charClassTable.Count) && charClassTable[character])
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает все символы кроме указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Тип символов, которые не пропускаются.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipNotClassChars (IReadOnlyList<byte> charClassTable, byte suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character < charClassTable.Count) && ((charClassTable[character] & suitableClassMask) != 0))
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает все символы кроме указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Тип символов, которые не пропускаются.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipNotClassChars (IReadOnlyList<short> charClassTable, short suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character < charClassTable.Count) && ((charClassTable[character] & suitableClassMask) != 0))
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает все символы кроме указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Тип символов, которые не пропускаются.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipNotClassChars (IReadOnlyList<int> charClassTable, int suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character < charClassTable.Count) && ((charClassTable[character] & suitableClassMask) != 0))
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		/// <summary>
		/// Пропускает все символы кроме указанного типа.
		/// </summary>
		/// <param name="charClassTable">Таблица типов символов.</param>
		/// <param name="suitableClassMask">Тип символов, которые не пропускаются.</param>
		/// <returns>Текущая позиция в разбираемой строке после пропуска.</returns>
		public int SkipNotClassChars (IReadOnlyList<long> charClassTable, long suitableClassMask)
		{
			if (charClassTable == null)
			{
				throw new ArgumentNullException (nameof (charClassTable));
			}

			Contract.EndContractBlock ();

			while (_currentPos < _endPos)
			{
				var pos = _currentPos;
				var character = GetCodePoint (ref pos);

				if ((character < charClassTable.Count) && ((charClassTable[character] & suitableClassMask) != 0))
				{
					break;
				}

				_currentPos = pos;
			}

			return _currentPos;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private int GetCodePoint (ref int index)
		{
			if (index >= _endPos)
			{
				return -1;
			}

			var highSurrogate = _source[index++];
			if ((index >= _endPos) || (highSurrogate < 0xd800) || (highSurrogate > 0xdbff))
			{
				return highSurrogate;
			}

			var lowSurrogate = _source[index];
			if ((lowSurrogate < 0xdc00) || (lowSurrogate > 0xdfff))
			{
				return highSurrogate;
			}

			index++;
			return ((highSurrogate - '\ud800') * 0x400) + (lowSurrogate - '\udc00') + 0x10000;
		}
	}
}
