using UnityEngine;
using System.Collections;
using System;

namespace Playmove
{
    [Serializable]
    public class PYBundleVersion
    {
        public string Version { get; set; }
        public string CreationDate { get; set; }

        public bool IsReadable
        {
            get { return true; }
        }

        public PYBundleVersion()
        {
            Version = "-1.-1.-1.-1";
            CreationDate = "-1/-1/-1";
        }
        public PYBundleVersion(string version, string creationDate)
        {
            Version = version;
            CreationDate = creationDate;
        }

        public override string ToString()
        {
            return Version;
        }
    }
}