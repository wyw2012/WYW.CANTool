using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WYW.CAN
{
    [Serializable]
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CopyFrom(object source)
        {
            if (GetType() == source.GetType())
            {
                PropertyInfo[] properties = source.GetType().GetProperties();
                PropertyInfo[] properties2 = GetType().GetProperties();
                if (properties2.Length == properties.Length)
                {
                    for (int i = 0; i < properties2.Length; i++)
                    {
                        object value = properties[i].GetValue(source, null);
                        properties2[i].SetValue(this, value);
                    }
                }

                return;
            }

            PropertyInfo[] properties3 = source.GetType().GetProperties();
            PropertyInfo[] properties4 = GetType().GetProperties();
            for (int j = 0; j < properties4.Length; j++)
            {
                for (int k = 0; k < properties3.Length; k++)
                {
                    if (properties4[j].Name == properties3[k].Name && properties4[j].PropertyType == properties3[k].PropertyType)
                    {
                        object value2 = properties3[k].GetValue(source, null);
                        properties4[j].SetValue(this, value2);
                        break;
                    }
                }
            }
        }

        public void CopyTo(object target)
        {
            PropertyInfo[] properties = GetType().GetProperties();
            PropertyInfo[] properties2 = target.GetType().GetProperties();
            for (int i = 0; i < properties2.Length; i++)
            {
                for (int j = 0; j < properties.Length; j++)
                {
                    if (properties2[i].Name == properties[j].Name && properties2[i].PropertyType == properties[j].PropertyType)
                    {
                        object value = properties[j].GetValue(this, null);
                        properties2[i].SetValue(target, value);
                        break;
                    }
                }
            }
        }
    }
}
