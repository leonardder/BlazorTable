﻿using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace BlazorTable
{
    public partial class BooleanFilter<TableItem> : IFilter<TableItem>
    {
        [CascadingParameter(Name = "FilterManager")] public IFilterManager<TableItem> FilterManager { get; set; }

        [Inject] public ILogger<NumberFilter<TableItem>> Logger { get; set; }

        private BooleanCondition Condition { get; set; }

        public List<Type> FilterTypes => new List<Type>()
        {
            typeof(bool)
        };

        protected override void OnInitialized()
        {
            if (FilterTypes.Contains(FilterManager.Column.Type.GetNonNullableType()))
            {
                FilterManager.Filter = this;

                if (FilterManager.Column.Filter != null)
                {
                    var nodeType = FilterManager.Column.Filter.Body.NodeType;

                    if (FilterManager.Column.Filter.Body is BinaryExpression binaryExpression
                        && binaryExpression.NodeType == ExpressionType.AndAlso)
                    {
                        nodeType = binaryExpression.Right.NodeType;
                    }

                    switch (nodeType)
                    {
                        case ExpressionType.IsTrue:
                            Condition = BooleanCondition.True;
                            break;
                        case ExpressionType.IsFalse:
                            Condition = BooleanCondition.False;
                            break;
                        case ExpressionType.Equal:
                            Condition = BooleanCondition.IsNull;
                            break;
                        case ExpressionType.NotEqual:
                            Condition = BooleanCondition.IsNotNull;
                            break;
                    }
                }
            }
        }

        public void ApplyFilter()
        {
            switch (Condition)
            {
                case BooleanCondition.True:
                    FilterManager.Column.Filter = Expression.Lambda<Func<TableItem, bool>>(
                        Expression.AndAlso(
                            Expression.NotEqual(FilterManager.Column.Property.Body, Expression.Constant(null)),
                            Expression.IsTrue(Expression.Convert(FilterManager.Column.Property.Body, FilterManager.Column.Type.GetNonNullableType()))),
                        FilterManager.Column.Property.Parameters);
                    break;
                case BooleanCondition.False:
                    FilterManager.Column.Filter = Expression.Lambda<Func<TableItem, bool>>(
                        Expression.AndAlso(
                            Expression.NotEqual(FilterManager.Column.Property.Body, Expression.Constant(null)),
                            Expression.IsFalse(Expression.Convert(FilterManager.Column.Property.Body, FilterManager.Column.Type.GetNonNullableType()))),
                        FilterManager.Column.Property.Parameters);
                    break;
                case BooleanCondition.IsNull:
                    FilterManager.Column.Filter = Expression.Lambda<Func<TableItem, bool>>(
                        Expression.Equal(FilterManager.Column.Property.Body, Expression.Constant(null)),
                        FilterManager.Column.Property.Parameters);
                    break;
                case BooleanCondition.IsNotNull:
                    FilterManager.Column.Filter = Expression.Lambda<Func<TableItem, bool>>(
                        Expression.NotEqual(FilterManager.Column.Property.Body, Expression.Constant(null)),
                        FilterManager.Column.Property.Parameters);
                    break;
            }
        }
    }

    public enum BooleanCondition
    {
        [Description("True")]
        True,

        [Description("False")]
        False,

        [Description("Is null")]
        IsNull,

        [Description("Is not null")]
        IsNotNull
    }
}
