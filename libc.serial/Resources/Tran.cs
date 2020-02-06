using System.Reflection;
using libc.translation;
namespace libc.serial.Resources {
    internal static class Tran {
        public static readonly ILocalizer Instance = new Localizer(new LocalizationSource(Assembly.GetExecutingAssembly(), 
            $"{typeof(Tran).Namespace}.tran.i18n.json"));
    }
}