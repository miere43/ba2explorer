using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ba2Explorer
{
    public sealed class NotifyTaskCompletion<T> : INotifyPropertyChanged
    {
        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        public Task<T> Task { get; private set; }

        public T Result => Task.Status == TaskStatus.RanToCompletion ? Task.Result : default(T);

        public TaskStatus Status => Task.Status;

        public bool IsCompleted => Task.IsCompleted;

        public bool IsNotCompleted => !Task.IsCompleted;

        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

        public bool IsCanceled => Task.IsCanceled;

        public bool IsFaulted => Task.IsFaulted;

        public AggregateException Exception => Task.Exception;

        public Exception InnerException => Task.Exception == null ? null : Task.Exception.InnerException;

        public string ErrorMessage => InnerException == null ? null : InnerException.Message;

        #endregion

        public NotifyTaskCompletion(Task<T> task)
        {
            Task = task;
        }

        public void StartWatch()
        {
            if (!Task.IsCompleted)
            {
                var _ = WatchTaskAsync(Task);
            }

        }

        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                await task;
            }
            catch { }

            var pc = PropertyChanged;
            if (PropertyChanged == null)
                return;
            pc(this, new PropertyChangedEventArgs(nameof(Status)));
            pc(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            pc(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));

            if (task.IsCanceled)
                pc(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            else if (task.IsFaulted)
            {
                pc(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                pc(this, new PropertyChangedEventArgs(nameof(Exception)));
                pc(this, new PropertyChangedEventArgs(nameof(InnerException)));
                pc(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            else
            {
                pc(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
                pc(this, new PropertyChangedEventArgs(nameof(Result)));
            }
        }
    }
}
