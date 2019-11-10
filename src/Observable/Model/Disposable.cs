using System;

namespace Skclusive.Mobx.Observable
{
    public class Disposable : IDisposable
    {
        public Disposable(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Action = action;
        }

        private Action Action { set; get; }

        private bool Diposed { set; get; }

        public void Dispose()
        {
            if (Diposed)
                throw new System.InvalidOperationException("Already Disposed");

            Action();

            Diposed = true;
        }
    }
}
