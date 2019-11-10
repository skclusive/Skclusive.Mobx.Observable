using System;
using System.Timers;

namespace Skclusive.Mobx.Observable
{
    public class ExecutionPlan : IDisposable
    {
        private Timer planTimer;

        private Action planAction;

        bool isRepeatedPlan;

        private ExecutionPlan(int millisecondsDelay, Action planAction, bool isRepeatedPlan)
        {
            planTimer = new Timer(millisecondsDelay);

            planTimer.Elapsed += GenericTimerCallback;

            planTimer.Enabled = true;

            this.planAction = planAction;

            this.isRepeatedPlan = isRepeatedPlan;
        }

        public static ExecutionPlan Delay(int millisecondsDelay, Action planAction)
        {
            return new ExecutionPlan(millisecondsDelay, planAction, false);
        }

        public static ExecutionPlan Repeat(int millisecondsInterval, Action planAction)
        {
            return new ExecutionPlan(millisecondsInterval, planAction, true);
        }
        private void GenericTimerCallback(object sender, System.Timers.ElapsedEventArgs e)
        {
            planAction();
            if (!isRepeatedPlan)
            {
                Abort();
            }
        }

        public void Abort()
        {
            planTimer.Enabled = false;
            planTimer.Elapsed -= GenericTimerCallback;
        }

        public void Dispose()
        {
            if (planTimer != null)
            {
                Abort();
                planTimer.Dispose();
                planTimer = null;
            }
            else
            {
                throw new ObjectDisposedException(typeof(ExecutionPlan).Name);
            }
        }
    }
}
