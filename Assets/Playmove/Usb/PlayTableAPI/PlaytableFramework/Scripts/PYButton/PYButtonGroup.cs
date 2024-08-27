using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class PYButtonGroup : MonoBehaviour
    {
        public PYButtonGroup(string groupName)
        {
            GroupName = groupName;
        }

        public PYButtonGroup(string groupName, bool enabled, float disableTime)
        {
            GroupName = groupName;
            Enabled = enabled;
            DisableTime = disableTime;
        }

        public string GroupName = "Default";
        public bool Enabled = true;
        public float DisableTime;

        private bool _routinePlaying;
        private IEnumerator _routine;

        private void Start()
        {
            _routine = Routine(0);
        }

        private void Update()
        {
            _routine.MoveNext();
        }

        public void Enable()
        {
            Enabled = true;
            DisableTime = 0;
        }

        public void Disable(float disableTime = 0)
        {
            Enabled = false;
            if (disableTime > DisableTime)
            {
                if (_routinePlaying)
                    DisableTime = disableTime;
                else
                    _routine = Routine(disableTime);
            }
        }

        private IEnumerator Routine(float disableTime)
        {
            DisableTime = disableTime;
            _routinePlaying = true;
            while (DisableTime > 0)
            {
                DisableTime -= Time.deltaTime;
                yield return null;
            }

            _routinePlaying = false;
            Enable();
            yield return null;
        }
    }
}