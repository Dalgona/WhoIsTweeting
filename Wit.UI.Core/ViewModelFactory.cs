using System;

namespace Wit.UI.Core
{
    public class ViewModelFactory
    {
        private readonly IWindowManager _wm;

        public ViewModelFactory(IWindowManager wm)
        {
            _wm = wm;
        }

        public ViewModelBase Create<TVm>(params object[] args) where TVm : ViewModelBase
        {
            object[] args2 = new object[args.Length + 2];

            args2[0] = this;
            args2[1] = _wm;

            args.CopyTo(args2, 2);
            return Activator.CreateInstance(typeof(TVm), args2) as ViewModelBase;
        }
    }
}
