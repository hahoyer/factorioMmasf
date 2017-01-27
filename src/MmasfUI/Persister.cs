using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace MmasfUI
{
    sealed class Persister : DumpableObject
    {
        sealed class Member<T> : DumpableObject, IMember
        {
            readonly string Name;
            public readonly Action<T> Load;
            public readonly Func<T> Store;
            readonly IPersitenceHandler<T> Handler;

            public Member(string name, Action<T> load, Func<T> store, IPersitenceHandler<T> handler)
            {
                Name = name;
                Load = load;
                Store = store;
                Handler = handler;
            }

            void IMember.Load()
            {
                var value = Handler.Get(Name);
                if (value != null)
                    Load(value);
            }

            void IMember.Store() => Handler.Set(Name, Store());
        }

        interface IMember
        {
            void Load();
            void Store();
        }

        readonly IDictionary<string, IMember> Members = new ConcurrentDictionary<string, IMember>();

        readonly hw.Helper.File Handle;
        [EnableDump]
        string FileName => Handle.FullName;

        internal Persister(hw.Helper.File handle) { Handle = handle; }

        public void Register<T>(string name, Action<T> load, Func<T> store)
            =>
                Members.Add
                (
                    name,
                    new Member<T>(name, load, store, new FilePersistenceHandler<T>(FileName)));

        public void Load()
        {
            foreach (var member in Members)
                member.Value.Load();
        }

        public void Store()
        {
            foreach (var member in Members)
                member.Value.Store();
        }

        public void Store(string name) => Members[name].Store();
    }
}