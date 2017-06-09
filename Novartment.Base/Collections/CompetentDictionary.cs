using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Threading;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// Потокобезопасный словарь только для чтения, автоматически создающий записи при обращении к ним
	/// и генерирующий уведомления о добавлении.
	/// Записи создаются с помощью указанной фабрики по производству значений.
	/// </summary>
	/// <typeparam name="TKey">Тип ключей в словаре.</typeparam>
	/// <typeparam name="TValue">Тип значений в словаре.</typeparam>
	/// <remarks>
	/// Особенности:
	/// * свойства Keys и Values предоставляются в виде списка с доступом по номеру позиции без блокировок и с уведомлением о добавлениях;
	/// * никогда не порождает KeyNotFoundException;
	/// * TryGetValue() всегда возвращает true;
	/// * единственный метод, который возвращает реальную информацию о наличии записи - ContainsKey().
	/// Порождает событие CollectionChanged только вида NotifyCollectionChangedAction.Add.
	/// </remarks>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	[SuppressMessage (
		"Microsoft.Design",
		"CA1063:ImplementIDisposableCorrectly",
		Justification = "Implemented correctly.")]
	public class CompetentDictionary<TKey, TValue> :
		IReadOnlyDictionary<TKey, TValue>,
		INotifyCollectionChanged,
		IDisposable
	{
		// блокировки и синхронизации основаны на нескольких предположениях:
		// * _entries изменяется только одним способом: заменяется на копию (содержащую теже объекты), отличающуюся неиспользуемым запасом в размере
		// * _count изменяется только одним способом: инкрементируется после добавления нового значения в _entries
		// * _buckets требует читающий захват _bucketsLock для чтения и эксклюзивный захват - для изменения
		// * _valueFactory вызывается внутри эксклюзивного захвата _bucketsLock
		//
		// поэтому:
		// доступ к _entries по индексу (от 0 до _count) не требует никаких блокировок, то есть все IEnumerator не требуют никакой блокировки
		// медленная фабрика _valueFactory будет значительно блокировать все операции, а сложная может привести к взаимным блокировкам
		// можно посылать уведомления с помощью NotifyCollectionChangedEventArgs (в них требуется указание индекса)
		private readonly int _lockTimeoutMilliseconds = 1000;
		private readonly string _lockReadTimeoutMessage = "Elapsed timeout waiting acquiring lock for read data operation. Probably it is deadlock.";
		private readonly string _lockWriteTimeoutMessage = "Elapsed timeout waiting acquiring lock for write data operation. Probably it is deadlock.";
		private readonly IEqualityComparer<TKey> _comparer;
		private readonly Func<TKey, TValue> _valueFactory;
		private readonly ReaderWriterLockSlim _bucketsLock = new ReaderWriterLockSlim ();
		private readonly KeyEnumerationProvider _keyEnumerationProvider; // ссылка на внутренний класс
		private readonly ValueEnumerationProvider _valueEnumerationProvider; // ссылка на внутренний класс
		private Entry[] _entries;
		private int[] _buckets;
		private int _count;

		/// <summary>
		/// Инициализирует новый экземпляр класса CompetentDictionary,
		/// использующий указанную фабрику значений и компаратор ключей.
		/// </summary>
		/// <param name="valueFactory">
		/// Функция-фабрика значений для записей словаря,
		/// не должна содержать обращений к собственно словарю,
		/// должна быть быстрой и не содержать каких либо блокировок.
		/// </param>
		/// <param name="comparer">
		/// Компаратор, который будет использоваться при сравнении ключей,
		/// или null, чтобы использовать реализацию компаратора по умолчанию.
		/// </param>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public CompetentDictionary (Func<TKey, TValue> valueFactory, IEqualityComparer<TKey> comparer = null)
		{
			if (valueFactory == null)
			{
				throw new ArgumentNullException (nameof (valueFactory));
			}

			Contract.EndContractBlock ();

			_keyEnumerationProvider = new KeyEnumerationProvider (this);
			_valueEnumerationProvider = new ValueEnumerationProvider (this);
			_comparer = comparer ?? EqualityComparer<TKey>.Default;
			_valueFactory = valueFactory;
		}

		/// <summary>Происходит, когда запись добавляется в словарь.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>Получает число записей, которое содержится в словаре.</summary>
		/// <returns>Число записей, содержащихся в словаре.</returns>
		public int Count => _count;

		/// <summary>Получает коллекию ключей словаря.</summary>
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _keyEnumerationProvider;

		/// <summary>Получает список, содержащий ключи из словаря.</summary>
		/// <returns>Список System.Collections.Generic.IReadOnlyList&lt;TKey&gt;, содержащий ключи из словаря.</returns>
		public IReadOnlyList<TKey> Keys => _keyEnumerationProvider;

		/// <summary>Получает коллекию значений словаря.</summary>
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _valueEnumerationProvider;

		/// <summary>Получает список, содержащий значения из словаря.</summary>
		/// <returns>Список System.Collections.Generic.IReadOnlyList&lt;TValue&gt;, содержащий значения из словаря.</returns>
		public IReadOnlyList<TValue> Values => _valueEnumerationProvider;

		/// <summary>Возвращает значение, связанное с указанным ключом.</summary>
		/// <param name="key">Ключ записи, значение которой требуется получить.</param>
		/// <returns>Значение в записи словаря, найденной по указанному ключу.</returns>
		public TValue this[TKey key]
		{
			get
			{
				if (key == null)
				{
					throw new ArgumentNullException(nameof(key));
				}

				Contract.EndContractBlock();

				return FindOrCreateValue(key);
			}
		}

		/// <summary>Определяет, содержит ли словарь указанную запись.</summary>
		/// <param name="keyValuePair">Структура KeyValuePair&lt;TKey, TValue&gt;, которую требуется найти в словаре.</param>
		/// <returns>Значение true, если объект keyValuePair найден в словаре в противном случае — значение false.</returns>
		public bool Contains (KeyValuePair<TKey, TValue> keyValuePair)
		{
			if (keyValuePair.Key == null)
			{
				throw new ArgumentOutOfRangeException (nameof (keyValuePair));
			}

			Contract.EndContractBlock ();

			var value = FindOrCreateValue (keyValuePair.Key);
			return EqualityComparer<TValue>.Default.Equals (value, keyValuePair.Value);
		}

		/// <summary>Определяет, содержится ли указанный ключ в словаре.</summary>
		/// <param name="key">Ключ, который требуется найти в словаре.</param>
		/// <returns>
		/// Значение True, если словарь содержит элемент с указанным ключом;
		/// в противном случае — значение False.
		/// </returns>
		public bool ContainsKey (TKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException (nameof (key));
			}

			Contract.EndContractBlock ();

			var isLockEntered = _bucketsLock.TryEnterReadLock (_lockTimeoutMilliseconds);
			if (!isLockEntered)
			{
				throw new TimeoutException (_lockReadTimeoutMessage);
			}

			try
			{
				return FindEntry (key) >= 0;
			}
			finally
			{
				_bucketsLock.ExitReadLock ();
			}
		}

		/// <summary>Получает значение, связанное с указанным ключом.</summary>
		/// <param name="key">Ключ значения, которое необходимо получить.</param>
		/// <param name="value">Возвращаемое значение, связанное с указанном ключом, если он найден;
		/// в противном случае — значение по умолчанию для данного типа параметра value.
		/// Этот параметр передается неинициализированным.</param>
		/// <returns>
		/// Значение true, если словарь содержит элемент с указанным ключом;
		/// в противном случае — значение false.
		/// </returns>
		public bool TryGetValue (TKey key, out TValue value)
		{
			if (key == null)
			{
				throw new ArgumentNullException (nameof (key));
			}

			Contract.EndContractBlock ();
			value = FindOrCreateValue (key);
			return true;
		}

		/// <summary>
		/// Получает перечислитель элементов словаря.
		/// </summary>
		/// <returns>Перечислитель элементов словаря.</returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			return new KeyValuePairEnumerator (this);
		}

		/// <summary>
		/// Получает перечислитель элементов словаря.
		/// </summary>
		/// <returns>Перечислитель элементов словаря.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>Освобождает все внутренние ресурсы и отключает подписчиков на события.</summary>
		[SuppressMessage (
		"Microsoft.Usage",
			"CA1816:CallGCSuppressFinalizeCorrectly",
			Justification = "There is no meaning to introduce a finalizer in derived type.")]
		[SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public void Dispose ()
		{
			CollectionChanged = null;
			_keyEnumerationProvider.Dispose ();
			_valueEnumerationProvider.Dispose ();
			_bucketsLock.Dispose ();
		}

		private static int[] CreateIndex (Entry[] entries, int count)
		{
			var size = entries.Length;
			var buckets = new int[size];
			for (var i = 0; i < buckets.Length; i++)
			{
				buckets[i] = -1;
			}

			for (var i = 0; i < count; i++)
			{
				var index = entries[i].Hash % size;
				entries[i].Next = buckets[index];
				buckets[index] = i;
			}

			return buckets;
		}

		// вызов этого метода обязательно должен быть внутри захваченного на чтение _bucketsLock
		private int FindEntry (TKey key)
		{
			if (_buckets != null)
			{
				var num = _comparer.GetHashCode (key) & int.MaxValue;
				for (var i = _buckets[num % _buckets.Length]; i >= 0; i = _entries[i].Next)
				{
					var isEntryEqualsKey = (_entries[i].Hash == num) && _comparer.Equals (_entries[i].Key, key);
					if (isEntryEqualsKey)
					{
						return i;
					}
				}
			}

			return -1;
		}

		private TValue FindOrCreateValue (TKey key)
		{
			int foundIndex;
			var isLockEntered = _bucketsLock.TryEnterReadLock (_lockTimeoutMilliseconds);
			if (!isLockEntered)
			{
				throw new TimeoutException (_lockReadTimeoutMessage);
			}

			try
			{
				foundIndex = FindEntry (key);
			}
			finally
			{
				_bucketsLock.ExitReadLock ();
			}

			if (foundIndex >= 0)
			{
				return _entries[foundIndex].Value;
			}

			TValue newValue;
			isLockEntered = _bucketsLock.TryEnterWriteLock (_lockTimeoutMilliseconds);
			if (!isLockEntered)
			{
				throw new TimeoutException (_lockWriteTimeoutMessage);
			}

			int newIndex;
			try
			{
				// пока не был захвачен замок на запись, другой поток исполнения уже мог добавить такую же запись и мы получим дупликат.
				// поэтому после получения эксклюзивного доступа, проверяем наличие записи ещё раз
				foundIndex = FindEntry (key);
				if (foundIndex >= 0)
				{
					return _entries[foundIndex].Value;
				}

				// Тут мы делаем то, что не рекомендуется делать внутри эксклюзивно захваченной блокировки -
				// вызываем внешний компонент, который неизвестно что будет делать.
				// Надеемся он там отработает очень быстро и не вызовет повторного входа в наш заблокированный объект.
				newValue = _valueFactory.Invoke (key);

				// создаём массивы если нужно
				if (_buckets == null)
				{
					// начальная инициализация внутреннего хранилища
					var initialCapacity = PrimeNumber.MinValue;
					var numArray = new int[initialCapacity];
					for (var i = 0; i < numArray.Length; i++)
					{
						numArray[i] = -1;
					}

					_buckets = numArray;
					_entries = new Entry[initialCapacity];
				}

				// увеличиваем размер если надо
				if (_count >= _entries.Length)
				{
					var newCapacity = PrimeNumber.GetGreaterThanOrEqual (_count * 2);
					var newEntries = new Entry[newCapacity];
					Array.Copy (_entries, newEntries, _count);
					_buckets = CreateIndex (_entries, _count);

					// важно что _entries заменяется на копию, в которой содержится тоже, что было раньше
					// иначе будет сбой в методах, не использующих блокировку
					_entries = newEntries;
				}

				// создаём новую запись
				var hash = _comparer.GetHashCode (key) & int.MaxValue;
				var index = hash % _buckets.Length;
				newIndex = _count;
				_entries[newIndex].Hash = hash;
				_entries[newIndex].Next = _buckets[index];
				_entries[newIndex].Key = key;
				_entries[newIndex].Value = newValue;
				_buckets[index] = newIndex;

				// важно что _count увеличивается только тогда, когда полностью сформирована новая запись
				// иначе будет сбой в методах, не использующих блокировку
				_count++;
			}
			finally
			{
				_bucketsLock.ExitWriteLock ();
			}

			// уведомляем о добавлении
			this.CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue> (key, newValue), newIndex));
			_keyEnumerationProvider.OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, key, newIndex));
			_valueEnumerationProvider.OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, newValue, newIndex));

			return newValue;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct Entry
		{
			internal int Hash;
			internal int Next;
			internal TKey Key;
			internal TValue Value;
		}

		internal class KeyValuePairEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
		{
			private readonly CompetentDictionary<TKey, TValue> _dictionary;
			private readonly int _count; // кол-во элементов родительского класса запоминаем отдельно для обеспечения повторяемости проходов
			private int _index;
			private KeyValuePair<TKey, TValue> _current;

			internal KeyValuePairEnumerator (CompetentDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
				_count = dictionary._count;
				_current = default (KeyValuePair<TKey, TValue>);
			}

			/// <summary>
			/// Получает текущий элемент перечислителя.
			/// </summary>
			public KeyValuePair<TKey, TValue> Current => _current;

			object IEnumerator.Current
			{
				get
				{
					if ((_index == 0) || (_index == (_count + 1)))
					{
						throw new InvalidOperationException("Can not get current element of enumeration because it not started or already eneded.");
					}

					return _current;
				}
			}

			/// <summary>
			/// Возвращает перечислитель в исходное положение.
			/// </summary>
			public void Reset ()
			{
				_index = 0;
				_current = default (KeyValuePair<TKey, TValue>);
			}

			/// <summary>
			/// Перемещает перечислитель к следующему элементу строки.
			/// </summary>
			/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
			/// false, если перечислитель достиг конца.</returns>
			public bool MoveNext ()
			{
				if (_index < _count)
				{
					var entry = _dictionary._entries[_index]; // внимание, тут необходимо копирование всей структуры KeyValuePair а не ссылки на неё
					_current = new KeyValuePair<TKey, TValue> (entry.Key, entry.Value);
					_index++;
					return true;
				}

				_index = _count + 1;
				_current = default (KeyValuePair<TKey, TValue>);
				return false;
			}

			/// <summary>
			/// Ничего не делает.
			/// </summary>
			public void Dispose ()
			{
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		internal class KeyEnumerator : IEnumerator<TKey>
		{
			private readonly CompetentDictionary<TKey, TValue> _dictionary;
			private readonly int _count; // кол-во элементов внешнего класса запоминаем для обеспечения повторяемости проходов
			private int _index;
			private TKey _currentKey;

			internal KeyEnumerator(CompetentDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
				_count = dictionary._count;
				_currentKey = default(TKey);
			}

			/// <summary>
			/// Получает текущий элемент перечислителя.
			/// </summary>
			public TKey Current => _currentKey;

			object IEnumerator.Current
			{
				get
				{
					if ((_index == 0) || (_index == (_count + 1)))
					{
						throw new InvalidOperationException("Can not get current element of enumeration because it not started or already ended");
					}

					return _currentKey;
				}
			}

			/// <summary>
			/// Ничего не делает.
			/// </summary>
			public void Dispose ()
			{
			}

			/// <summary>
			/// Перемещает перечислитель к следующему элементу строки.
			/// </summary>
			/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
			/// false, если перечислитель достиг конца.</returns>
			public bool MoveNext ()
			{
				if (_index < _count)
				{
					var entry = _dictionary._entries[_index]; // внимание, тут осуществляется копирование всего объекта а не ссылки на него
					_currentKey = entry.Key;
					_index++;
					return true;
				}

				_index = _count + 1;
				_currentKey = default (TKey);
				return false;
			}

			void IEnumerator.Reset ()
			{
				_index = 0;
				_currentKey = default (TKey);
			}
		}

		internal class ValueEnumerator : IEnumerator<TValue>
		{
			private readonly CompetentDictionary<TKey, TValue> _dictionary;
			private readonly int _count; // кол-во элементов внешнего класса запоминаем для обеспечения повторяемости проходов
			private int _index;
			private TValue _currentValue;

			internal ValueEnumerator (CompetentDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
				_count = dictionary._count;
				_currentValue = default (TValue);
			}

			/// <summary>
			/// Получает текущий элемент перечислителя.
			/// </summary>
			public TValue Current => _currentValue;

			object IEnumerator.Current
			{
				get
				{
					if ((_index == 0) || (_index == (_count + 1)))
					{
						throw new InvalidOperationException("Can not get current element of enumeration because it not started or already ended");
					}

					return _currentValue;
				}
			}

			/// <summary>
			/// Ничего не делает.
			/// </summary>
			public void Dispose ()
			{
			}

			/// <summary>
			/// Перемещает перечислитель к следующему элементу строки.
			/// </summary>
			/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
			/// false, если перечислитель достиг конца.</returns>
			public bool MoveNext ()
			{
				if (_index < _count)
				{
					var entry = _dictionary._entries[_index]; // внимание, тут осуществляется копирование всей структуры, а не ссылки
					_currentValue = entry.Value;
					_index++;
					return true;
				}

				_index = _count + 1;
				_currentValue = default (TValue);
				return false;
			}

			void IEnumerator.Reset ()
			{
				_index = 0;
				_currentValue = default (TValue);
			}
		}

		// внутренние классы KeyEnumerationProvider и ValueEnumerationProvider совершенно не нужны
		// их методы GetEnumerator() должен реализовывать родительский класс
		// но компилятор не позволяет в одном классе реализовать несколько интерфейсов IEnumerable<>
		// по времени жизни и доступу к членам внутренний и внешний классы связываются 1:1
		internal sealed class KeyEnumerationProvider : IReadOnlyList<TKey>, IReadOnlyFiniteSet<TKey>, INotifyCollectionChanged, IDisposable
		{
			private readonly CompetentDictionary<TKey, TValue> _dictionary; // ссылка на внешний класс из внутреннего

			public KeyEnumerationProvider (CompetentDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
			}

			public event NotifyCollectionChangedEventHandler CollectionChanged;

			public int Count => _dictionary.Count;

			public TKey this[int index] => _dictionary._entries[index].Key;

			IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();

			public IEnumerator<TKey> GetEnumerator () => new KeyEnumerator (_dictionary);

			public void Dispose ()
			{
				CollectionChanged = null;
			}

			public bool Contains (TKey key) => _dictionary.ContainsKey (key);

			internal void OnCollectionChanged (NotifyCollectionChangedEventArgs args)
			{
				this.CollectionChanged?.Invoke (this, args);
			}
		}

		internal sealed class ValueEnumerationProvider : IReadOnlyList<TValue>, INotifyCollectionChanged, IDisposable
		{
			private readonly CompetentDictionary<TKey, TValue> _dictionary; // ссылка на внешний класс из внутреннего

			public ValueEnumerationProvider (CompetentDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
			}

			public event NotifyCollectionChangedEventHandler CollectionChanged;

			public int Count => _dictionary.Count;

			public TValue this[int index] => _dictionary._entries[index].Value;

			IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();

			public IEnumerator<TValue> GetEnumerator () => new ValueEnumerator (_dictionary);

			public void Dispose ()
			{
				CollectionChanged = null;
			}

			internal void OnCollectionChanged (NotifyCollectionChangedEventArgs args)
			{
				this.CollectionChanged?.Invoke (this, args);
			}
		}
	}
}
