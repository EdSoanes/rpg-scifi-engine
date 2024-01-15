using Rpg.Sys.Components;
using System.Collections;

namespace Rpg.Sys
{
    public class ConditionStore : IList<Condition>
    {
        private Graph? _graph;

        private readonly List<Condition> _store = new List<Condition>();

        public int Count => _store.Count;

        public bool IsReadOnly => false;

        public Condition this[int index]
        {
            get => _store[index];
            set => _store[index] = value;
        }

        public void Initialize(Graph graph) 
            => _graph = graph;

        public int IndexOf(Condition item)
            => _store.IndexOf(item);

        public void Insert(int index, Condition item)
        {
            _store.Insert(index, item);
            _graph!.Mods.Add(item.GetModifiers());
        }

        public void RemoveAt(int index)
        {
            var condition = _store[index];
            _store.RemoveAt(index);

            foreach (var mod in condition.GetModifiers())
                _graph!.Mods.Remove(mod);
        }

        public void Add(Condition item)
        {
            _store.Add(item);
            _graph!.Mods.Add(item.GetModifiers());
        }

        public void AddRange(IEnumerable<Condition> items)
        {
            _store.AddRange(items);
            _graph!.Mods.Add(items.SelectMany(x => x.GetModifiers()).ToArray());
        }

        public void Clear()
        {
            foreach (var condition in _store)
                foreach (var mod in condition.GetModifiers())
                    _graph!.Mods.Remove(mod);

            _store.Clear();
        }
         
        public bool Contains(Condition item)
            => _store.Contains(item);

        public void CopyTo(Condition[] array, int arrayIndex)
        {
            _store.CopyTo(array, arrayIndex);
            _graph!.Mods.Add(array.SelectMany(x => x.GetModifiers()).ToArray());
        }

        public bool Remove(string conditionName)
        {
            var condition = _store.FirstOrDefault(x => x.Name == conditionName);
            return condition != null
                ? Remove(condition)
                : false;
        }

        public bool Remove(Condition item)
        {
            _graph!.Mods.Expire(item.GetModifiers());
            return _store.Remove(item);
        }

        public IEnumerator<Condition> GetEnumerator()
            => _store.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
