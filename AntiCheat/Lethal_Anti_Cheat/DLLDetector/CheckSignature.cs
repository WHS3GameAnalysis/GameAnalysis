using System;
using System.Security.Cryptography.X509Certificates;

namespace Lethal_Anti_Cheat.DLLDetector
{
    public static class CheckSignature
    {
        public static bool IsFileSigned(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                X509Certificate.CreateFromSignedFile(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
