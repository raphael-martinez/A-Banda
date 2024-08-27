using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Playmove
{
    [AddComponentMenu("Event/PY Physics 2D Raycaster")]
    [RequireComponent(typeof(Camera))]
    public class PYPhysics2DRaycaster : PhysicsRaycaster
    {
        protected PYPhysics2DRaycaster()
        { }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (eventCamera == null)
                return;

            var ray = eventCamera.ScreenPointToRay(eventData.position);

            float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;

            var hits = Physics2D.RaycastAll(ray.origin, ray.direction, dist, finalEventMask);

            if (hits.Length != 0)
            {
                for (int b = 0, bmax = hits.Length; b < bmax; ++b)
                {
                    // Retirando calculo perspectiva para distância
                    Vector3 eventCameraPos = hits[b].transform.position;
                    eventCameraPos.z = eventCamera.transform.position.z;

                    var result = new RaycastResult
                    {
                        gameObject = hits[b].collider.gameObject,
                        module = this,
                        distance = Vector3.Distance(eventCameraPos, hits[b].transform.position),
                        worldPosition = hits[b].point,
                        worldNormal = hits[b].normal,
                        index = resultAppendList.Count,
                        //Retirando diferença de layers para calculo de clique em cena
                        //sortingLayer = sr != null ? sr.sortingLayerID : 0,
                        //sortingOrder = sr != null ? sr.sortingOrder : 0
                    };
                    resultAppendList.Add(result);
                }
            }
        }
    }
}