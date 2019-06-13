namespace NanoMite
{
    public static class MiteHelper
    {
        public static string GetSize(long size)
        {
            var postfix = "Bytes";
            var result = size;
            if (size >= 1073741824) //more than 1 GB
            {
                result = size / 1073741824;
                postfix = "GB";
            }
            else if (size >= 1048576) //more that 1 MB
            {
                result = size / 1048576;
                postfix = "MB";
            }
            else if (size >= 1024) //more that 1 KB
            {
                result = size / 1024;
                postfix = "KB";
            }

            return result.ToString("F1") + " " + postfix;
        }
    }
}