using System;
using System.Collections.Generic;

namespace Playmove.Framework.Keyboards
{
    /// <summary>
    /// Keyboard configuration, like keys letter values, disposition 
    /// and keys action values. This can be used to translate texts
    /// in keyboard
    /// </summary>
    [Serializable]
    public class KeyboardConfiguration
    {
        /// <summary>
        /// Configuration for each page on keyboard
        /// </summary>
        [Serializable]
        public class Page
        {
            public List<string> Lines;
            public string NextPage;

            public Page()
            {
                Lines = new List<string>();
            }
        }

        public List<Page> Pages;
        public string Confirm;
        public string Cancel;
        public string Backspace;
        public string ClearAll;

        public KeyboardConfiguration()
        {
            Pages = new List<Page>();
        }
    }
}
