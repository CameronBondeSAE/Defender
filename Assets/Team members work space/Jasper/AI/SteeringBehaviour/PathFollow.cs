using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jasper_AI
{
    public class PathFollow : MonoBehaviour
    {
        private int currentPoint;
        [SerializeField] private AIBase aiBase;
        private bool _following;
        private bool _random;

        public delegate void PathEnd();
        public event PathEnd OnPathEnd;

        public void StartFollowing(bool random = false)
        {
            if (random)
            {
                aiBase.patrolPoints = WaypointManager.Instance.GetUniqueWaypoints(aiBase.patrolPointsCount);
                _random = true;
            }
            Debug.Log($"{name} starting path follow");
            currentPoint = 0;
            _following = true;
            aiBase.MoveTo(aiBase.patrolPoints[currentPoint].position);
        }

        public void StopFollowing()
        {
            _following = false;
        }

        private void FixedUpdate()
        {
            if (!_following) return; 
            
            //if close enough to the current patrol point then move to the next one 
            if (!(Vector3.Distance(transform.position, aiBase.patrolPoints[currentPoint].position) < 1)) return;
            
            currentPoint++;

            if (currentPoint > aiBase.patrolPoints.Length)
            {
                if (_random)
                {
                    currentPoint = 0;
                    aiBase.patrolPoints = WaypointManager.Instance.GetUniqueWaypoints(aiBase.patrolPointsCount);
                }
                else
                {
                    _following = false;
                    OnPathEnd?.Invoke();
                }
            }
                
            aiBase.MoveTo(aiBase.patrolPoints[currentPoint].position);
        }
    }
}
