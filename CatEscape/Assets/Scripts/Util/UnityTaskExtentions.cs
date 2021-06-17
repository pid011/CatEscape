using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CatEscape.Util
{
    public static class UnityTaskExtensions
    {
        public static TaskYieldInstruction AsCoroutine(this Task task)
        {
            if (task == null)
            {
                throw new NullReferenceException();
            }

            return new TaskYieldInstruction(task);
        }

        public static TaskYieldInstruction<TResult> AsCoroutine<TResult>(this Task<TResult> task)
        {
            if (task == null)
            {
                throw new NullReferenceException();
            }

            return new TaskYieldInstruction<TResult>(task);
        }
    }

    public class TaskYieldInstruction : CustomYieldInstruction
    {
        public TaskYieldInstruction(Task task)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        public Task Task { get; protected set; }

        public override bool keepWaiting => !Task.IsCompleted;
    }

    public class TaskYieldInstruction<TResult> : TaskYieldInstruction
    {
        public TaskYieldInstruction(Task<TResult> task) : base(task)
        {
        }

        public new Task<TResult> Task
        {
            get => base.Task as Task<TResult>;

            protected set => base.Task = value;
        }

        public TResult Result => Task.Result;
    }
}
