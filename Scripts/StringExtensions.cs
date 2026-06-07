namespace Smart_Branch_Switch.Scripts
{
    public static class StringExtensions
    {

        //Public static methods

        public static string FixToWins(this string pathToFix)
        {
            //Prepare the value to return
            string toReturn = pathToFix;

            //Replace bars
            string winPath = pathToFix.Replace("/", @"\");
            //Remove a ending "\" at the end of the Path, if is a path, and not a Drive root path, like "D:\"
            if (winPath.Length > 3 && winPath.EndsWith(@"\") == true)
                winPath = winPath.TrimEnd('\\');
            //Inform the result
            toReturn = winPath;

            //Return the value
            return toReturn;
        }

        public static string FixToUnix(this string pathToFix)
        {
            //Prepare the value to return
            string toReturn = pathToFix;

            //Replace bars
            string unixPath = pathToFix.Replace(@"\", "/");
            //Remove a ending "/" at the end of the Path, if is a path, and not the root path, like "/"
            if (unixPath.Length > 1 && unixPath.EndsWith("/") == true)
                unixPath = unixPath.TrimEnd('/');
            //Inform the result
            toReturn = unixPath;

            //Return the value
            return toReturn;
        }
    }
}
