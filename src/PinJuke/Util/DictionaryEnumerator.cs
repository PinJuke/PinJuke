using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Util
{
    /// <summary>
    /// Wraps a IDictionaryEnumerator of e.g. an OrderedDictionary which is not generic.
    /// </summary>
    class DictionaryEnumerator<TKey, TValue> : IEnumerator<(TKey, TValue)>
    {
        private readonly IDictionaryEnumerator dictionaryEnumerator;

        public DictionaryEnumerator(IDictionaryEnumerator dictionaryEnumerator)
        {
            this.dictionaryEnumerator = dictionaryEnumerator;
        }

        public (TKey, TValue) Current => ((TKey)dictionaryEnumerator.Key, (TValue)dictionaryEnumerator.Value!);

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return dictionaryEnumerator.MoveNext();
        }

        public void Reset()
        {
            dictionaryEnumerator.Reset();
        }

        public void Dispose()
        {
        }

    }
}
