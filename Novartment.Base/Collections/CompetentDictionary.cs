using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Threading;

namespace Novartment.Base.Collections
{
	/// <summary>
	/// A thread-safe read-only dictionary that automatically creates entries when they are accessed
	/// and generates notifications when they are added.
	/// Entries are created using the specified value factory.
	/// </summary>
	/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
	/// <remarks>
	/// Peculiar properties:
	/// * the Keys and Values properties are provided as a list with access by position number without locks and with notification of additions;
	/// * never throws the KeyNotFoundException;
	/// * TryGetValue() always returns True;
	/// * The only method that returns real information about the existence of a record is ContainsKey ().
	/// Generates a CollectionChanged event of only one kind — NotifyCollectionChangedAction.Add.
	/// </remarks>
	public sealed class CompetentDictionary<TKey, TValue> :
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
		/// Initializes a new instance of the CompetentDictionary class that is empty
		/// and uses a specified value factory and key comparer.
		/// </summary>
		/// <param name="valueFactory">
		/// The function-factory for the entries of the dictionary.
		/// Should not access the actual dictionary.
		/// Should be fast and not contain any locks.
		/// </param>
		/// <param name="comparer">
		/// The comparer to be used when comparing keys. Specify null-reference to use default comparer for type TKey.
		/// </param>
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

		/// <summary>Occurs when an entry is added to the dictionary.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>Gets the number of entries contained in the dictionary.</summary>
		public int Count => _count;

		/// <summary>Gets a collection of dictionary keys.</summary>
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _keyEnumerationProvider;

		/// <summary>Gets a list of dictionary keys.</summary>
		public IReadOnlyList<TKey> Keys => _keyEnumerationProvider;

		/// <summary>Gets a collection of dictionary values.</summary>
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _valueEnumerationProvider;

		/// <summary>Gets a list of dictionary values.</summary>
		public IReadOnlyList<TValue> Values => _valueEnumerationProvider;

		/// <summary>Gets the element that has the specified key in the dictionary.</summary>
		/// <param name="key">The key to locate.</param>
		/// <returns>The element that has the specified key in the dictionary.</returns>
		public TValue this[TKey key]
		{
			get
			{
				if (key == null)
				{
					throw new ArgumentNullException (nameof (key));
				}

				Contract.EndContractBlock ();

				return FindOrCreateValue (key);
			}
		}

		/// <summary>Determines whether the dictionary contains the specified entry.</summary>
		/// <param name="keyValuePair">The KeyValuePair&lt;TKey, TValue&gt; structure to locate in the dictionary.</param>
		/// <returns>True if keyValuePair is found in the dictionary; otherwise, False</returns>
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

		/// <summary>Determines whether the read-only dictionary contains an element that has the specified key.</summary>
		/// <param name="key">The key to locate.</param>
		/// <returns>
		/// True if the read-only dictionary contains an element that has the specified key;
		/// otherwise, False.
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

		/// <summary>Gets the value that is associated with the specified key.</summary>
		/// <param name="key">The key to locate.</param>
		/// <param name="value">The value associated with the specified key.</param>
		/// <returns>True because entries are created if not exists.</returns>
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
		/// Returns an enumerator for the dictionary.
		/// </summary>
		/// <returns>An enumerator for the dictionary.</returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			return new KeyValuePairEnumerator (this);
		}

		/// <summary>
		/// Returns an enumerator for the dictionary.
		/// </summary>
		/// <returns>An enumerator for the dictionary.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Performs freeing and releasing resources.
		/// </summary>
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

		internal sealed class KeyValuePairEnumerator :
			IEnumerator<KeyValuePair<TKey, TValue>>
		{
			private readonly CompetentDictionary<TKey, TValue> _dictionary;
			private readonly int _count; // кол-во элементов родительского класса запоминаем отдельно для обеспечения повторяемости проходов
			private int _index;
			private KeyValuePair<TKey, TValue> _current;

			internal KeyValuePairEnumerator (CompetentDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
				_count = dictionary._count;
				_current = default;
			}

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			public KeyValuePair<TKey, TValue> Current => _current;

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			object IEnumerator.Current
			{
				get
				{
					if ((_index == 0) || (_index == (_count + 1)))
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it not started or already eneded.");
					}

					return _current;
				}
			}

			/// <summary>
			///  Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			public void Reset ()
			{
				_index = 0;
				_current = default;
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// True if the enumerator was successfully advanced to the next element;
			/// False if the enumerator has passed the end of the collection.
			/// </returns>
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
				_current = default;
				return false;
			}

			/// <summary>
			/// Does nothing.
			/// </summary>
			public void Dispose ()
			{
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		internal sealed class KeyEnumerator :
			IEnumerator<TKey>
		{
			private readonly CompetentDictionary<TKey, TValue> _dictionary;
			private readonly int _count; // кол-во элементов внешнего класса запоминаем для обеспечения повторяемости проходов
			private int _index;
			private TKey _currentKey;

			internal KeyEnumerator (CompetentDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
				_count = dictionary._count;
				_currentKey = default;
			}

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			public TKey Current => _currentKey;

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			object IEnumerator.Current
			{
				get
				{
					if ((_index == 0) || (_index == (_count + 1)))
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it not started or already ended");
					}

					return _currentKey;
				}
			}

			/// <summary>
			/// Does nothing.
			/// </summary>
			public void Dispose ()
			{
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// True if the enumerator was successfully advanced to the next element;
			/// False if the enumerator has passed the end of the collection.
			/// </returns>
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
				_currentKey = default;
				return false;
			}

			/// <summary>
			///  Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			void IEnumerator.Reset ()
			{
				_index = 0;
				_currentKey = default;
			}
		}

		internal sealed class ValueEnumerator :
			IEnumerator<TValue>
		{
			private readonly CompetentDictionary<TKey, TValue> _dictionary;
			private readonly int _count; // кол-во элементов внешнего класса запоминаем для обеспечения повторяемости проходов
			private int _index;
			private TValue _currentValue;

			internal ValueEnumerator (CompetentDictionary<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
				_count = dictionary._count;
				_currentValue = default;
			}

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			public TValue Current => _currentValue;

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			object IEnumerator.Current
			{
				get
				{
					if ((_index == 0) || (_index == (_count + 1)))
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it not started or already ended");
					}

					return _currentValue;
				}
			}

			/// <summary>
			/// Does nothing.
			/// </summary>
			public void Dispose ()
			{
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// True if the enumerator was successfully advanced to the next element;
			/// False if the enumerator has passed the end of the collection.
			/// </returns>
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
				_currentValue = default;
				return false;
			}

			/// <summary>
			///  Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			void IEnumerator.Reset ()
			{
				_index = 0;
				_currentValue = default;
			}
		}

		// внутренние классы KeyEnumerationProvider и ValueEnumerationProvider совершенно не нужны
		// их методы GetEnumerator() должен реализовывать родительский класс
		// но компилятор не позволяет в одном классе реализовать несколько интерфейсов IEnumerable<>
		// по времени жизни и доступу к членам внутренний и внешний классы связываются 1:1
		internal sealed class KeyEnumerationProvider :
			IReadOnlyList<TKey>,
			IReadOnlyFiniteSet<TKey>,
			INotifyCollectionChanged,
			IDisposable
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

		internal sealed class ValueEnumerationProvider :
			IReadOnlyList<TValue>,
			INotifyCollectionChanged,
			IDisposable
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
