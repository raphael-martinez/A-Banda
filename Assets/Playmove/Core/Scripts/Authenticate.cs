namespace Playmove.Core
{
    public class Authenticate
    {
        public bool IsValid()
        {
            return SopService.Authentication.TrueValidation;
        }

        public int SopAuthentication(string ret)
        {
            return SopService.Authentication.Authenticate(ret);
        }
    }
}