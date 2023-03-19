using libc.translation;
using System.Reflection;

namespace libc.serial.Resources
{

  internal static class Tran
  {
    public static readonly ILocalizer Instance = new Localizer(
        new JsonLocalizationSource(
            Assembly.GetExecutingAssembly(),
            $"{typeof(Tran).Namespace}.tran.i18n.json", PropertyCaseSensitivity.CaseInsensitive
        )
    );
  }

}