using System;
using System.Collections.Generic;
using System.Linq;

namespace MetX.Standard.Library.Extensions
{
    /// <summary>Provides simple methods for retrieving tokens from a string.
    /// <para>A token is a piece of a delimited string. For instance in the string "this is a test" when " " (a space) is used as a delimiter, "this" is the first token and "test" is the last (4th) token.</para>
    /// <para>Asking for a token beyond the end of a string returns a blank string. Asking for the zeroth or a negative token returns a blank string.</para>
    /// </summary>
    public static class ForStrings
    {
        public static string RemoveBlankLines(this string target)
        {
            if (target.IsEmpty())
                return string.Empty;

            var result = target
                .Lines()
                .Where(l => l.Replace("\t", "").Trim().IsNotEmpty()).ToArray();

            return string.Join("\n", result);
        }

        public static string RemoveAll(this string target, string[] stringsToRemove)
        {
            if (target.IsEmpty())
                return string.Empty;
            if (stringsToRemove.IsEmpty())
                return target;
            return stringsToRemove
                .Aggregate(target, (current, stringToRemove) 
                    => current
                        .Replace(stringToRemove, string.Empty));
        }

        public static string RemoveAll(this string target, char[] charsToRemove)
        {
            if (target.IsEmpty())
                return string.Empty;
            if (charsToRemove.IsEmpty())
                return target;
            return charsToRemove
                .Aggregate(target, (current, charToRemove) 
                    => current
                        .Replace(charToRemove.ToString(), string.Empty));
        }

        public static string AsFormattedXml(this string target)
        {
            if (target.IsEmpty())
                return string.Empty;

            var result = target
                    .Replace("><", ">\n\t<")
                ;
            return result;
        }
        
        public static string BlankToNull(this string target)
        {
            return target == string.Empty ? null : target;
        }
        
        public static string ProperCase(this string target)
        {
            if (target.IsEmpty())
                return string.Empty;
            
            var result = string.Empty;
            var isFirst = true;
            char? previousLetter = null;

            // For each char
            foreach (var letter in target.Trim())
            {
                //  If first
                if (isFirst)
                {
                    //    upper case and add to output
                    result += char.ToUpper(letter);
                    isFirst = false;
                }
                //  Else If last was space and char isn't uppercase
                else if (previousLetter == ' ')
                {
                    // upper case and add to output
                    result += char.ToUpper(letter);
                }
                //  Else If char is already upper and previous was not
                else if (char.IsUpper(letter) && !char.IsUpper(previousLetter.Value))
                {
                    //    add space then char
                    result += ' ';
                    result += letter;
                }
                //  Else
                else
                {
                    //    add char
                    result += letter;
                }
                previousLetter = letter;
            }

            return result;
        }

        public static bool IsEmpty<T>(this IList<T> target)
        {
            return target == null || target.Count == 0;
        }

        public static bool IsNotEmpty<T>(this IList<T> target)
        {
            return target?.Count > 0;
        }

        public static bool IsEmpty(this string target)
        {
            return target == null || 0u >= (uint)target.Length;
        }

        public static int AsInteger(this string target, int defaultValue = 0)
        {
            if (target.IsEmpty())
                return defaultValue;

            return int.TryParse(target, out var integerValue) 
                ? integerValue 
                : defaultValue;
        }

        public static bool ThrowIfEmpty(this string target, string targetsName)
        {
            if (string.IsNullOrEmpty(target))
                throw new ArgumentException(targetsName);
            return false;
        }

        public static bool IsNotEmpty(this string target)
        {
            return !string.IsNullOrEmpty(target);
        }

        public static List<string> AsList(this string[] target)
        {
            return new(target);
        }

        public static List<string> Slice(this string target) { return target.LineList(StringSplitOptions.RemoveEmptyEntries); }

        public static List<string> Dice(this string target) { return target.AllTokens(" ", StringSplitOptions.RemoveEmptyEntries); }

        public static string[] Lines(this string target, StringSplitOptions options = StringSplitOptions.None)
        {
            if (string.IsNullOrEmpty(target)) 
                return new[] { string.Empty };
            
            return target
                .Replace("\r", string.Empty)
                .Split(new[] { '\n' }, options);
        }

        public static List<string> LineList(this string target, StringSplitOptions options = StringSplitOptions.None)
        {
            return target.Lines(options).AsList();
        }

        public static string[] Words(this string target, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            if (string.IsNullOrEmpty(target)) 
                return new[] { string.Empty };
            
            return target.Trim()
                .Split(new[] { ' ' }, options);
        }

        public static List<string> WordList(this string target, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return target.Words(options).AsList();
        }

        public static string Flatten<T>(this IList<T> target)
        {
            return target.IsEmpty() 
                ? string.Empty 
                : string.Join(Environment.NewLine, target);
        }

        public static string AsFilename(this string target, string suffix = "")
        {
            if (target.IsEmpty()) return string.Empty;
            target = target.Replace(new[] { ":", @"\", "/", "*", "\"", "?", "<", ">", "|" }, " ");
            if (suffix.IsNotEmpty())
                target += suffix;

            if (char.IsDigit(target[0]))
                target = "_" + target;

            while (target.Contains("  ")) 
                target = target.Replace("  ", " ");

            while (target.EndsWith("."))
                target = target.Substring(0, target.Length - 2);
            while (target.Contains(".."))
                target = target.Replace("..", ".");
            while (target.Contains("__"))
                target = target.Replace("__", "_");

            return target.ProperCase().Replace(" ", "");
        }

        public static string Left(this string target, int length)
        {
            if (string.IsNullOrEmpty(target) || length < 1) return string.Empty;
            if (target.Length <= length) return target;
            return target.Substring(0, length);
        }

        public static string Right(this string target, int length)
        {
            // 01234
            // ABCDE
            //    DE
            // "ABCDE".Right(2) == "DE"
            // "ABCDE".Right(0) == ""

            if (string.IsNullOrEmpty(target) || length < 1) 
                return string.Empty;

            return length >= target.Length 
                ? target 
                : target.Substring(target.Length - length);
        }

        public static string Mid(this string target, int startAt, int length)
        {
            if (string.IsNullOrEmpty(target) || length < 1 || startAt > target.Length || startAt > target.Length) return string.Empty;
            if (startAt + length > target.Length) return target.Substring(startAt);
            return target.Substring(startAt, length);
        }

        public static string Mid(this string target, int startAt)
        {
            if (string.IsNullOrEmpty(target) || startAt < 1 || startAt > target.Length) return string.Empty;
            return target.Substring(startAt);
        }

        public static string Replace(this string target, string[] strings, string replacement)
        {
            if (string.IsNullOrEmpty(target)) return string.Empty;
            if (strings == null || strings.Length == 0) return target;
            return strings.Aggregate(target, (current, s) => current.Replace(s, replacement));
        }

        public static void TransformAllNotEmpty(this IList<string> target, Func<string, int, string> func)
        {
            if (target.IsEmpty()) return;

            for (var i = 0; i < target.Count; i++)
            {
                var s = target[i];
                if (s.IsEmpty()) continue;
                target[i] = func(s, i);
            }
        }

        public static void TransformAllNotEmpty(this IList<string> target, Func<string, string> func)
        {
            if (target.IsEmpty()) return;
            for (var i = 0; i < target.Count; i++)
            {
                var s = target[i];
                if (s.IsEmpty()) continue;
                target[i] = func(s);
            }
        }
    }
}