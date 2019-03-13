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

        public ViewModelBase Create<TVm>() where TVm : ViewModelBase
        {
            return Activator.CreateInstance(typeof(TVm), this, _wm) as ViewModelBase;
        }
    }
}
