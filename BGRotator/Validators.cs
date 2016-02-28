using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace BGRotator
{
    public class DirectoryExistsRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null)
            {
                string input = value as string;

                if (Directory.Exists(input))
                    return new ValidationResult(true, null);
            }

            return new ValidationResult(false, "Directory does not exist");
        }
    }
}
