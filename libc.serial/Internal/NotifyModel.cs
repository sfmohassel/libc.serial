using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace libc.serial.Internal
{
    internal abstract class NotifyModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T field, T value,
            Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(selectorExpression);

            return true;
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            if (selectorExpression == null) throw new ArgumentNullException(nameof(selectorExpression));

            if (!(selectorExpression.Body is MemberExpression body))
                throw new ArgumentException(
                    "The body must be a member expression");

            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(body.Member.Name);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}