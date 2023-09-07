using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace DEClientInterface
{
    internal class EnumToItemsSource : MarkupExtension
    {
        private readonly Type _type;

        public EnumToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return (from x in _type.GetMembers().SelectMany((MemberInfo member) => member.GetCustomAttributes(typeof(DescriptionAttribute), inherit: true).Cast<DescriptionAttribute>())
                    select x.Description).ToList();
        }

        public static T GetValueFromDescription<T>(string description)
        {
            Type typeFromHandle = typeof(T);
            if (!typeFromHandle.IsEnum)
            {
                throw new InvalidOperationException();
            }
            FieldInfo[] fields = typeFromHandle.GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                if (Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
                {
                    if (descriptionAttribute.Description == description)
                    {
                        return (T)fieldInfo.GetValue(null);
                    }
                }
                else if (fieldInfo.Name == description)
                {
                    return (T)fieldInfo.GetValue(null);
                }
            }
            return default(T);
        }

        public static string GetDescriptionFromValue(Enum value)
        {
            return value.GetType().GetMember(value.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
        }
    }
}
