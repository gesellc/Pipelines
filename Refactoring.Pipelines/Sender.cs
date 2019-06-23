﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Refactoring.Pipelines
{
    public abstract class Sender<T> : ISender<T>
    {
        protected readonly List<IListener<T>> Listeners = new List<IListener<T>>();

        public abstract string Name { get; }

        IEnumerable<IGraphNode> ISender.Children =>
            Listeners;

        public IGraphNode Collector =>
            Listeners.OfType<CollectorPipe<T>>().SingleOrDefault();

        public void AddListener(IListener<T> listener) { Listeners.Add(listener); }

        public abstract IEnumerable<IGraphNode> Parents { get; }

        protected void _Send(T value)
        {
            foreach (var listener in Listeners)
            {
                listener.OnMessage(value);
            }
        }

        public FunctionPipe<T, TOutput> Process<TOutput>(Func<T, TOutput> func)
        {
            return new FunctionPipe<T, TOutput>(func, this);
        }
    }
}