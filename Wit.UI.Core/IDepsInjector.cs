namespace Wit.UI.Core
{
    public interface IDepsInjector
    {
        void Register<TInterface, TConcrete>()
            where TInterface : class
            where TConcrete : class, TInterface, new();

        void Register<T>() where T : class, new();

        T Create<T>(params object[] ctorArgs) where T : class, new();

        TInterface GetInstance<TInterface>() where TInterface : class;

        void Reset();
    }
}
