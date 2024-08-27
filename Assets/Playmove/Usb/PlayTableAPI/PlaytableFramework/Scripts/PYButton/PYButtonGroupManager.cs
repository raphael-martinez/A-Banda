using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Playmove
{
    public class PYButtonGroupManager : MonoBehaviour
    {
        #region Singleton
        private static PYButtonGroupManager _instance;
        public static PYButtonGroupManager Instance
        {
            get
            {
                if (!_instance) _instance = FindObjectOfType<PYButtonGroupManager>();
                return _instance;
            }
        }
        #endregion

        public Dictionary<string, PYButtonGroup> Groups = new Dictionary<string, PYButtonGroup>();

        private string _group;

        public void OnDisable()
        {
            foreach (KeyValuePair<string, PYButtonGroup> item in Groups)
                Destroy(item.Value);
        }

        /// <summary>
        /// Verify group state
        /// </summary>
        /// <param name="buttonGroup"></param>
        /// <returns></returns>
        public bool IsEnable(string buttonGroup)
        {
            if (VerifyAddGroup(buttonGroup))
                return Groups[buttonGroup].Enabled;
            return false;
        }

        public void EnableAll()
        {
            foreach (string key in Groups.Keys.ToList())
                Groups[key].Enable();
        }

        public void DisableAll(float disableTime = 0)
        {
            foreach (string key in Groups.Keys.ToList())
                Groups[key].Disable(disableTime);
        }

        /// <summary>
        /// Disables all.
        /// </summary>
        /// <param name="group">Do not disable this</param>
        public void DisableAllButThis(string group)
        {
            if (!string.IsNullOrEmpty(group))
                VerifyAddGroup(group);

            foreach (string key in Groups.Keys.ToList())
                if (!string.IsNullOrEmpty(group) && key != group)
                    Groups[key].Disable();
        }

        /// <summary>
        /// Disable a button group
        /// </summary>
        /// <param name="buttonGroup"></param>
        public void DisableGroup(string buttonGroup, float disableTime = 0)
        {
            VerifyAddGroup(buttonGroup);
            Groups[buttonGroup].Disable(disableTime);
        }

        /// <summary>
        /// Enable a button group
        /// </summary>
        /// <param name="buttonGroup"></param>
        public void EnableGroup(string buttonGroup)
        {
            VerifyAddGroup(buttonGroup);
            if (!string.IsNullOrEmpty(buttonGroup))
                Groups[buttonGroup].Enable();
        }

        private bool VerifyAddGroup(string buttonGroup)
        {
            if (!Application.isPlaying)
                return false;

            if (string.IsNullOrEmpty(buttonGroup))
                buttonGroup = "Default";

            if (Groups.ContainsKey(buttonGroup))
                return true;
            else
            {
                PYButtonGroup group = gameObject.AddComponent<PYButtonGroup>();
                group.GroupName = buttonGroup;
                Groups.Add(buttonGroup, group);
                return true;
            }
        }
    }
}