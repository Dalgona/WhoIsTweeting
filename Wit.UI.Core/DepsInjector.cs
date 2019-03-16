using System;
using System.Collections.Generic;
using System.Linq;

namespace Wit.UI.Core
{
    public class DepsInjector : IDepsInjector
    {
        #region Static Members

        private static readonly Lazy<DepsInjector> _defaultInstance = new Lazy<DepsInjector>(() => new DepsInjector());

        public static DepsInjector Default => _defaultInstance.Value;

        #endregion

        private readonly object _stateLock = new object();
        private readonly HashSet<Type> _targets = new HashSet<Type>();
        private readonly Dictionary<Type, Type> _typeMapping = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> _instMapping = new Dictionary<Type, object>();
        private readonly Dictionary<Type, HashSet<Type>> _depsGraph = new Dictionary<Type, HashSet<Type>>();
        private bool _isGraphStale = true;

        public void Register<TInterface, TConcrete>()
            where TInterface : class
            where TConcrete : class, TInterface, new()
        {
            Type iType = typeof(TInterface);
            Type cType = typeof(TConcrete);

            lock (_stateLock)
            {
                if (_typeMapping.ContainsKey(iType))
                {
                    throw new InvalidOperationException($"The interface {iType.Name} is already registered.");
                }
                else
                {
                    _targets.Add(cType);
                    _typeMapping.Add(iType, cType);
                    _isGraphStale = true;
                }
            }
        }

        public void Register<T>() where T : class, new()
        {
            lock (_stateLock)
            {
                _targets.Add(typeof(T));
                _isGraphStale = true;
            }
        }

        public T Create<T>(params object[] ctorArgs) where T : class, new()
        {
            Type type = typeof(T);
            Type baseType = type;

            lock (_stateLock)
            {
                while (!_targets.Contains(baseType))
                {
                    baseType = baseType.BaseType;

                    if (baseType == null)
                    {
                        throw new InvalidOperationException($"The type {type.Name} is not registered.");
                    }
                }
            }

            T obj;

            lock (_stateLock)
            {
                if (_isGraphStale)
                {
                    ReconstructDepsGraph();
                }

                obj = (T)GetInstance(type, baseType);
            }

            return obj;
        }

        public void Reset()
        {
            lock (_stateLock)
            {
                _targets.Clear();
                _typeMapping.Clear();
                _instMapping.Clear();
                _depsGraph.Clear();
            }
        }

        private void ReconstructDepsGraph()
        {
            _depsGraph.Clear();

            HashSet<Type> interfaceSet = new HashSet<Type>(_typeMapping.Keys);

            foreach (Type type in _targets)
            {
                var depends =
                    type.GetProperties()
                        .Select(p => p.PropertyType)
                        .Where(pt => pt.IsInterface && interfaceSet.Contains(pt));

                _depsGraph.Add(type, new HashSet<Type>(depends));
            }

            _isGraphStale = false;
        }

        private object GetInstance(Type instType, Type baseType)
        {
            if (_instMapping.TryGetValue(baseType, out object obj))
            {
                return obj;
            }
            else
            {
                _typeMapping.TryGetValue(instType, out Type mappedType);

                object newObj = Activator.CreateInstance(mappedType ?? instType);
                var props = instType.GetProperties();

                foreach (Type depType in _depsGraph[mappedType ?? baseType])
                {
                    object depObj = GetInstance(depType, depType);
                    var targetProps = props.Where(p => p.PropertyType == depType);

                    foreach (var prop in targetProps)
                    {
                        prop.SetValue(newObj, depObj);
                    }
                }

                _instMapping.Add(baseType, newObj);

                return newObj;
            }
        }
    }
}
