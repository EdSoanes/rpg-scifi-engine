//using Rpg.Sys.Components;
//using Rpg.Sys.Modifiers;
//using System.Collections;

//namespace Rpg.Sys
//{
//    public class ConditionStore : IList<Condition>
//    {
//        private Graph? _graph;

//        private readonly List<Condition> _store = new List<Condition>();

//        public int Count => _store.Count;

//        public bool IsReadOnly => false;

//        public Condition this[int index]
//        {
//            get => _store[index];
//            set => _store[index] = value;
//        }

//        public void Initialize(Graph graph) 
//            => _graph = graph;

//        public int IndexOf(Condition item)
//            => _store.IndexOf(item);

//        public void Insert(int index, Condition item)
//        {
//            _store.Insert(index, item);
//            _graph!.Mods.Add(item.GetModifiers());
//        }

//        public void RemoveAt(int index)
//        {
//            var condition = _store[index];
//            _store.RemoveAt(index);

//            foreach (var mod in condition.GetModifiers())
//                _graph!.Mods.Remove(mod);
//        }

//        public void Add(Condition item)
//        {
//            _store.Add(item);
//            _graph!.Mods.Add(item.GetModifiers());
//        }

//        public void AddRange(IEnumerable<Condition> items)
//        {
//            _store.AddRange(items);
//            _graph!.Mods.Add(items.SelectMany(x => x.GetModifiers()).ToArray());
//        }

//        public void Clear()
//        {
//            foreach (var condition in _store)
//                foreach (var mod in condition.GetModifiers())
//                    _graph!.Mods.Remove(mod);

//            _store.Clear();
//        }
         
//        public bool Contains(Condition item)
//            => _store.Contains(item);

//        public void CopyTo(Condition[] array, int arrayIndex)
//        {
//            _store.CopyTo(array, arrayIndex);
//            _graph!.Mods.Add(array.SelectMany(x => x.GetModifiers()).ToArray());
//        }

//        public bool Remove(string conditionName)
//        {
//            var condition = _store.FirstOrDefault(x => x.Name == conditionName);
//            return condition != null
//                ? Remove(condition)
//                : false;
//        }

//        public bool UpdateOnTurn(int newTurn)
//        {
//            var toRemove = new List<Condition>();
//            foreach (var condition in _store)
//            {
//                var expiry = condition.Duration.GetExpiry(_graph!.Turn);
//                if (expiry == ModifierExpiry.Remove || expiry == ModifierExpiry.Expired)
//                    toRemove.Add(condition);
//                else
//                    _graph!.Mods.Expire(condition.GetModifiers());
//            }

//            return Remove(toRemove.ToArray());
//        }

//        public bool Expire(string conditionName)
//        {
//            var condition = _store.FirstOrDefault(x => x.Name == conditionName);
//            if (condition != null)
//            {
//                condition.Duration.Expire(_graph!.Turn);
//                return _graph!.Mods.Expire(condition.GetModifiers());
//            }

//            return false;
//        }

//        public bool Remove(Condition condition)
//            => Remove(new[] { condition });

//        public bool Remove(params Condition[] items)
//        {
//            var res = false;

//            var mods = items.SelectMany(x => x.GetModifiers()).ToArray();
//            _graph!.Mods.Remove(mods);

//            foreach (var item in items)
//                res |= _store.Remove(item);

//            return res;
//        }

//        public IEnumerator<Condition> GetEnumerator()
//            => _store.GetEnumerator();

//        IEnumerator IEnumerable.GetEnumerator()
//            => GetEnumerator();
//    }
//}
