using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.SaaS.Accelerator.Services.Helpers;
public static class SunwaptaExtensions
{
  // return the innermost exception's message
  public static string GetBaseMessage(this Exception ex)
  {
    while (ex.InnerException != null)
    {
      ex = ex.InnerException;
    }
    return ex.Message;
  }

  // convert .NET MailAddress to SendGrid EmailAddress
  public static EmailAddress AsSendGridEmailAddress(this MailAddress input)
  {
    return new EmailAddress(input.Address, input.DisplayName);
  }

  // convert a string to SendGrid EmailAddress
  public static EmailAddress AsSendGridEmailAddress(this string input)
  {
    return new EmailAddress(input);
  }


  /// <summary>
  ///   For an array of strings, join them.
  /// </summary>
  public static string JoinedAsString(this IEnumerable<string> array, string separator, Func<string, string> converter)
  {
    return array.AsEnumerable().Select(converter).JoinedAsString(separator, string.Empty, string.Empty);
  }


  /// <summary>
  ///   Not IsNullOrEmpty
  /// </summary>
  [DebuggerStepThrough]
  public static bool HasContent(this string input)
  {
    return !string.IsNullOrEmpty(input);
  }


  public static string JoinedAsString(this IEnumerable<string> array, string separator, bool skipBlanks)
  {
    return array.AsEnumerable()
      .Where(s => !skipBlanks || s.HasContent())
      .JoinedAsString(separator, string.Empty, string.Empty);
  }

  /// <summary>
  ///   For an enumeration of strings, join them.
  /// </summary>
  public static string JoinedAsString(this IEnumerable<string> list)
  {
    return JoinedAsString(list, string.Empty);
  }

  /// <summary>
  ///   For an enumeration of strings, join them.
  /// </summary>
  public static string JoinedAsString(this IEnumerable<string> list, string separator)
  {
    return list.JoinedAsString(separator, string.Empty, string.Empty);
  }

  /// <summary>
  ///   For an enumeration of strings, join them. Each item has itemLeft and itemRight added.
  /// </summary>
  public static string JoinedAsString(this IEnumerable<string> list, string separator, string itemLeft,
    string itemRight, bool skipBlanks = false)
  {
    if (list == null)
    {
      return string.Empty;
    }
    var list2 = list.ToList();
    return list2.Any()
      ? string.Join(separator, list2.Where(s => !skipBlanks || s.HasContent()).Select(s => itemLeft + s + itemRight).ToArray())
      : "";
  }

}
