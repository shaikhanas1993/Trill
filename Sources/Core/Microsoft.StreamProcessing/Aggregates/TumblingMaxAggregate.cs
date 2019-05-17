﻿// *********************************************************************
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License
// *********************************************************************
using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Microsoft.StreamProcessing.Internal;

namespace Microsoft.StreamProcessing.Aggregates
{
    internal class TumblingMaxAggregate<T> : IAggregate<T, MinMaxState<T>, T>
    {
        private static readonly long InvalidSyncTime = StreamEvent.MinSyncTime - 1;
        private readonly Comparison<T> comparer;

        public TumblingMaxAggregate() : this(ComparerExpression<T>.Default) { }

        public TumblingMaxAggregate(IComparerExpression<T> comparer)
        {
            Contract.Requires(comparer != null);
            this.comparer = comparer.GetCompareExpr().Compile();
        }

        public Expression<Func<MinMaxState<T>>> InitialState()
            => () => new MinMaxState<T> { currentTimestamp = InvalidSyncTime };

        public Expression<Func<MinMaxState<T>, long, T, MinMaxState<T>>> Accumulate()
            => (state, timestamp, input) => new MinMaxState<T> {
                currentTimestamp = timestamp,
                currentValue = (state.currentTimestamp == InvalidSyncTime || this.comparer(input, state.currentValue) > 0) ? input : state.currentValue };

        public Expression<Func<MinMaxState<T>, long, T, MinMaxState<T>>> Deaccumulate()
            => (state, timestamp, input) => state; // never invoked, hence not implemented

        public Expression<Func<MinMaxState<T>, MinMaxState<T>, MinMaxState<T>>> Difference()
            => (leftSet, rightSet) => leftSet; // never invoked, hence not implemented

        public Expression<Func<MinMaxState<T>, T>> ComputeResult() => state => state.currentValue;
    }
}