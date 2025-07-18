using System.Text;

namespace BlogSlugify2ConventionalSlugify.MVC.Page
{
    public class FriendlyUrlHelper
    {
        public static string GetFriendlyTitle(string title, bool remapToAscii = false, int maxLength = 80)
        {
            if (title == null)
            {
                return string.Empty;
            }

            int length = title.Length;
            bool flag = false;
            StringBuilder stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                char c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    stringBuilder.Append(c);
                    flag = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    stringBuilder.Append((char)(c | 0x20u));
                    flag = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' || c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!flag && stringBuilder.Length > 0)
                    {
                        stringBuilder.Append('-');
                        flag = true;
                    }
                }
                else if (c >= '\u0080')
                {
                    int length2 = stringBuilder.Length;
                    if (remapToAscii)
                    {
                        stringBuilder.Append(RemapInternationalCharToAscii(c));
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }

                    if (length2 != stringBuilder.Length)
                    {
                        flag = false;
                    }
                }

                if (stringBuilder.Length >= maxLength)
                {
                    break;
                }
            }

            if (flag || stringBuilder.Length > maxLength)
            {
                return stringBuilder.ToString().Substring(0, stringBuilder.Length - 1);
            }

            return stringBuilder.ToString();
        }

        private static string RemapInternationalCharToAscii(char character)
        {
            string value = character.ToString().ToLowerInvariant();
            if ("àåáâäãåąā".Contains(value))
            {
                return "a";
            }

            if ("èéêěëę".Contains(value))
            {
                return "e";
            }

            if ("ìíîïı".Contains(value))
            {
                return "i";
            }

            if ("òóôõöøőð".Contains(value))
            {
                return "o";
            }

            if ("ùúûüŭů".Contains(value))
            {
                return "u";
            }

            if ("çćčĉ".Contains(value))
            {
                return "c";
            }

            if ("żźž".Contains(value))
            {
                return "z";
            }

            if ("śşšŝ".Contains(value))
            {
                return "s";
            }

            if ("ñń".Contains(value))
            {
                return "n";
            }

            if ("ýÿ".Contains(value))
            {
                return "y";
            }

            if ("ğĝ".Contains(value))
            {
                return "g";
            }

            if ("ŕř".Contains(value))
            {
                return "r";
            }

            if ("ĺľł".Contains(value))
            {
                return "l";
            }

            if ("úů".Contains(value))
            {
                return "u";
            }

            if ("đď".Contains(value))
            {
                return "d";
            }

            return character switch
            {
                'ť' => "t",
                'ž' => "z",
                'ß' => "ss",
                'Þ' => "th",
                'ĥ' => "h",
                'ĵ' => "j",
                _ => string.Empty,
            };
        }

    }
}
