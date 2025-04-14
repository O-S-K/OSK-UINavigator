using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityObject = UnityEngine.Object;

namespace OSK.UI
{
    public static class CanvasUtils
    {
        //  if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
        public static bool IsPointerOverUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }
            else
            {
                PointerEventData pe = new PointerEventData(EventSystem.current);
                pe.position = Input.mousePosition;
                List<RaycastResult> hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pe, hits);
                return hits.Count > 0;
            }
        }

        public static bool IsPointerOverUIObject()
        {
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Count > 0;
        }

        public static Vector3 FlexPosition(Canvas canvas, Transform pointTarget, Camera camUI)
        {
            Vector3 worldPosition;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(pointTarget.GetComponent<RectTransform>(),
                    pointTarget.position, canvas.worldCamera, out worldPosition);
            }
            else
            { 
                Vector3 screenPoint = camUI.WorldToScreenPoint(pointTarget.position);
                screenPoint.z = Mathf.Abs(camUI.transform.position.z);
                worldPosition = camUI.ScreenToWorldPoint(screenPoint);
            }
            return worldPosition;
        }

        public static Vector3 ConvertToUICameraSpace(Canvas canvas, Transform pointTarget, Camera mainCamera, Camera uiCamera)
        {
            Vector3 uiWorldPosition;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(pointTarget.GetComponent<RectTransform>(),
                    pointTarget.position, uiCamera, out uiWorldPosition);
            }
            else
            {
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(pointTarget.position);
                if (screenPoint.z < 0)
                {
                    Debug.LogWarning("Object is behind the main camera, unable to convert position.");
                    return Vector3.zero;
                }

                uiWorldPosition = uiCamera.ScreenToWorldPoint(screenPoint);
                uiWorldPosition.z = 0;
            }

            return uiWorldPosition;
        }

        public static Vector2 WorldToCanvasPosition(RectTransform canvas, Camera camera, Vector3 position)
        {
            //Vector position (percentage from 0 to 1) considering camera size.
            //For example (0,0) is lower left, middle is (0.5,0.5)
            Vector2 viewportPosition = camera.WorldToViewportPoint(position);

            //Calculate position considering our percentage, using our canvas size
            //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
            viewportPosition.x *= canvas.sizeDelta.x;
            viewportPosition.y *= canvas.sizeDelta.y;

            //The result is ready, but, this result is correct if canvas recttransform pivot is 0,0 - left lower corner.
            //But in reality its middle (0.5,0.5) by default, so we remove the amount considering cavnas rectransform pivot.
            //We could multiply with constant 0.5, but we will actually read the value, so if custom rect transform is passed(with custom pivot) , 
            //returned value will still be correct.

            viewportPosition.x -= canvas.sizeDelta.x * canvas.pivot.x;
            viewportPosition.y -= canvas.sizeDelta.y * canvas.pivot.y;
            return viewportPosition;
        }
 

        public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
        {
            Vector2 localPoint;
            Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin,
                from.rect.height * from.pivot.y + from.rect.yMin);
            Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);

            screenP += fromPivotDerivedOffset;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
            Vector2 pivotDerivedOffset = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin,
                to.rect.height * to.pivot.y + to.rect.yMin);
            return localPoint - pivotDerivedOffset;
        }

        public static Camera GetCanvasCamera(Transform canvasChild)
        {
            Canvas canvas = GetCanvas(canvasChild);
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                return canvas.worldCamera;
            }
            return null;
        }
 
        public static Canvas GetCanvas(Transform transform)
        {
            if (transform == null)
            {
                return null;
            }
            Canvas canvas = transform.GetComponent<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }
            return GetCanvas(transform.parent);
        }
        
        public static T GetOrAddComponent<T>(this UnityObject uo) where T : Component
        {
            return uo.GetComponent<T>() ?? uo.AddComponent<T>();
        }

        public static T GetComponent<T>(this UnityObject uo)
        {
            if (uo is GameObject)
            {
                return ((GameObject)uo).GetComponent<T>();
            }
            else if (uo is Component)
            {
                return ((Component)uo).GetComponent<T>();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        
        public static T AddComponent<T>(this UnityObject uo) where T : Component
        {
            if (uo is GameObject)
            {
                return ((GameObject)uo).AddComponent<T>();
            }
            else if (uo is Component)
            {
                return ((Component)uo).gameObject.AddComponent<T>();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}